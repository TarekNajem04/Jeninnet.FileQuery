//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Directories;

/// <summary>
/// Tests for the opt-in directory result mode.
/// </summary>
[TestClass]
public sealed class DirectoryResultTests {
    private static readonly string[] _allResults =
    [
        "bin",
        "bin/tool.exe",
        "file.txt",
        "keep",
        "keep/keep.txt",
        "src",
        "src/a.cs",
        "src/nested",
        "src/nested/b.cs"
    ];

    private static readonly string[] _reincludedKeep =
    [
        "keep",
        "keep/keep.txt"
    ];

    /// <summary>
    /// The default path-only API still returns files only.
    /// </summary>
    [TestMethod]
    public void Execute_ByDefault_ReturnsFilesOnly() {
        using var environment = CreateEnvironment();

        var paths = FileQuery.From(environment.Root)
            .Execute(TestContext.CancellationToken)
            .Select(path => Relative(environment.Root, path))
            .ToArray();

        paths.Contains("src", StringComparer.Ordinal).Should().BeFalse();
        paths.Contains("src/nested", StringComparer.Ordinal).Should().BeFalse();
        paths.Contains("src/a.cs", StringComparer.Ordinal).Should().BeTrue();
    }

    /// <summary>
    /// Enabling directory results adds directories to the same traversal.
    /// </summary>
    [TestMethod]
    public void Execute_IncludeDirectories_ReturnsFilesAndDirectories() {
        using var environment = CreateEnvironment();

        var paths = FileQuery.From(environment.Root)
            .IncludeDirectories()
            .Execute(TestContext.CancellationToken)
            .Select(path => Relative(environment.Root, path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        paths.Should().BeEquivalentTo(_allResults);
    }

    /// <summary>
    /// Entry results mark directories without a trailing slash in the relative path.
    /// </summary>
    [TestMethod]
    public void ExecuteEntries_IncludeDirectories_MarksDirectoryEntries() {
        using var environment = CreateEnvironment();

        var entries = FileQuery.From(environment.Root)
            .IncludeDirectories()
            .ExecuteEntries(TestContext.CancellationToken)
            .ToArray();

        var directory = entries.Single(entry => entry.RelativePath == "src/nested");
        var file = entries.Single(entry => entry.RelativePath == "src/nested/b.cs");

        directory.IsDirectory.Should().BeTrue();
        file.IsDirectory.Should().BeFalse();
        directory.RelativePath.EndsWith('/').Should().BeFalse();
    }

    /// <summary>
    /// Directory-only patterns and negation decide directory results and prune excluded subtrees.
    /// </summary>
    [TestMethod]
    public void IncludeDirectories_DirectoryOnlyNegation_PrunesExcludedSubtrees() {
        using var environment = CreateEnvironment();

        var entries = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!keep/"])
            .IncludeDirectories()
            .ExecuteEntries(TestContext.CancellationToken)
            .Select(entry => entry.RelativePath)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        entries.Should().Equal(_reincludedKeep);
    }

    /// <summary>
    /// A directory-only exclusion removes the directory and its subtree.
    /// </summary>
    [TestMethod]
    public void IncludeDirectories_DirectoryOnlyExclusion_RemovesSubtree() {
        using var environment = CreateEnvironment();

        var entries = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["bin/"])
            .IncludeDirectories()
            .ExecuteEntries(TestContext.CancellationToken)
            .Select(entry => entry.RelativePath)
            .ToArray();

        entries.Contains("bin", StringComparer.Ordinal).Should().BeFalse();
        entries.Contains("bin/tool.exe", StringComparer.Ordinal).Should().BeFalse();
        entries.Contains("keep/keep.txt", StringComparer.Ordinal).Should().BeTrue();
    }

    /// <summary>
    /// Async entry enumeration returns the same directory results.
    /// </summary>
    [TestMethod]
    public async Task ExecuteEntriesAsync_IncludeDirectories_MatchesSyncResultsAsync() {
        using var environment = CreateEnvironment();
        var builder = FileQuery.From(environment.Root).IncludeDirectories();

        var sync = builder.ExecuteEntries(TestContext.CancellationToken)
            .Select(static entry => $"{entry.RelativePath}|{entry.IsDirectory}")
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var asyncResults = new List<string>();

        await foreach(var entry in builder.ExecuteEntriesAsync(TestContext.CancellationToken)) {
            asyncResults.Add($"{entry.RelativePath}|{entry.IsDirectory}");
        }

        asyncResults.OrderBy(static value => value, StringComparer.Ordinal).Should().Equal(sync);
    }

    /// <summary>
    /// Explicitly disabling directory results restores the default behavior.
    /// </summary>
    [TestMethod]
    public void IncludeDirectories_False_ReturnsFilesOnly() {
        using var environment = CreateEnvironment();

        var paths = FileQuery.From(environment.Root)
            .IncludeDirectories(false)
            .Execute(TestContext.CancellationToken)
            .Select(path => Relative(environment.Root, path))
            .ToArray();

        paths.Contains("src", StringComparer.Ordinal).Should().BeFalse();
        paths.Contains("src/a.cs", StringComparer.Ordinal).Should().BeTrue();
    }

    /// <summary>
    /// Creates a tree with files and nested directories.
    /// </summary>
    private static TestEnvironment CreateEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFiles(
            "file.txt",
            "src/a.cs",
            "src/nested/b.cs",
            "bin/tool.exe",
            "keep/keep.txt");

        return environment;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
