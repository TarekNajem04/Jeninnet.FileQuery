//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Tests for breadth-first traversal strategy.
/// </summary>
[TestClass]
public sealed class BfsTraversalTests {
    // ======================================================================
    // BFS must return the same file set as DFS — only order may differ.
    // ======================================================================

    /// <summary>
    /// BFS and DFS must return identical file sets; only traversal order may differ.
    /// </summary>
    [TestMethod]
    public void BfsTraversal_ReturnsAllFiles_IdenticalToDfs() {
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
    /// BFS must return files closer to the root before files deeper in the tree.
    /// This is the defining property of breadth-first traversal.
    /// </summary>
    [TestMethod]
    public void BfsTraversal_RootFilesAppearBeforeDeepFiles() {
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

        var rootIndex = results.FindIndex(static p => p.EndsWith("root.txt", StringComparison.Ordinal));
        var deepIndex = results.FindIndex(static p => p.EndsWith("deep.txt", StringComparison.Ordinal));

        Assert.IsLessThan(
            deepIndex, rootIndex,
            $"BFS must return 'root.txt' (index {rootIndex}) before 'deep.txt' (index {deepIndex}).");
    }

    /// <summary>
    /// BFS must honour <see cref="FileQueryOptions.MaxRecursionDepth"/> in the same
    /// way DFS does.
    /// </summary>
    [TestMethod]
    public void BfsTraversal_RespectsMaxRecursionDepth() {
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

        Assert.Contains(static p => p.EndsWith("root.txt", StringComparison.Ordinal), results, "root.txt at depth 0 must be included.");
        Assert.Contains(static p => p.EndsWith("a.txt", StringComparison.Ordinal), results, "a.txt at depth 1 must be included.");
        Assert.DoesNotContain(static p => p.EndsWith("b.txt", StringComparison.Ordinal), results, "b.txt at depth 2 must be excluded.");
    }

    /// <summary>
    /// BFS async must return the same files as BFS sync.
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
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}

/// <summary>
/// Tests for UNC path normalization in <see cref="PathUtilities"/>.
/// </summary>
/// <remarks>
/// UNC paths have the form <c>\\server\share\path</c> on Windows.
/// After normalization they must produce <c>//server/share/path</c>.
/// The leading double-slash must be preserved; collapsing it to a single slash
/// produces an invalid path that matchers cannot evaluate correctly.
/// </remarks>
[TestClass]
public sealed class PathUtilitiesUncTests {

    // ======================================================================
    // IsUncRoot structural cases
    // (documented in PathUtilities.IsUncRoot XML remarks)
    // ======================================================================

    /// <summary>
    /// A UNC path without a trailing slash is normalized with forward slashes
    /// and the leading double-slash is preserved.
    /// </summary>
    [TestMethod]
    public void Normalize_UncPath_NoTrailingSlash_LeadingDoubleSlashPreserved() {
        const string input = @"\\server\share\file.txt";
        const string expected = "//server/share/file.txt";

        Assert.AreEqual(expected, PathUtilities.Normalize(input).Replace('\\', '/'),
            "Backslashes must become forward slashes and '//' must not be collapsed to '/'.");
    }

    /// <summary>
    /// A UNC root path with a trailing slash (<c>\\server\share\</c>) must preserve
    /// that trailing slash after normalization, because it identifies the root of the
    /// share — not a redundant separator.
    /// </summary>
    /// <remarks>
    /// <strong>Regression guard:</strong> the previous <c>IsUncRoot</c> implementation
    /// counted total slashes (== 3). <c>"//server/share/"</c> has 4 slashes, so
    /// <c>IsUncRoot</c> returned <see langword="false"/> and <c>TrimTrailingSlash</c>
    /// removed the final slash. This test failed before the structural-parsing fix.
    /// </remarks>
    /// <summary>Tests Normalize_UncRoot_WithTrailingSlash_SlashPreserved.</summary>
    [TestMethod]
    public void Normalize_UncRoot_WithTrailingSlash_SlashPreserved() {
        const string input = @"\\server\share\";
        const string expected = "//server/share/";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "A UNC root path's trailing slash must be preserved.");
    }

    /// <summary>
    /// A UNC root path without a trailing slash must not have one added.
    /// </summary>
    [TestMethod]
    public void Normalize_UncRoot_WithoutTrailingSlash_NoSlashAdded() {
        const string input = @"\\server\share";
        const string expected = "//server/share";

        Assert.AreEqual(expected, PathUtilities.Normalize(input));
    }

