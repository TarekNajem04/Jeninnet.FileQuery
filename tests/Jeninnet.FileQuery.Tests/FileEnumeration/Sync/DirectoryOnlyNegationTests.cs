//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests negated directory-only patterns to verify they re-include target directories
/// without leaking traversal into unrelated excluded directories.
/// </summary>
[TestClass]
public class DirectoryOnlyNegationTests {

    /// <summary>
    /// Verifies that a negated directory-only pattern (e.g. <c>!*.github/</c>)
    /// re-includes the target directory without leaking traversal into other
    /// excluded directories such as <c>bin/</c>.
    /// </summary>
    [TestMethod]
    public async Task NegatedDotDirectory_ShouldNotTraverseUnrelatedExcludedDirsAsync() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            [".github/workflows/test.yml"] = "x",
            [".git/config"] = "x",
            ["bin/app.dll"] = "x",
            ["src/Program.cs"] = "x"
        });

        var patternOptions = new PatternOptions(
            Patterns: ".*/; bin/; !*.github/;",
            Gitignore: null,
            Glob: null,
            RegularExpression: null
        );
        var typedPatterns = PatternBuilder.Build(patternOptions);

        var fileQueryOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    TypedPatterns: typedPatterns.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable())
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: FileQueryOptions.UNLIMITED_RECURSION_DEPTH,
                IgnoreInaccessible: true,
                CaseSensitivity: Enums.CaseSensitivity.PlatformDefault
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        FileQuery query = new(env.Root, fileQueryOptions);
        var result = (
            await fileQueryEngine.ExecuteAsync(query, CancellationToken.None)
                                 .ToListAsync(CancellationToken.None)
        ).ConvertAll(p => PathUtilities.Normalize(Path.GetRelativePath(env.Root, p)));

        // .github/workflows/test.yml — included via !*.github/ negation
        result.Should().Contain(".github/workflows/test.yml");

        // src/Program.cs — not excluded by any pattern
        result.Should().Contain("src/Program.cs");

        // .git/config — excluded by .*/ (dot-directory), must NOT appear
        result.Should().NotContain(p => p.StartsWith(".git/", StringComparison.Ordinal));

        // bin/app.dll — excluded by bin/, must NOT appear
        result.Should().NotContain(p => p.StartsWith("bin/", StringComparison.Ordinal));
    }
}
