namespace Jeninnet.FileQuery.Tests.Architecture;

/// <summary>
/// Contract tests for <see cref="IFileQueryEngine"/>.
/// Validates interface stability, argument validation, and basic execution guarantees.
/// </summary>
[TestClass]
public class IFileQueryEngineContractTests {
    private readonly IFileQueryEngine _engine = FileQueryRuntime.Create();

    /// <summary>
    /// Verifies that Should Throw When NullQueryExecuted.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_NullQueryExecuted() => ((Action)(() => _engine.Execute(null!))).Should().Throw<ArgumentNullException>();

    /// <summary>
    /// Verifies that ExecuteAsync NullQuery ShouldThrowAsync.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_NullQuery_ShouldThrowAsync() => await ((Func<Task>)(async () => {
        await foreach(var _ in _engine.ExecuteAsync(null!, TestContext.CancellationToken)) {
            /*
             * Empty loop body; the exception should be thrown before any iteration occurs.
             */
        }
    })).Should().ThrowAsync<ArgumentNullException>();

    /// <summary>
    /// Verifies that Should ThrowDirectoryNotFoundException When NonExistentDirectory.
    /// </summary>
    [TestMethod]
    public void Should_ThrowDirectoryNotFoundException_When_NonExistentDirectory() {
        var query = new FileQuery(
            rootPath: Path.Combine(
                path1: Path.GetTempPath(),
                path2: Guid.NewGuid().ToString("n")
            ),
            new FileQueryOptions(new FileQueryOptionsConfig(new([])))
        );

        Action act = () => _ = _engine.Execute(query).ToList();

        act.Should().Throw<DirectoryNotFoundException>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync NonExistentDirectory ShouldThrowDirectoryNotFoundExceptionAsync.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_NonExistentDirectory_ShouldThrowDirectoryNotFoundExceptionAsync() {
        var query = new FileQuery(
            rootPath: Path.Combine(
                path1: Path.GetTempPath(),
                path2: Guid.NewGuid().ToString("n")
            ),
            new FileQueryOptions(new FileQueryOptionsConfig(new([])))
        );

        Func<Task> actAsync = async () => {
            await foreach(var _ in _engine.ExecuteAsync(query, TestContext.CancellationToken)) {
                /*
                 * Empty loop body; the exception should be thrown before any iteration occurs.
                 */
            }
        };

        await actAsync.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync ShouldRespectCancellationTokenAsync.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_ShouldRespectCancellationTokenAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt");

        var query = FileQuery.From(env.Root).Build();
        using var cts = new CancellationTokenSource();

        // Pre-cancel
        await cts.CancelAsync();

        await ((Func<Task>)(async () => {
            await foreach(var _ in _engine.ExecuteAsync(query, cts.Token)) {
                /*
                 * Empty loop body; the exception should be thrown before any iteration occurs.
                 */
            }
        })).Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies that Should ReturnEmpty When ExcludeAllPatternUsed.
    /// </summary>
    [TestMethod]
    public void Should_ReturnEmpty_When_ExcludeAllPatternUsed() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt");

        // Use "**" which acts as "exclude everything" in this engine's default context (see README)
        var options = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new(Patterns: ["**"])));
        var query = new FileQuery(env.Root, options);

        var results = _engine.Execute(query).ToList();

        Assert.IsEmpty(results);
    }

    /// <summary>
    /// Gets or sets TestContext.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}

