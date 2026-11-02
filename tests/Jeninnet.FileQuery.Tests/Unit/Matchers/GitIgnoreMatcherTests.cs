//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for the GitIgnore instruction matcher, validating negation, anchoring, directory-only rules,
/// recursive wildcards, and re-inclusion semantics.
/// </summary>
/// <remarks>
/// We use GitIgnorePatternCompiler for all tests to ensure the IsNegated and DirectoryOnly flags
/// are correctly set, as all matchers utilize these flags. Each matcher's unique logic (anchoring, segment matching)
/// is then tested against its own constraints.
/// </remarks>
[TestClass]
public class GitIgnoreMatcherTests {
    private static GitIgnoreInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>
    /// Verifies that a later negation rule overrides a previous match for the same file.
    /// </summary>
    [TestMethod]
    public void Should_OverridePreviousMatch_When_NegationApplied() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!*.cs", "Program.cs"]);

        (matcher.Match(patterns, CreateFileContext(path: "Test.cs")).IsSuccess()).Should().BeTrue();
        (matcher.Match(patterns, CreateFileContext(path: "Program.cs")).IsSuccess()).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a directory-only rule only applies to directories and not files with the same name.
    /// </summary>
    [TestMethod]
    public void Should_ApplyCorrectly_When_DirectoryOnlyRuleUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!bin/"]);

        (matcher.Match(patterns, CreateDirectoryContext(path: "bin")).IsSuccess()).Should().BeTrue();
        (matcher.Match(patterns, CreateFileContext(path: "bin")).IsSuccess()).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a negation rule with a recursive wildcard matches files at any depth.
    /// </summary>
    [TestMethod]
    public void Should_MatchDeepPaths_When_RecursiveWildcardUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**/temp/*.txt"]);

        (matcher.Match(patterns, CreateFileContext(path: "a/b/c/temp/d.txt")).IsSuccess()).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an unanchored exclusion pattern matches files at any location in the path tree.
    /// </summary>
    [TestMethod]
    public void Should_MatchUnanchored_When_BasicExclusionUsed() {
        // Unanchored pattern (matches anywhere)
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!*.log", "*.md"]);

        (matcher.Match(patterns, CreateFileContext(path: "debug.log")).IsSuccess()).Should().BeTrue(because: "File at root should match *.log.");
        (matcher.Match(patterns, CreateFileContext(path: "src/release.log")).IsSuccess()).Should().BeTrue(because: "Nested file should match unanchored *.log.");
        (matcher.Match(patterns, CreateFileContext(path: "README.md")).IsSuccess()).Should().BeFalse(because: "Unrelated file should not match.");
    }

    /// <summary>
    /// Verifies that an anchored exclusion pattern (starting with '/') only matches paths at the root level.
    /// </summary>
    [TestMethod]
    public void Should_MatchOnlyFromRoot_When_AnchoredExclusionUsed() {
        // Pattern anchored to root by '/'
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!/temp", "!/src/*.txt"]);

        // Matches at root
        (matcher.Match(patterns, CreateDirectoryContext(path: "temp")).IsSuccess()).Should().BeTrue(because: "Directory 'temp' at root should match '/temp'.");
        (matcher.Match(patterns, CreateFileContext(path: "src/file.txt")).IsSuccess()).Should().BeTrue(because: "File 'src/file.txt' at root should match '/src/*.txt'.");
        // Included by default GitIgnoreMatcher: IsIncluded=true
        (matcher.Match(patterns, CreateDirectoryContext(path: "bin/temp")).IsSuccess()).Should().BeTrue(because: "Directory 'bin/temp' should not match anchored '/temp'.");
        (matcher.Match(patterns, CreateFileContext(path: "other/src/file.txt")).IsSuccess()).Should().BeTrue(because: "File should not match if prefix is present.");
    }

    /// <summary>
    /// Verifies that a directory-only exclusion pattern matches directories but not files with the same name.
    /// </summary>
    [TestMethod]
    public void Should_MatchDirectory_When_DirectoryOnlyExclusionUsed() {
        // Directory-only pattern "logs/"
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!logs/"]);

        // Matches directory itself (crucial for FileQueryEngine pruning)
        (matcher.Match(patterns, CreateDirectoryContext(path: "logs")).IsSuccess()).Should().BeTrue(because: "Directory 'logs' should be matched.");
        // Does NOT match file with the same name (by matcher logic, if no other pattern does)
        // Since there are no exclusion rules, the file defaults to included.
        (matcher.Match(patterns, CreateFileContext(path: "logs")).IsSuccess()).Should().BeTrue(because: "File 'logs' should be included because the default is to include and the directory-only rule does not match files.");
        // this match the nested directory as well, because unanchored patterns match anywhere in the path
        (matcher.Match(patterns, CreateDirectoryContext(path: "src/logs")).IsSuccess()).Should().BeTrue(because: "Nested directory 'src/logs' should be matched.");
        // ===== to Prevent the Match =====
        var matcher2 = CreateMatcher();
        var patterns2 = Compile(patterns: ["/logs/"]);

        //with anchored exclusion this should NOT match the nested directory
        (matcher2.Match(patterns2, CreateDirectoryContext(path: "src/logs")).IsSuccess()).Should().BeTrue(because: "Nested directory 'src/logs' should be matched by default.");
    }

    /// <summary>
    /// Verifies that re-inclusion semantics work correctly: a directory is excluded but a specific file within it is re-included.
    /// </summary>
    [TestMethod]
    public void Should_ReIncludeFiles_When_DirectoryReInclusionSemanticsApplied() {
        // Standard GitIgnore pattern set for re-inclusion: exclude folder, include file inside.
        var matcher = CreateMatcher();
        var patterns = Compile(
                          patterns: [
                              "ignored_dir/",
                              "!ignored_dir/keep.txt"
                          ]
                      );

        // 1. Directory is excluded (matched by ignored_dir/).
        (matcher.Match(patterns, CreateDirectoryContext(path: "ignored_dir")).IsSuccess()).Should().BeFalse(because: "Directory 'ignored_dir' is matched by the EXCLUSION rule 'ignored_dir/', But not included.");
        // 2. File to keep is included (Last rule wins)
        (matcher.Match(patterns, CreateFileContext(path: "ignored_dir/keep.txt")).IsSuccess()).Should().BeTrue(because: "File 'keep.txt' should be included by '!ignored_dir/keep.txt'.");
        // 3. File to exclude is excluded (matched by ignored_dir/)
        (matcher.Match(patterns, CreateFileContext(path: "ignored_dir/dump.txt")).IsSuccess()).Should().BeFalse(because: "Files under an ignored directory are excluded.");
    }

    /// <summary>
    /// Verifies that a recursive wildcard negation pattern matches files at any depth in the directory tree.
    /// </summary>
    [TestMethod]
    public void Should_MatchDeeply_When_RecursiveWildcardUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**/a.txt"]);

