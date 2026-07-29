namespace Jeninnet.FileQuery.Tests.Unit.Traversal;

/// <summary>
/// Tests for breadth-first traversal strategy.
/// </summary>
[TestClass]
public sealed class BfsTraversalTests {
    /// <summary>
    /// Verifies that BFS traversal returns the same set of files as DFS traversal for the same query options.
    /// </summary>
    [TestMethod]
    public void Should_ReturnAllFilesIdenticalToDfs_When_BfsTraversalUsed() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "root.txt",
            "sub1/a.txt",
            "sub2/b.txt",
            "sub1/deep/c.txt"
        );

        var engine = FileQueryRuntime.Create();

        var bfsOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                RecurseSubdirectories: true,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.BreadthFirst)
            )
        );

        var dfsOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: bfsOptions.PatternInput,
                RecurseSubdirectories: bfsOptions.RecurseSubdirectories,
                MaxRecursionDepth: bfsOptions.MaxRecursionDepth,
                IgnoreInaccessible: bfsOptions.IgnoreInaccessible,
                PatternMatchingMode: bfsOptions.PatternMatchingMode,
                CaseSensitivity: bfsOptions.CaseSensitivity,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.DepthFirst)
            )
        );

        var bfsResults = engine.Execute(new(env.Root, bfsOptions)).ToList();
        var dfsResults = engine.Execute(new(env.Root, dfsOptions)).ToList();

        Assert.AreSequenceEqual(dfsResults, bfsResults, SequenceOrder.InAnyOrder, "BFS and DFS must return the same set of files.");

        Assert.HasCount(4, bfsResults);
    }

    /// <summary>
    /// Verifies that Should AppearRootFilesBeforeDeep When BfsTraversalUsed.
    /// </summary>
    [TestMethod]
    public void Should_AppearRootFilesBeforeDeep_When_BfsTraversalUsed() {
        using var env = new TestEnvironment();
        env.CreateFile("root.txt");
        env.CreateFile("sub/nested/deep.txt");

        var engine = FileQueryRuntime.Create();

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                RecurseSubdirectories: true,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.BreadthFirst)
            )
        );

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(2, results);

        var rootIndex = results.FindIndex(p => p.EndsWith("root.txt", StringComparison.Ordinal));
        var deepIndex = results.FindIndex(p => p.EndsWith("deep.txt", StringComparison.Ordinal));

        Assert.IsLessThan(
            deepIndex, rootIndex,
            $"BFS must return 'root.txt' (index {rootIndex}) before 'deep.txt' (index {deepIndex}).");
    }

    /// <summary>
    /// Verifies that Should RespectMaxRecursionDepth When BfsTraversalUsed.
    /// </summary>
    [TestMethod]
    public void Should_RespectMaxRecursionDepth_When_BfsTraversalUsed() {
        using var env = new TestEnvironment();
        env.CreateFile("root.txt");
        env.CreateFile("level1/a.txt");
        env.CreateFile("level1/level2/b.txt");

        var engine = FileQueryRuntime.Create();

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 1,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.BreadthFirst)
            )
        );

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(2, results,
            "MaxRecursionDepth = 1 must include depth 0 and 1 only.");

        results.Should().Contain(p => p.EndsWith("root.txt", StringComparison.Ordinal), "root.txt at depth 0 must be included.");
        results.Should().Contain(p => p.EndsWith("a.txt", StringComparison.Ordinal), "a.txt at depth 1 must be included.");
        results.Should().NotContain(p => p.EndsWith("b.txt", StringComparison.Ordinal), "b.txt at depth 2 must be excluded.");
    }

    /// <summary>
    /// Verifies that asynchronous BFS traversal produces the same results as synchronous BFS traversal.
    /// </summary>
    [TestMethod]
    public async Task BfsTraversal_AsyncMatchesSyncAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "sub/b.txt", "sub/deep/c.txt");

        var engine = FileQueryRuntime.Create();

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                RecurseSubdirectories: true,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.BreadthFirst)
            )
        );

        var syncResults = engine.Execute(new(env.Root, options)).Order().ToList();
        var asyncResults = await engine
            .ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
            .ToListAsync(TestContext.CancellationToken);
        asyncResults.Sort();

        Assert.AreSequenceEqual(syncResults, asyncResults, "BFS async must match BFS sync when both results are sorted.");
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test execution.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
