//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.GitIgnore;

/// <summary>
/// End-to-end tests for loading Git ignore files into a query.
/// </summary>
[TestClass]
public sealed class GitIgnoreFileIngestionTests {
    private static readonly string[] _rootOnly = ["a.txt", "src/d.txt"];
    private static readonly string[] _parentOnly = ["a.txt", "b.log"];
    private static readonly string[] _nestedScope = ["a.txt", "b.log", "other/b.tmp", "src/c.log", "src/d.txt"];
    private static readonly string[] _negationResult = ["a.txt", "important.log", "src/d.txt"];
    private static readonly string[] _nestedNegationResult = ["b.log", "src/c.log", "src/keep.txt"];
    private static readonly string[] _buildIgnored = ["a.txt", "b.log", "src/c.log", "src/d.txt"];
    private static readonly string[] _anchoredFileResult = ["b.log", "src/a.txt", "src/c.log", "src/d.txt"];
    private static readonly string[] _gitVectorResult = ["a.txt", "b.log", "doc/other.txt", "frotz/file.txt", "other/doc/frotz/file.txt", "src/c.log", "src/d.txt"];

    /// <summary>
    /// The root <c>.gitignore</c> excludes files at every depth.
    /// </summary>
    [TestMethod]
    public void RootIgnoreFile_ExcludesMatchingFiles() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "*.log\n");

        QueryFiles(environment).Should().Equal(_rootOnly);
    }

    /// <summary>
    /// A nested ignore file only affects its subtree.
    /// </summary>
    [TestMethod]
    public void NestedIgnoreFile_IsScopedToItsDirectory() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile("src/.gitignore", "*.tmp\n");
        environment.CreateFiles("src/a.tmp", "other/b.tmp");

        QueryFiles(environment).Should().Equal(_nestedScope);
    }

    /// <summary>
    /// Deeper negations override earlier rules.
    /// </summary>
    [TestMethod]
    public void Negation_UsesLastMatchWins() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "*.log\n!important.log\n");
        environment.CreateFile("important.log", "keep");

        QueryFiles(environment).Should().Equal(_negationResult);
    }

    /// <summary>
    /// A deeper ignore file can re-include a file excluded by a parent rule.
    /// </summary>
    [TestMethod]
    public void NestedNegation_OverridesParentRule() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "*.txt\n");
        environment.CreateFile("src/.gitignore", "!keep.txt\n");
        environment.CreateFile("src/keep.txt", "included");
        environment.CreateFile("src/other.txt", "excluded");

        QueryFiles(environment).Should().Equal(_nestedNegationResult);
    }

    /// <summary>
    /// Directory-only rules prune the directory subtree.
    /// </summary>
    [TestMethod]
    public void DirectoryOnlyRule_PrunesSubtree() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "build/\n");
        environment.CreateFiles("build/output.txt", "build/nested/result.txt");

        QueryFiles(environment).Should().Equal(_buildIgnored);
    }

    /// <summary>
    /// Escaped comment/negation characters are treated as literals.
    /// </summary>
    [TestMethod]
    public void EscapedCharacters_AreLiterals() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "\\#literal.txt\n\\!important.txt\n");
        environment.CreateFiles("#literal.txt", "!important.txt");

        QueryFiles(environment).Should().Equal(_buildIgnored);
    }

    /// <summary>
    /// Unescaped trailing spaces are ignored, matching Git.
    /// </summary>
    [TestMethod]
    public void TrailingSpaces_AreTrimmed() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "trailing.txt   \n");
        environment.CreateFile("trailing.txt", "ignored");

        QueryFiles(environment).Should().Equal(_buildIgnored);
    }

    /// <summary>
    /// <c>.git/info/exclude</c> is loaded for a repository-root query.
    /// </summary>
    [TestMethod]
    public void InfoExclude_IsLoaded() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".git/info/exclude", "*.bak\n");
        environment.CreateFile("backup.bak", "ignored");

        QueryFiles(environment).Should().Equal(_buildIgnored);
    }

    /// <summary>
    /// An explicitly supplied global excludes file is loaded.
    /// </summary>
    [TestMethod]
    public void GlobalExcludesFile_IsLoaded() {
        using var environment = CreateRootEnvironment();
        using var global = new TestEnvironment();
        global.CreateFile("global-ignore", "*.tmp\n");
        environment.CreateFiles("scratch.tmp", "src/nested.tmp");

        var files = FileQuery.From(environment.Root)
            .UseGitIgnoreFiles(new GitIgnoreOptions { GlobalExcludesFile = global.Abs("global-ignore") })
            .Execute(TestContext.CancellationToken)
            .Select(path => Relative(environment.Root, path))
            .Where(static path => !IsIgnoreMetadata(path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        files.Should().Equal(_buildIgnored);
    }

    /// <summary>
    /// Nested ignore files inside an ignored directory are not read, matching Git.
    /// </summary>
    [TestMethod]
    public void NestedIgnoreFiles_AreNotReadInsideIgnoredDirectory() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "src/\n");
        environment.CreateFile("src/.gitignore", "!keep.txt\n");
        environment.CreateFile("src/keep.txt", "must stay ignored");

        QueryFiles(environment).Should().Equal(_parentOnly);
    }

    /// <summary>
    /// Unsupported lines in a real ignore file are skipped instead of failing the whole query.
    /// </summary>
    [TestMethod]
    public void UnsupportedRule_IsSkipped() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "[abc\n*.log\n");

        QueryFiles(environment).Should().Equal(_rootOnly);
    }

    /// <summary>
    /// The Git metadata directory is always excluded, even without an explicit rule.
    /// </summary>
    [TestMethod]
    public void UseGitIgnoreFiles_AlwaysExcludesGitDirectory() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".git/config", "repository metadata");

        var files = QueryFiles(environment);

        files.Should().NotContain(".git/config");
    }

    /// <summary>
    /// Git documentation vectors keep their anchored-path semantics.
    /// </summary>
    [TestMethod]
    public void GitDocumentationVector_AnchoredDirectoryRule() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "doc/frotz/\n");
        environment.CreateFiles(
            "doc/frotz/file.txt",
            "doc/other.txt",
            "frotz/file.txt",
            "other/doc/frotz/file.txt");

        QueryFiles(environment).Should().Equal(_gitVectorResult);
    }

    /// <summary>
    /// A leading slash anchors a file rule to the query root.
    /// </summary>
    [TestMethod]
    public void GitDocumentationVector_AnchoredFileRule() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "/a.txt\n");
        environment.CreateFile("src/a.txt", "nested");

        QueryFiles(environment).Should().Equal(_anchoredFileResult);
    }

    /// <summary>
    /// A leading <c>**&#47;</c> matches at every depth.
    /// </summary>
    [TestMethod]
    public void GitDocumentationVector_LeadingDoubleStar() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "**/foo\n");
        environment.CreateFiles("foo", "a/foo", "a/b/foo");

        QueryFiles(environment).Should().Equal(_buildIgnored);
    }

    /// <summary>
    /// A positive repository-level rule that targets an ancestor of the query root
    /// excludes the whole query root, matching Git semantics.
    /// </summary>
    [TestMethod]
    public void InfoExclude_AncestorDirectoryRule_ExcludesQueryRoot() {
        using var environment = new TestEnvironment();
        environment.CreateFile(".git/info/exclude", "ignored/\n");
        environment.CreateFile("ignored/sub/a.txt", "excluded");

        var queryRoot = Path.Combine(environment.Root, "ignored", "sub");

        var files = FileQuery.From(queryRoot)
                             .UseGitIgnoreFiles()
                             .Execute(TestContext.CancellationToken)
                             .Select(path => Relative(queryRoot, path))
                             .ToArray();

        files.Should().BeEmpty();
    }

    /// <summary>
    /// Creates a small repository-like tree.
    /// </summary>
    private static TestEnvironment CreateRootEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFiles("a.txt", "b.log", "src/c.log", "src/d.txt");
        return environment;
    }

    /// <summary>
    /// Runs an ignore-aware query and returns sorted relative paths without ignore metadata files.
    /// </summary>
    /// <param name="environment">The test environment.</param>
    private static string[] QueryFiles(TestEnvironment environment) =>
        [.. FileQuery.From(environment.Root)
            .UseGitIgnoreFiles()
            .Execute()
            .Select(path => Relative(environment.Root, path))
            .Where(static path => !IsIgnoreMetadata(path))
            .OrderBy(static path => path, StringComparer.Ordinal)];

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static bool IsIgnoreMetadata(string path) =>
        string.Equals(path, ".gitignore", StringComparison.Ordinal) ||
        path.EndsWith("/.gitignore", StringComparison.Ordinal) ||
        string.Equals(path, ".git/info/exclude", StringComparison.Ordinal);

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