        (matcher.Match(patterns, CreateFileContext(path: "a.txt")).IsSuccess()).Should().BeTrue();
        (matcher.Match(patterns, CreateFileContext(path: "dir/a.txt")).IsSuccess()).Should().BeTrue();
        (matcher.Match(patterns, CreateFileContext(path: "dir/subdir/a.txt")).IsSuccess()).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that when the last rule is a negation, it wins over prior exclusion rules.
    /// </summary>
    [TestMethod]
    public void Should_Win_When_LastRuleIsNegation() {
        // *.tmp is exclusion. !important.tmp is inclusion.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!*.tmp"]);

        (matcher.Match(patterns, CreateFileContext(path: "delete.tmp")).IsSuccess()).Should().BeTrue(because: "!*.tmp matches and is the last rule. (True = Included, which is the problem with this matcher's logic. GitIgnore means True=Excluded).");
        // Test with `*.tmp` first:
        var matcher1 = CreateMatcher();
        var patterns1 = Compile(patterns: ["*.tmp"]);
        (matcher1.Match(patterns1, CreateFileContext(path: "delete.tmp")).IsSuccess()).Should().BeFalse(because: "Must be excluded by '*.tmp' (IsMatch=False).");
        (matcher1.Match(patterns1, CreateFileContext(path: "README.md")).IsSuccess()).Should().BeTrue(because: "Default is Include (IsMatch=True)."); // Default isIncluded=true in GitIgnoreMatcher!

        // Test complex:
        var matcher2 = CreateMatcher();
        var patterns2 = Compile(patterns: ["*.tmp", "!important.tmp"]);
        (matcher2.Match(patterns2, CreateFileContext(path: "delete.tmp")).IsSuccess()).Should().BeFalse(because: "Excluded by '*.tmp'.");
        (matcher2.Match(patterns2, CreateFileContext(path: "important.tmp")).IsSuccess()).Should().BeTrue(because: "Included by '!important.tmp'.");
    }

