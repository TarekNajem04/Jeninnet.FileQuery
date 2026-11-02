//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests the documented cross-dialect precedence: Glob and Regex can re-include
/// files that GitIgnore excluded, including below an excluded directory.
/// </summary>
[TestClass]
public sealed class CrossDialectReinclusionTests {
    private static readonly string[] _expectedPaths = ["root.txt", "sub/a.cs"];
    private static readonly string[] _expectedLeadingDoubleStarPaths = ["other/src/a.cs", "root.txt"];

    /// <summary>
    /// Gets the test context injected by MSTest.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
    /// <summary>
    /// A Glob rule re-includes a file inside a directory excluded by GitIgnore,
    /// while a non-matching file stays excluded.
    /// </summary>
    [TestMethod]
    public void Glob_ReincludesFile_InsideGitIgnoreExcludedDirectory() {
        using var environment = new TestEnvironment();
        environment.CreateFiles("sub/a.cs", "sub/b.log", "root.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                        [PatternKind.GitIgnore] = ["sub/"],
                        [PatternKind.Glob] = ["sub/*.cs"]
                    })));

        var results = FileQueryRuntime.Create()
                                      .Execute(new(environment.Root, options), TestContext.CancellationToken)
                                      .Select(path => Path.GetRelativePath(environment.Root, path).Replace('\\', '/'))
                                      .OrderBy(static path => path, StringComparer.Ordinal)
                                      .ToArray();

        results.Should().Equal(_expectedPaths);
    }

    /// <summary>
    /// A leading recursive wildcard in the Glob fallback is treated as unanchored,
    /// so it can re-include below any excluded directory.
    /// </summary>
    [TestMethod]
    public void GlobWithLeadingDoubleStar_ReincludesBelowExcludedDirectory() {
        using var environment = new TestEnvironment();
        environment.CreateFiles("other/src/a.cs", "other/b.txt", "root.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                        [PatternKind.GitIgnore] = ["other/"],
                        [PatternKind.Glob] = ["**/src/*.cs"]
                    })));

        var results = FileQueryRuntime.Create()
            .Execute(new(environment.Root, options), TestContext.CancellationToken)
            .Select(path => Path.GetRelativePath(environment.Root, path).Replace('\\', '/'))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        results.Should().Equal(_expectedLeadingDoubleStarPaths);
    }

    /// <summary>
    /// A Regex rule re-includes a file inside a directory excluded by GitIgnore.
    /// </summary>
    [TestMethod]
    public void Regex_ReincludesFile_InsideGitIgnoreExcludedDirectory() {
        using var environment = new TestEnvironment();
        environment.CreateFiles("sub/a.cs", "sub/b.log", "root.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                        [PatternKind.GitIgnore] = ["sub/"],
                        [PatternKind.Regex] = [@"^sub/.*\.cs$"]
                    })));

        var results = FileQueryRuntime.Create()
            .Execute(new(environment.Root, options), TestContext.CancellationToken)
            .Select(path => Path.GetRelativePath(environment.Root, path).Replace('\\', '/'))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        results.Should().Equal(_expectedPaths);
    }
}
