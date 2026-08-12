namespace Jeninnet.FileQuery.Tests.Unit.Traversal;
/// <summary>
/// Engine-level tests verifying that traversal still produces correct results when
/// the relative path is composed in the reusable <see cref="RelativePathBuffer"/>
/// instead of an allocation per entry.
/// </summary>
[TestClass]
public class RelativePathBufferEngineTests {
    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Verifies that shallow and root-relative files match with their full paths
    /// as results.
    /// </summary>
    [TestMethod]
    public void Execute_ShallowAndRootRelative_FilesMatchWithFullPaths() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.log", "sub/c.txt");

        var engine = FileQueryRuntime.Create();
        var query = engine.From(env.Root).Where("*.log", "*.tmp", "!**/*.txt", "!**/*.md").Build();

        var results = engine.Execute(query).ToList();

        TestAssertEx.HasCount(results, 2);
        Assert.Contains(static x => x.EndsWith("a.txt", StringComparison.Ordinal), results);
        Assert.Contains(static x => x.EndsWith(System.IO.Path.Combine("sub", "c.txt"), StringComparison.Ordinal), results);

        foreach(var result in results) {
            Assert.IsTrue(result.StartsWith(env.Root, StringComparison.Ordinal));
            Assert.IsTrue(File.Exists(result), $"Result '{result}' must be a real file path.");
        }
    }

    /// <summary>
    /// Verifies that a deeply nested path long enough to grow the buffer past its
    /// initial capacity still matches with a correct full path.
    /// </summary>
    [TestMethod]
    public void Execute_DeeplyNestedPathRequiringBufferGrowth_MatchesCorrectly() {
        using var env = new TestEnvironment();

        var segments = Enumerable.Range(0, 12)
                                 .Select(static i => $"directory-with-a-long-name-{i:00}")
                                 .ToArray();
        var relativeDeep = System.IO.Path.Combine(
            [.. segments, "deeply-nested-file.txt"]
        );

        Assert.IsGreaterThan(256, relativeDeep.Length, "Test must exercise buffer growth.");

        env.CreateFiles(relativeDeep, "root-level.txt");

        var engine = FileQueryRuntime.Create();
        var query = engine.From(env.Root).Where("*.log", "*.tmp", "!**/*.txt", "!**/*.md").Build();

        var results = engine.Execute(query).ToList();

        TestAssertEx.HasCount(results, 2);
        Assert.Contains(
            x => x.EndsWith(System.IO.Path.Combine(relativeDeep), StringComparison.Ordinal),
            results
        );
        Assert.Contains(static x => x.EndsWith("root-level.txt", StringComparison.Ordinal), results);
        Assert.IsTrue(File.Exists(results[0]));
    }

    /// <summary>
    /// Verifies that matching and non-matching entries are separated exactly and
    /// that non-matching entries never appear in the results.
    /// </summary>
    [TestMethod]
    public void Execute_MatchingAndNonMatchingEntries_AreSeparatedExactly() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "keep.txt",
            "keep.md",
            "drop.log",
            "drop.tmp",
            "sub/keep.txt",
            "sub/drop.log"
        );

        var engine = FileQueryRuntime.Create();
        var query = engine
            .From(env.Root)
            .Where("*.log", "*.tmp", "!**/*.txt", "!**/*.md")
            .Build();

        var results = engine.Execute(query).ToList();

        TestAssertEx.HasCount(results, 3);
        Assert.Contains(static x => x.EndsWith("keep.txt", StringComparison.Ordinal), results);
        Assert.Contains(static x => x.EndsWith("keep.md", StringComparison.Ordinal), results);
        Assert.Contains(static x => x.EndsWith(System.IO.Path.Combine("sub", "keep.txt"), StringComparison.Ordinal), results);
        Assert.DoesNotContain(static x => x.EndsWith("drop.log", StringComparison.Ordinal), results);
        Assert.DoesNotContain(static x => x.EndsWith("drop.tmp", StringComparison.Ordinal), results);
    }

    /// <summary>
    /// Verifies that result ordering is preserved: parent directories are visited
    /// before their children and files keep their enumeration order.
    /// </summary>
    [TestMethod]
    public void Execute_ResultOrdering_IsPreserved() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "sub/c.txt");

        var engine = FileQueryRuntime.Create();
        var query = engine.From(env.Root).Where("*.log", "*.tmp", "!**/*.txt", "!**/*.md").Build();

        var results = engine.Execute(query).ToList();

        TestAssertEx.HasCount(results, 3);
        Assert.IsTrue(results[0].EndsWith("a.txt", StringComparison.Ordinal));
        Assert.IsTrue(results[1].EndsWith("b.txt", StringComparison.Ordinal));
        Assert.IsTrue(results[2].EndsWith(System.IO.Path.Combine("sub", "c.txt"), StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that the async traversal produces identical results to the sync
    /// traversal when paths are composed in the reusable buffer.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_DeepPaths_MatchSyncResultsAsync() {
        using var env = new TestEnvironment();

        var segments = Enumerable.Range(0, 10)
                                 .Select(static i => $"deep-directory-{i:00}")
                                 .ToArray();
        var relativeDeep = System.IO.Path.Combine([.. segments, "async-file.txt"]);
        env.CreateFiles(relativeDeep, "root.txt");

        var engine = FileQueryRuntime.Create();
        var query = engine.From(env.Root).Where("*.log", "*.tmp", "!**/*.txt", "!**/*.md").Build();

        var syncResults = engine.Execute(query).Order().ToList();
        var asyncResults = await engine.ExecuteAsync(query, TestContext.CancellationToken)
                                       .ToListAsync(TestContext.CancellationToken);

        Assert.AreSequenceEqual(syncResults, [.. asyncResults.Order()]);
        Assert.HasCount(2, asyncResults);
    }
}
