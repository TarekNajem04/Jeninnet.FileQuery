namespace Jeninnet.FileQuery.Tests.ContractTests;

/// <summary>
/// Contract tests for <see cref="IFileQueryEngine"/>.
/// Validates interface stability, argument validation, and basic execution guarantees.
/// </summary>
[TestClass]
public class IFileQueryEngineContractTests {
    private readonly IFileQueryEngine _engine = FileQueryRuntime.Create();

    [TestMethod]
    public void Execute_NullQuery_ShouldThrow() => TestAssertEx.Throws<ArgumentNullException>(() => _engine.Execute(null!));

    [TestMethod]
    public async Task ExecuteAsync_NullQuery_ShouldThrowAsync() => await TestAssertEx.ThrowsAsync<ArgumentNullException>(async () => {
        await foreach(var _ in _engine.ExecuteAsync(null!, TestContext.CancellationToken)) {
            /*
             * Empty loop body; the exception should be thrown before any iteration occurs.
             */
        }
    });

    [TestMethod]
    public void Execute_NonExistentDirectory_ShouldThrowDirectoryNotFoundException() {
        var query = new FileQuery(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n")), new(new([])));

        void Act() => _ = _engine.Execute(query).ToList();

        TestAssertEx.Throws<DirectoryNotFoundException>(Act);
    }

    [TestMethod]
    public async Task ExecuteAsync_NonExistentDirectory_ShouldThrowDirectoryNotFoundExceptionAsync() {
        var query = new FileQuery(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n")), new(new([])));

        async Task ActAsync() {
            await foreach(var _ in _engine.ExecuteAsync(query, TestContext.CancellationToken)) {
                /*
                 * Empty loop body; the exception should be thrown before any iteration occurs.
                 */
            }
        }

        await TestAssertEx.ThrowsAsync<DirectoryNotFoundException>(ActAsync);
    }

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

    [TestMethod]
    public void Execute_ShouldReturnEmpty_WhenExcludeAllPatternIsUsed() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt");

        // Use "**" which acts as "exclude everything" in this engine's default context (see README)
        var options = new FileQueryOptions(patternInput: new(patterns: ["**"]));
        var query = new FileQuery(env.Root, options);

        var results = _engine.Execute(query).ToList();

        Assert.IsEmpty(results);
    }

    public TestContext TestContext { get; set; } = null!;
}
