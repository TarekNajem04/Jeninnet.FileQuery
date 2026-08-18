//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

/// <summary>
/// Tests behavior when the caller cancels asynchronous enumeration.
/// Ensures:
/// - OperationCanceledException is thrown
/// - No partial enumeration continues
/// - Cancellation is detected early
/// </summary>
[TestClass]
public class EnumerateFilesAsync_CancellationTests {
    /// <summary>
    /// Immediate cancellation before enumeration starts.
    /// </summary>
    [TestMethod]
    public async Task Should_ThrowImmediately_When_Canceled_Async() {
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

        Func<Task> actAsync = async () => {
            await foreach(var _ in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token)) {
                // Should never enter
            }
        };

        await actAsync.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Cancellation occurs in the middle of enumeration.
    /// </summary>
    [TestMethod]
    public async Task Should_Stop_When_CanceledDuringIteration_Async() {
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

        Func<Task> actAsync = async () => {
            await foreach(var path in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token)) {
                results.Add(path);

                // cancel after first item
                if(results.Count == 1) {
                    await cts.CancelAsync();
                }
            }
        };

        await actAsync.Should().ThrowAsync<OperationCanceledException>();
        // Only one item should have been returned
        results.Should().HaveCount(1);
    }

    /// <summary>
    /// Cancellation inside a deep recursive directory tree.
    /// </summary>
    [TestMethod]
    public async Task Should_Cancel_When_DuringDeepRecursion_Async() {
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

        Func<Task> actAsync = async () => {
            await foreach(var path in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token)) {
                count++;
                await cts.CancelAsync(); // Cancel immediately when first file found
            }
        };

        await actAsync.Should().ThrowAsync<OperationCanceledException>();
        Assert.AreEqual(1, count);
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