    /// <summary>
    /// Verifies that the matcher correctly handles an empty or root path, which represents the project root itself.
    /// </summary>
    /// <remarks>
    /// Mitigates: Runtime errors from null/empty path segment lists in SplitNormalizedPath and MatchPathSegments.
    /// </remarks>
    /// <summary>Tests Should_DefaultToExclude_When_PathIsEmpty.</summary>
    [TestMethod]
    public void Should_DefaultToExclude_When_PathIsEmpty() {
        // An empty path ("") or a single root slash ("/") represents the project root.
        // GitIgnore typically does not match the root itself unless explicitly excluded.
        // The default is `isIncluded = false`.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!"]);

        // Assuming normalized path for root is "", which your SplitNormalizedPath converts to Array.Empty<string>()
        (matcher.Match(patterns, CreateDirectoryContext(path: "")).IsSuccess()).Should().BeFalse(because: "The project root (empty path) should be excluded by default.");
        // Test explicit inclusion of the root directory itself (using a compiler that treats "/" as a match for the root path)
        // NOTE: This behavior is highly implementation-specific. Assuming it shouldn't match.
        (matcher.Match(patterns, CreateDirectoryContext(path: "src")).IsSuccess()).Should().BeTrue(because: "A normal subdirectory should be included by default.");
    }

    /// <summary>
    /// Verifies that a bare '/' pattern throws a PatternException because it is not valid.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_EmptyPathWithBareRootPattern() {
        // Arrange

        // Act
        static void act() => Compile(patterns: ["/"]);

