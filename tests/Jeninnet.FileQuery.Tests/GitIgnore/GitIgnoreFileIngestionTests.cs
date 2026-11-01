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
    private static readonly string[] ROOT_ONLY = ["a.txt", "src/d.txt"];
    private static readonly string[] PARENT_ONLY = ["a.txt", "b.log"];
    private static readonly string[] NESTED_SCOPE = ["a.txt", "b.log", "other/b.tmp", "src/c.log", "src/d.txt"];
    private static readonly string[] NEGATION_RESULT = ["a.txt", "important.log", "src/d.txt"];
    private static readonly string[] NESTED_NEGATION_RESULT = ["b.log", "src/c.log", "src/keep.txt"];
    private static readonly string[] BUILD_IGNORED = ["a.txt", "b.log", "src/c.log", "src/d.txt"];
    private static readonly string[] ANCHORED_FILE_RESULT = ["b.log", "src/a.txt", "src/c.log", "src/d.txt"];
    private static readonly string[] GIT_VECTOR_RESULT = ["a.txt", "b.log", "doc/other.txt", "frotz/file.txt", "src/c.log", "src/d.txt"];

    /// <summary>
    /// The root <c>.gitignore</c> excludes files at every depth.
    /// </summary>
    [TestMethod]
    public void RootIgnoreFile_ExcludesMatchingFiles() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "*.log\n");

        CollectionAssert.AreEqual(
            ROOT_ONLY,
            QueryFiles(environment));
    }

    /// <summary>
    /// A nested ignore file only affects its subtree.
    /// </summary>
    [TestMethod]
    public void NestedIgnoreFile_IsScopedToItsDirectory() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile("src/.gitignore", "*.tmp\n");
        environment.CreateFiles("src/a.tmp", "other/b.tmp");

        CollectionAssert.AreEqual(
            NESTED_SCOPE,
            QueryFiles(environment));
    }

    /// <summary>
    /// Deeper negations override earlier rules.
    /// </summary>
    [TestMethod]
    public void Negation_UsesLastMatchWins() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "*.log\n!important.log\n");
        environment.CreateFile("important.log", "keep");

        CollectionAssert.AreEqual(
            NEGATION_RESULT,
            QueryFiles(environment));
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

        CollectionAssert.AreEqual(
            NESTED_NEGATION_RESULT,
            QueryFiles(environment));
    }

    /// <summary>
    /// Directory-only rules prune the directory subtree.
    /// </summary>
    [TestMethod]
    public void DirectoryOnlyRule_PrunesSubtree() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "build/\n");
        environment.CreateFiles("build/output.txt", "build/nested/result.txt");

        CollectionAssert.AreEqual(
            BUILD_IGNORED,
            QueryFiles(environment));
    }

    /// <summary>
    /// Escaped comment/negation characters are treated as literals.
    /// </summary>
    [TestMethod]
    public void EscapedCharacters_AreLiterals() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "\\#literal.txt\n\\!important.txt\n");
        environment.CreateFiles("#literal.txt", "!important.txt");

        CollectionAssert.AreEqual(
            BUILD_IGNORED,
            QueryFiles(environment));
    }

    /// <summary>
    /// Unescaped trailing spaces are ignored, matching Git.
    /// </summary>
    [TestMethod]
    public void TrailingSpaces_AreTrimmed() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "trailing.txt   \n");
        environment.CreateFile("trailing.txt", "ignored");

        CollectionAssert.AreEqual(
            BUILD_IGNORED,
            QueryFiles(environment));
    }

    /// <summary>
    /// <c>.git/info/exclude</c> is loaded for a repository-root query.
    /// </summary>
    [TestMethod]
    public void InfoExclude_IsLoaded() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".git/info/exclude", "*.bak\n");
        environment.CreateFile("backup.bak", "ignored");

        CollectionAssert.AreEqual(
            BUILD_IGNORED,
            QueryFiles(environment));
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
            .Execute()
            .Select(path => Relative(environment.Root, path))
            .Where(static path => !IsIgnoreMetadata(path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(BUILD_IGNORED, files);
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

        CollectionAssert.AreEqual(
            PARENT_ONLY,
            QueryFiles(environment));
    }

    /// <summary>
    /// Git documentation vectors keep their anchored-path semantics.
    /// </summary>
    [TestMethod]
    public void GitDocumentationVector_AnchoredDirectoryRule() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "doc/frotz/\n");
        environment.CreateFiles("doc/frotz/file.txt", "doc/other.txt", "frotz/file.txt");

        CollectionAssert.AreEqual(
            GIT_VECTOR_RESULT,
            QueryFiles(environment));
    }

    /// <summary>
    /// A leading slash anchors a file rule to the query root.
    /// </summary>
    [TestMethod]
    public void GitDocumentationVector_AnchoredFileRule() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "/a.txt\n");
        environment.CreateFile("src/a.txt", "nested");

        CollectionAssert.AreEqual(
            ANCHORED_FILE_RESULT,
            QueryFiles(environment));
    }

    /// <summary>
    /// A leading <c>**&#47;</c> matches at every depth.
    /// </summary>
    [TestMethod]
    public void GitDocumentationVector_LeadingDoubleStar() {
        using var environment = CreateRootEnvironment();
        environment.CreateFile(".gitignore", "**/foo\n");
        environment.CreateFiles("foo", "a/foo", "a/b/foo");

        CollectionAssert.AreEqual(
            BUILD_IGNORED,
            QueryFiles(environment));
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
}
