//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for the hybrid path matcher, validating combined GitIgnore and Glob pattern matching,
/// negation, directory-only rules, recursive wildcards, case sensitivity, and re-inclusion semantics.
/// </summary>
[TestClass]
public class HybridPathMatcherTests {
    private static HybridPathMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(ClassifiedPatternSet patterns) => CompiledPatternFactory.Compile(patterns);

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) =>
        Compile(
            new ClassifiedPatternSet() {
                Patterns = [.. patterns.Select(static pattern => new ClassifiedPattern(Text: pattern, Type: PatternClassifier.Classify(pattern)))]
            }
        );

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>
    /// Verifies that a single literal pattern with negation matches the exact file name.
    /// </summary>
    [TestMethod]
    public void Should_MatchSingleLiteral() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!foo.txt"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "foo.txt")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "bar.txt")).IsSuccess());
    }

    /// <summary>
    /// Verifies that negation patterns correctly override previous matches.
    /// </summary>
    [TestMethod]
    public void Should_SupportNegation() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!*.cs", "Program.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "Test.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "Program.cs")).IsSuccess());
    }

    /// <summary>
    /// Verifies that directory-only rules apply only to directories and not files with the same name.
    /// </summary>
    [TestMethod]
    public void Should_SupportDirectoryOnlyRules() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!obj/"]);

        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "obj")).IsSuccess(), "the directory 'obj' should be matched"); // directory
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "obj")).IsSuccess(), "the file 'obj' should not be matched excluded by pattern '**'"); // file
    }

    /// <summary>
    /// Verifies that wildcard patterns match files based on glob-style extensions.
    /// </summary>
    [TestMethod]
    public void Should_MatchWildcardPatterns() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["*.txt", "!*.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "Program.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "Program.txt")).IsSuccess());
    }

    /// <summary>
    /// Verifies that recursive wildcard patterns match files at any depth within a specified directory.
    /// </summary>
    [TestMethod]
    public void Should_MatchRecursiveWildcardPatterns() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["*.txt", "!src/**/*.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/Program.cs")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/utils/Helper.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "src/utils/Helper.txt")).IsSuccess());
    }

    /// <summary>
    /// Verifies that multiple patterns from CLI-style input are correctly evaluated with proper precedence.
    /// </summary>
    [TestMethod]
    public void Should_HandleMultiPatternCliInput() {
        // Simulate CLI input: "*.cs;!/bin/**;src/**/*.txt"
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [
                "**",
                "!*.cs",
                "/bin/**",
                "!src/**/*.txt"
            ]
        );

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "Test.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "bin/Ignore.cs")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/data/file.txt")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "src/data/file.md")).IsSuccess());
    }

    /// <summary>
    /// Verifies that case-insensitive matching correctly ignores file name casing differences.
    /// </summary>
    [TestMethod]
    public void Should_SupportIgnoreCase() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["*.txt", "!Foo.TXT"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "foo.txt", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "FOO.TXT", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "bar.txt", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>
    /// Verifies that the matcher returns false when given an empty or null path.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_PathIsEmptyOrNull() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**"]);

        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: null!)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "")).IsSuccess());
    }

    /// <summary>
    /// Tests the complex GitIgnore scenario: directory exclusion with subsequent subdirectory re-inclusion,
    /// verifying the 'last rule wins' and traversal semantics are correctly handled.
    /// </summary>
    [TestMethod]
    public void Should_RestoreSubDir_When_DirectoryOnlyRuleApplied() {
        // ARRANGE
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [
                "**",                               // 1. Global Exclude: Match anything (Default Excluded)
                "ignore_me/**",                     // 2. Exclusion: Explicitly exclude ALL files/subdirs under 'ignore_me' (Pruning Rule)
                "!ignore_me/recover/**",            // 3. Re-include the SUBTREE 'recover/' (Allows matching and traversal into 'recover')
                //"!ignore_me/recover/file.txt"     // 4. Explicitly re-include the specific file
            ]
        );

        // ACT & ASSERT: Use normalized virtual paths (forward slashes)

        // --- Paths that MUST be INCLUDED ---
        const string dirToRestore = "ignore_me/recover"; // Directory path (without trailing slash for canonical path handling)
        const string fileToRestore = "ignore_me/recover/file.txt";

        // --- Paths that MUST be EXCLUDED ---
        const string outsideFile = "file.txt";
        const string rootFile = "ignore_me/root.txt";
        const string binFile = "ignore_me/bin/file.txt";
        const string binDir = "ignore_me/bin"; // Correct path for the directory, removed trailing '**'

        // 1. EXPECTED INCLUSION: The file the test failed to find previously (matched by !ignore_me/recover/file.txt)
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: fileToRestore)).IsSuccess(), "File inside re-included subdirectory must be included by the explicit negation rule.");
        // 2. EXPECTED INCLUSION: The subdirectory itself (matched by !ignore_me/recover/**)
        // The pattern "!ignore_me/recover/**" matches the directory path "ignore_me/recover"
        // and allows traversal.
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: dirToRestore)).IsSuccess(), "Re-included subdirectory path must be included by '!ignore_me/recover/**'.");
        // ------------------------------------

        // 3. EXPECTED EXCLUSION: A file outside the excluded tree
        // Should be false because it matched the exclusion rule "**" and did not match any inclusion rule.
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: outsideFile)).IsSuccess(), "File outside the ignored tree must be excluded by '**'.");
        // 4. EXPECTED EXCLUSION: File in the excluded parent directory (matched by ignore_me/**)
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: rootFile)).IsSuccess(), "File in the excluded parent directory must be excluded.");
        // 5. EXPECTED EXCLUSION: File in another excluded subdirectory (matched by ignore_me/**)
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: binFile)).IsSuccess(), "File in another excluded subdirectory must be excluded.");
        // 6. EXPECTED EXCLUSION: Another excluded subdirectory (matched by ignore_me/**)
        // This is the failing assertion. It must be False because "ignore_me/**" is a non-negated pruning rule.
        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: binDir)).IsSuccess(), $"Another excluded subdirectory '{binDir}' path must be excluded by pattern [\"ignore_me/**\"].");
    }

    /// <summary>
    /// Verifies that simple negation patterns correctly re-include files that were previously excluded.
    /// </summary>
    [TestMethod]
    public void Should_SimpleNegate_When_NegationPatternUsed() {
        var files = new[] {
                        "xxx.bin",
                        "file.txt",
                        "file1.txt",
                        "file2.txt",
                    };
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [
                "*.txt",    // 1. Exclude all .txt files
                "!*.bin",   // 2. Re-include all .bin files
                "!file.txt" // 3. Re-include specific file.txt
            ]
        );
        var result = files.Where(file => matcher.Match(patterns, CreateFileContext(path: file)).IsSuccess())
                          .ToList()
                          ;
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.EndsWith(".bin", StringComparison.Ordinal));
        result.Should().Contain(x => x.EndsWith("file.txt", StringComparison.Ordinal));
        result.Should().NotContain(x => x.EndsWith("file1.txt", StringComparison.Ordinal));
        result.Should().NotContain(x => x.EndsWith("file2.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that directory-only inclusion patterns include files inside subdirectories while excluding root-level directories.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFilesInsideSubdirectories_When_DirectoryOnlyInclusionUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!*/"]);

        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "temp")).IsSuccess(), "Root directory 'temp' must be excluded (IsNegated=false).");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "temp/file.log")).IsSuccess(), "File inside root 'temp' must be excluded.");
        // ==========================================
        // Nested Must Match
        // ==========================================
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "src/temp/")).IsSuccess(), "Nested directory 'src/temp' must be included.");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/temp/file.log")).IsSuccess(), "File inside nested 'src/temp' must be included.");
    }

    /// <summary>
    /// Verifies strict anchoring of an exclusion rule so it only excludes paths at the root, leaving nested paths included.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeDescendants_When_AnchoredExclusionUsed() {
        // Pattern: "/temp/" excludes the 'temp' directory and everything inside it, ONLY at the root.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["/temp/"]);

        // Root Match: Should be successfully matched and EXCLUDED.
        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: "temp")).IsSuccess(), "Root directory 'temp' must be excluded.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "temp/file.log")).IsSuccess(), "File inside root 'temp' must be excluded.");
        // Nested Must NOT Match:
        // It should NOT match the exclusion rule, therefore it remains INCLUDED (the default state).
        var nestedDirResult = matcher.Match(patterns, CreateDirectoryContext(path: "src/temp"));

        // Assertion 2: Since it didn't match the only exclusion rule, the final result must be INCLUDED (True).
        Assert.IsTrue(nestedDirResult.IsSuccess(), "Since it did not match the exclusion rule, the nested directory should be Included/Traversed by default.");
        var nestedFileResult = matcher.Match(patterns, CreateFileContext(path: "src/temp/file.log"));
        Assert.IsTrue(nestedFileResult.IsSuccess(), "File inside non-matching nested 'src/temp' should be Included by default.");
    }
}
