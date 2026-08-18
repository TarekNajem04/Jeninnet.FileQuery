//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
    /// before their children and sibling files keep filesystem enumeration order.
    /// </summary>
    /// <remarks>
    /// Sibling order is taken from the OS enumerator — it is not alphabetical and
    /// must not be hard-coded (Linux CI often differs from Windows).
    /// </remarks>
    [TestMethod]
    public void Execute_ResultOrdering_IsPreserved() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "sub/c.txt");

        var engine = FileQueryRuntime.Create();
        var query = engine.From(env.Root).Where("*.log", "*.tmp", "!**/*.txt", "!**/*.md").Build();

        var results = engine.Execute(query).ToList();

        TestAssertEx.HasCount(results, 3);

        var aIndex = results.FindIndex(static r => r.EndsWith("a.txt", StringComparison.Ordinal));
        var bIndex = results.FindIndex(static r => r.EndsWith("b.txt", StringComparison.Ordinal));
        var cIndex = results.FindIndex(
            static r => r.EndsWith(System.IO.Path.Combine("sub", "c.txt"), StringComparison.Ordinal)
        );

        Assert.IsGreaterThanOrEqualTo(0, aIndex);
        Assert.IsGreaterThanOrEqualTo(0, bIndex);
        Assert.IsGreaterThanOrEqualTo(0, cIndex);

        // BFS default: root files appear before descendants.
        Assert.IsLessThan(cIndex, aIndex);
        Assert.IsLessThan(cIndex, bIndex);

        // Sibling order must match native directory enumeration order.
        var rootSiblingOrder = Directory
            .EnumerateFileSystemEntries(env.Root)
            .Select(System.IO.Path.GetFileName)
            .Where(static name => name is "a.txt" or "b.txt")
            .ToList();

        Assert.HasCount(2, rootSiblingOrder);
        var expectedFirstSibling = rootSiblingOrder[0]!;
        var expectedSecondSibling = rootSiblingOrder[1]!;
        var firstSiblingIndex = expectedFirstSibling == "a.txt" ? aIndex : bIndex;
        var secondSiblingIndex = expectedSecondSibling == "a.txt" ? aIndex : bIndex;
        Assert.IsLessThan(secondSiblingIndex, firstSiblingIndex);
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
