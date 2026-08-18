namespace Jeninnet.FileQuery.Tests.ContractTests;

/// <summary>
/// Contract tests for <see cref="IFileQueryEngine"/>.
/// Validates interface stability, argument validation, and basic execution guarantees.
/// </summary>
[TestClass]
public class IFileQueryEngineContractTests {
    private readonly IFileQueryEngine _engine = FileQueryRuntime.Create();

    /// <summary>Tests Execute_NullQuery_ShouldThrow.</summary>
    [TestMethod]
    public void Execute_NullQuery_ShouldThrow() => TestAssertEx.Throws<ArgumentNullException>(() => _engine.Execute(null!));

    /// <summary>Tests Execute_NonExistentDirectory_ShouldThrowDirectoryNotFoundException.</summary>
    [TestMethod]
    public async Task ExecuteAsync_NullQuery_ShouldThrowAsync() => await TestAssertEx.ThrowsAsync<ArgumentNullException>(async () => {
        await foreach(var _ in _engine.ExecuteAsync(null!, TestContext.CancellationToken)) {
            /*
             * Empty loop body; the exception should be thrown before any iteration occurs.
             */
        }
    });

    /// <summary>Tests Execute_NonExistentDirectory_ShouldThrowDirectoryNotFoundException.</summary>
    [TestMethod]
    public void Execute_NonExistentDirectory_ShouldThrowDirectoryNotFoundException() {
        var query = new FileQuery(
            rootPath: Path.Combine(
                path1: Path.GetTempPath(),
                path2: Guid.NewGuid().ToString("n")
            ),
            new FileQueryOptions(new FileQueryOptionsConfig(new([])))
        );

        void Act() => _ = _engine.Execute(query).ToList();

        TestAssertEx.Throws<DirectoryNotFoundException>(Act);
    }

    /// <summary>Tests Execute_ShouldReturnEmpty_WhenExcludeAllPatternIsUsed.</summary>
    [TestMethod]
    public async Task ExecuteAsync_NonExistentDirectory_ShouldThrowDirectoryNotFoundExceptionAsync() {
        var query = new FileQuery(
            rootPath: Path.Combine(
                path1: Path.GetTempPath(),
                path2: Guid.NewGuid().ToString("n")
            ),
            new FileQueryOptions(new FileQueryOptionsConfig(new([])))
        );

        async Task ActAsync() {
            await foreach(var _ in _engine.ExecuteAsync(query, TestContext.CancellationToken)) {
                /*
                 * Empty loop body; the exception should be thrown before any iteration occurs.
                 */
            }
        }

        await TestAssertEx.ThrowsAsync<DirectoryNotFoundException>(ActAsync);
    }

    /// <summary>Tests Execute_ShouldReturnEmpty_WhenExcludeAllPatternIsUsed.</summary>
    [TestMethod]
    public async Task ExecuteAsync_ShouldRespectCancellationTokenAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt");

        var query = FileQuery.From(env.Root).Build();
        using var cts = new CancellationTokenSource();

        // Pre-cancel
        await cts.CancelAsync();

        await TestAssertEx.ThrowsAsync<OperationCanceledException>(async () => {
            await foreach(var _ in _engine.ExecuteAsync(query, cts.Token)) {
                /*
                 * Empty loop body; the exception should be thrown before any iteration occurs.
                 */
            }
        });
    }

    /// <summary>Tests Execute_ShouldReturnEmpty_WhenExcludeAllPatternIsUsed.</summary>
    [TestMethod]
    public void Execute_ShouldReturnEmpty_WhenExcludeAllPatternIsUsed() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt");

        // Use "**" which acts as "exclude everything" in this engine's default context (see README)
        var options = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new(Patterns: ["**"])));
        var query = new FileQuery(env.Root, options);

        var results = _engine.Execute(query).ToList();

        Assert.IsEmpty(results);
    }

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