    /// <summary>
    /// A path below the UNC root (<c>//server/share/folder/</c>) must have its
    /// trailing slash trimmed — it is a non-root path.
    /// </summary>
    [TestMethod]
    public void Normalize_UncPath_BelowRoot_TrailingSlashTrimmed() {
        const string input = "//server/share/folder/";
        const string expected = "//server/share/folder";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "Trailing slash on a non-root UNC path must be trimmed.");
    }

    /// <summary>
    /// A deep UNC path (<c>//server/share/folder/file.txt</c>) must have
    /// all separators normalized and the double-slash preserved.
    /// </summary>
    [TestMethod]
    public void Normalize_DeepUncPath_NormalizedCorrectly() {
        const string input = @"\\server\share\project\src\Program.cs";
        const string expected = "//server/share/project/src/Program.cs";

        Assert.AreEqual(expected, PathUtilities.Normalize(input));
    }

    // ======================================================================
    // Duplicate internal slashes inside UNC paths
    // ======================================================================

    /// <summary>
    /// Internal duplicate slashes inside a UNC path must be collapsed while the
    /// leading <c>//</c> is preserved.
    /// </summary>
    [TestMethod]
    public void Normalize_UncPath_InternalDuplicateSlashes_Collapsed() {
        const string input = "//server//share//file.txt";
        const string expected = "//server/share/file.txt";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "Internal consecutive slashes must be collapsed; leading '//' must survive.");
    }

    /// <summary>
    /// When <c>trimTrailingSlash: false</c> is passed, a trailing slash on a
    /// non-root local path must be preserved.
    /// </summary>
    [TestMethod]
    public void Normalize_LocalPath_TrimTrailingSlashFalse_SlashPreserved() {
        const string input = @"C:\Users\Test\";
        const string expected = "C:/Users/Test/";

        Assert.AreEqual(expected, PathUtilities.Normalize(input, trimTrailingSlash: false),
            "trimTrailingSlash: false must preserve a trailing slash on non-root paths.");
    }

    /// <summary>
    /// When <c>trimTrailingSlash: false</c> is passed, a trailing slash on a
    /// non-root UNC path must be preserved.
    /// </summary>
    [TestMethod]
    public void Normalize_UncNonRootPath_TrimTrailingSlashFalse_SlashPreserved() {
        const string input = @"\\server\share\folder\";
        const string expected = "//server/share/folder/";

        Assert.AreEqual(expected, PathUtilities.Normalize(input, trimTrailingSlash: false),
            "trimTrailingSlash: false must preserve trailing slash on non-root UNC paths.");
    }

    /// <summary>
    /// The default value of <c>trimTrailingSlash</c> is <see langword="true"/>,
    /// so calling <c>Normalize</c> without the argument must trim trailing slashes
    /// from non-root paths — exactly as before the parameter was added.
    /// </summary>
    [TestMethod]
    public void Normalize_DefaultBehavior_TrimTrailingSlashIsTrue() {
        const string input = @"C:\Users\Test\";
        const string expected = "C:/Users/Test";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "Default behavior (trimTrailingSlash: true) must not be altered by adding the optional parameter.");
    }

    /// <summary>Tests Normalize_WindowsPath_BackslashesToForwardSlashes.</summary>
    [TestMethod]
    public void Normalize_WindowsPath_BackslashesToForwardSlashes() =>
        Assert.AreEqual(
            "C:/Users/Test/file.txt",
            PathUtilities.Normalize(@"C:\Users\Test\file.txt"));

    /// <summary>Tests Normalize_LocalPath_DuplicateSlashesCollapsed.</summary>
    [TestMethod]
    public void Normalize_LocalPath_DuplicateSlashesCollapsed() =>
        Assert.AreEqual(
            "C:/Users/Test/file.txt",
            PathUtilities.Normalize(@"C:\\Users//Test\\file.txt"));

    /// <summary>Tests Normalize_DriveRoot_TrailingSlashPreserved.</summary>
    [TestMethod]
    public void Normalize_DriveRoot_TrailingSlashPreserved() =>
        Assert.AreEqual("C:/", PathUtilities.Normalize("C:/"));

    /// <summary>Tests Normalize_NullOrEmpty_ThrowsArgumentException.</summary>
    [TestMethod]
    public void Normalize_NullOrEmpty_ThrowsArgumentException() {
        Assert.ThrowsExactly<ArgumentNullException>(static () => PathUtilities.Normalize(null));
        Assert.ThrowsExactly<ArgumentException>(static () => PathUtilities.Normalize(""));
    }
}
