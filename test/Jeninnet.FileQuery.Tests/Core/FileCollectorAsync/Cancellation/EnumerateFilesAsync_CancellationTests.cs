namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.Cancellation;

/// <summary>
/// Tests behavior when the caller cancels asynchronous enumeration.
/// Ensures:
/// - OperationCanceledException is thrown
/// - No partial enumeration continues
/// - Cancellation is detected early
/// </summary>
[TestClass]
public class EnumerateFilesAsync_CancellationTests
{
    /// <summary>
    /// Immediate cancellation before enumeration starts.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ShouldThrowImmediatelyWhenCanceledAsync()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: ["**/*"]
                )
            )
        );

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel BEFORE starting enumeration

        async Task ActAsync()
        {
            await foreach(var _ in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token))
            {
                // Should never enter
            }
        }

        await TestAssertEx.ThrowsAsync<OperationCanceledException>(ActAsync);
    }

    /// <summary>
    /// Cancellation occurs in the middle of enumeration.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ShouldStopWhenCanceledDuringIterationAsync()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("1.txt", "2.txt", "3.txt", "4.txt", "5.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: ["!*.txt"]
                )
            )
        );

        var results = new List<string>();

        using var cts = new CancellationTokenSource();

        async Task ActAsync()
        {
            await foreach(var path in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token))
            {
                results.Add(path);

                // cancel after first item
                if(results.Count == 1)
                {
                    await cts.CancelAsync();
                }
            }
        }

        await TestAssertEx.ThrowsAsync<OperationCanceledException>(ActAsync);
        // Only one item should have been returned
        TestAssertEx.HasCount(results, 1);
    }

    /// <summary>
    /// Cancellation inside a deep recursive directory tree.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ShouldCancelDuringDeepRecursionAsync()
    {
        using var env = new TestEnvironment();

        env.CreateFile("a/b/c/d/e/f/g/deep.txt");
        env.CreateFile("a/b/c/d/e/f/g/deep1.txt");
        env.CreateFile("a/b/c/d/e/f/g/deep1.txt");
        env.CreateFile("a/b/c/d/e/f/g/deep2.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        using var cts = new CancellationTokenSource();

        var count = 0;

        async Task ActAsync()
        {
            await foreach(var path in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token))
            {
                count++;
                await cts.CancelAsync(); // Cancel immediately when first file found
            }
        }

        await TestAssertEx.ThrowsAsync<OperationCanceledException>(ActAsync);
        Assert.AreEqual(1, count);
    }

    public TestContext TestContext { get; set; } = null!;
}