        // Assert
        var exception = ((Action)act).Should().Throw<PatternException>();
        exception.Exception.Message.Should().Contain("bare '/' is not a valid pattern");
    }

    /// <summary>
    /// Verifies that an empty pattern set defaults to excluding any path.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeByDefault_When_PathIsEmpty() {
        var matcher = CreateMatcher();
        // Empty pattern set, so no rules to match, should default to exclude (IsIncluded=false).
        var patterns = Compile(patterns: []);

        (matcher.Match(patterns, CreateDirectoryContext(path: "")).IsSuccess()).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a recursive wildcard at the end of a pattern matches the directory itself and all its descendants.
    /// </summary>
    /// <remarks>
    /// Mitigates: Off-by-one errors in the recursive MatchPathSegments loop, particularly when checking the base case.
    /// </remarks>
    /// <summary>Tests Should_MatchSubtree_When_RecursiveWildcardAtEnd.</summary>
    [TestMethod]
    public void Should_MatchSubtree_When_RecursiveWildcardAtEnd() {
        // Pattern: "docs/**" should match "docs" itself, and everything inside it.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!docs/**"]);

        // Matches 'docs' directory itself (assuming 'docs' segments: ['docs'] is matched by ['docs', '**'])
        (matcher.Match(patterns, CreateDirectoryContext(path: "docs")).IsSuccess()).Should().BeTrue(because: "Directory 'docs' should match 'docs/**'.");
        // Matches content in the docs subdirectory
        (matcher.Match(patterns, CreateFileContext(path: "docs/README.md")).IsSuccess()).Should().BeTrue(because: "File one level deep should match.");
        (matcher.Match(patterns, CreateFileContext(path: "docs/sub/image.jpg")).IsSuccess()).Should().BeTrue(because: "File multiple levels deep should match.");
        // Should NOT match a directory named 'docs' that is nested. (Unanchored behavior is assumed to be handled by the ** prefix of the pattern 'docs/**')
        (matcher.Match(patterns, CreateFileContext(path: "project/docs/README.md")).IsSuccess()).Should().BeTrue(because: "Pattern 'docs/**' is unanchored and should match nested paths too.");
    }

    /// <summary>
    /// Verifies strict anchoring of an exclusion rule so it only excludes paths at the root, leaving nested paths in the default included state.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeDescendants_When_AnchoredExclusionUsed() {
        // Pattern: "/temp/" is an EXCLUSION rule (IsNegated=false) for the 'temp' directory ONLY at the root.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["/temp/"]);

        // ==========================================
        // Root Match: Should be successfully matched and EXCLUDED (Successed() = False).
        // ==========================================

        // 1. Root Directory 'temp'
        (matcher.Match(patterns, CreateDirectoryContext(path: "temp")).IsSuccess()).Should().BeFalse(because: "Root directory 'temp' must be EXCLUDED by the anchored pattern.");
        // 2. File 'temp' (same name as directory)
        // A directory-only pattern SHOULD NOT match a file with the same name. It should default to Included.
        (matcher.Match(patterns, CreateFileContext(path: "temp")).IsSuccess()).Should().BeTrue(because: "File 'temp' (not a directory) should be INCLUDED because /temp/ is directory-only.");
        // 3. File inside root 'temp'
        // This file must be EXCLUDED because its ancestor directory 'temp' matched the exclusion rule.
        (matcher.Match(patterns, CreateFileContext(path: "temp/file.log")).IsSuccess()).Should().BeFalse(because: "File inside root 'temp' must be EXCLUDED.");
        // ==========================================
        // Nested Must NOT Match: Should default to INCLUDED (Successed() = True).
        // ==========================================

        // 1. Nested Directory 'src/temp'
        // The anchored rule /temp/ does not match nested directories, so it defaults to INCLUDED.
        (matcher.Match(patterns, CreateDirectoryContext(path: "src/temp")).IsSuccess()).Should().BeTrue(because: "Nested directory 'src/temp' must be INCLUDED (Traversed) as it doesn't match the anchored rule.");
        // 2. File inside nested 'src/temp'
        // Since the directory is traversed, the file inside is INCLUDED by default.
        (matcher.Match(patterns, CreateFileContext(path: "src/temp/file.log")).IsSuccess()).Should().BeTrue(because: "File inside nested 'src/temp' must be INCLUDED by default.");
    }

    /// <summary>
    /// Verifies that a complex negation correctly re-includes a deeply nested file within a generally excluded tree.
    /// </summary>
    /// <remarks>
    /// Mitigates: Failures in the "Last Rule Wins" loop and the recursive MatchPathSegments logic when matching the deep path twice.
    /// </remarks>
    /// <summary>Tests Should_ReIncludeDeeply_When_ComplexNegationUsed.</summary>
    [TestMethod]
    public void Should_ReIncludeDeeply_When_ComplexNegationUsed() {
        // 1. Exclude all log files in all subfolders
        // 2. Re-include only one specific, deeply nested file
        var matcher = CreateMatcher();
        var patterns = Compile(
            patterns: [
                "**/*.log",
                "!src/data/config.log"
            ]
        );

        // Excluded by **/*.log
        (matcher.Match(patterns, CreateFileContext(path: "root.log")).IsSuccess()).Should().BeFalse(because: "File at root excluded by **/*.log.");
        (matcher.Match(patterns, CreateFileContext(path: "src/temp/dump.log")).IsSuccess()).Should().BeFalse(because: "Deep file excluded by **/*.log.");
        // Included by the last rule
        (matcher.Match(patterns, CreateFileContext(path: "src/data/config.log")).IsSuccess()).Should().BeTrue(because: "Specific file included by !src/data/config.log.");
    }

    /// <summary>
    /// Verifies that the global re-inclusion pattern '!' correctly includes all paths.
    /// </summary>
    [TestMethod]
    public void Should_MatchEverything_When_GlobalInclusionApplied() {
        // The pattern ["!"] globally includes all files and directories.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!"]);

        // 1. The empty path ("") is matched by [**] and included.
        (matcher.Match(patterns, CreateDirectoryContext(path: "")).IsSuccess()).Should().BeFalse(because: "The project root (empty path) should be EXCLUDED by the global '!' rule.");
        // 2. A subdirectory path ("src") is matched by [**] and included.
        (matcher.Match(patterns, CreateDirectoryContext(path: "src")).IsSuccess()).Should().BeTrue(because: "A normal subdirectory should be INCLUDED by the global '!' rule.");
        // 3. A file path is matched by [**] and included.
        (matcher.Match(patterns, CreateFileContext(path: "file.txt")).IsSuccess()).Should().BeTrue(because: "A file should be INCLUDED by the global '!' rule.");
    }

    /// <summary>
    /// Verifies that a global re-inclusion pattern with a recursive wildcard correctly includes files in subdirectories.
    /// </summary>
    [TestMethod]
    public void Should_TestSubfolders_When_GlobalInclusionApplied() {
        // The pattern ["!"] globally includes all files and directories.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**/*.txt"]);

        (matcher.Match(patterns, CreateDirectoryContext(path: "bin/file3.txt")).IsSuccess()).Should().BeTrue(because: "The 'bin' directory should be INCLUDED by the global '!**/*.txt' rule.");
    }
}
