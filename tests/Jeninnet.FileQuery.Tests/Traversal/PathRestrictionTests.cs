//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Traversal;

/// <summary>
/// Tests the root-relative allow-list used by diff-aware queries.
/// </summary>
[TestClass]
public sealed class PathRestrictionTests {
    /// <summary>
    /// Verifies that only the listed files are returned.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldReturnOnlyListedFiles() {
        using var environment = CreateEnvironment();

        var results = FileQuery.From(environment.Root)
            .RestrictToPaths(["b.log", "src/nested/d.cs"])
            .Execute()
            .ToList();

        AssertPaths(results, environment, "b.log", "src/nested/d.cs");
    }

    /// <summary>
    /// Verifies that the async path honours the allow-list.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [TestMethod]
    public async Task RestrictToPaths_ShouldReturnOnlyListedFilesAsync() {
        using var environment = CreateEnvironment();
        var cancellationToken = TestContext?.CancellationToken ?? CancellationToken.None;

        var results = new List<string>();

        await foreach(var path in FileQuery.From(environment.Root)
            .RestrictToPaths(["a.txt", "src/c.cs"])
            .ExecuteAsync(cancellationToken)) {
            results.Add(path);
        }

        AssertPaths(results, environment, "a.txt", "src/c.cs");
    }

    /// <summary>
    /// Verifies that the allow-list combines with pattern matching.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldCombineWithPatterns() {
        using var environment = CreateEnvironment();

        var results = FileQuery.From(environment.Root)
            .RestrictToPaths(["a.txt", "b.log"])
            .Where("**;!*.txt")
            .Execute()
            .ToList();

        AssertPaths(results, environment, "a.txt");
    }

    /// <summary>
    /// Verifies that separators and current-directory segments are normalized.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldNormalizeSeparators() {
        using var environment = CreateEnvironment();

        var results = FileQuery.From(environment.Root)
            .RestrictToPaths([@".\src\c.cs", "src/./nested/d.cs"])
            .Execute()
            .ToList();

        AssertPaths(results, environment, "src/c.cs", "src/nested/d.cs");
    }

    /// <summary>
    /// Verifies that an empty allow-list matches no files.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldMatchNothing_WhenListIsEmpty() {
        using var environment = CreateEnvironment();

        var results = FileQuery.From(environment.Root)
            .RestrictToPaths([])
            .Execute()
            .ToList();

        results.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that an absolute path is rejected when the query is built.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldThrow_WhenPathIsAbsolute() {
        using var environment = CreateEnvironment();
        var absolute = environment.Abs("a.txt");

        var exception = Assert.Throws<ArgumentException>(() =>
            FileQuery.From(environment.Root).RestrictToPaths([absolute]).Build());

        exception.Message.Should().Contain("relative");
    }

    /// <summary>
    /// Verifies that parent segments are rejected when the query is built.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldThrow_WhenPathContainsParentSegment() {
        using var environment = CreateEnvironment();

        var exception = Assert.Throws<ArgumentException>(() =>
            FileQuery.From(environment.Root).RestrictToPaths(["src/../a.txt"]).Build());

        exception.Message.Should().Contain("parent");
    }

    /// <summary>
    /// Verifies that case-sensitive matching does not fold listed paths.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldRespectCaseSensitivity() {
        using var environment = CreateEnvironment();

        var results = FileQuery.From(environment.Root)
            .IgnoreCase(false)
            .RestrictToPaths(["SRC/C.CS"])
            .Execute()
            .ToList();

        results.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that case-insensitive matching folds listed paths.
    /// </summary>
    [TestMethod]
    public void RestrictToPaths_ShouldIgnoreCase_WhenConfigured() {
        using var environment = CreateEnvironment();

        var results = FileQuery.From(environment.Root)
            .IgnoreCase()
            .RestrictToPaths(["SRC/C.CS"])
            .Execute()
            .ToList();

        AssertPaths(results, environment, "src/c.cs");
    }

    /// <summary>
    /// Verifies that the option can be supplied directly on the options contract.
    /// </summary>
    [TestMethod]
    public void IncludedPaths_ShouldRestrictQuery_WhenConfiguredDirectly() {
        using var environment = CreateEnvironment();

        var options = new FileQueryOptions(new FileQueryOptionsConfig(
            PatternInput: new PatternInput(Patterns: ["!**"])) {
            IncludedPaths = ["src/c.cs"]
        });

        var results = FileQueryRuntime.Create()
            .Execute(new FileQuery(environment.Root, options))
            .ToList();

        AssertPaths(results, environment, "src/c.cs");
    }

    /// <summary>
    /// Creates the file tree used by these tests.
    /// </summary>
    /// <returns>The initialized test environment.</returns>
    private static TestEnvironment CreateEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFiles(
            "a.txt",
            "b.log",
            "src/c.cs",
            "src/nested/d.cs",
            "bin/e.txt");

        return environment;
    }

    /// <summary>
    /// Asserts that the results contain exactly the expected absolute paths.
    /// </summary>
    /// <param name="results">The query results.</param>
    /// <param name="environment">The owning test environment.</param>
    /// <param name="relativePaths">The expected paths relative to the environment root.</param>
    private static void AssertPaths(
        IEnumerable<string> results,
        TestEnvironment environment,
        params string[] relativePaths
    ) {
        var expected = relativePaths.Select(path => Path.GetFullPath(environment.Abs(path))).ToList();
        results.Select(static path => Path.GetFullPath(path)).Should().BeEquivalentTo(expected);
    }

    /// <inheritdoc/>
    public TestContext? TestContext { get; set; }
}
