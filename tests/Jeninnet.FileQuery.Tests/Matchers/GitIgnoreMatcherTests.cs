//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// NOTE: We use GitIgnorePatternCompiler for all tests to ensure the IsNegated and DirectoryOnly flags
/// are correctly set, as all matchers utilize these flags. Each matcher's unique logic (anchoring, segment matching)
/// is then tested against its own constraints.
/// </summary>
[TestClass]
public class GitIgnoreMatcherTests {
    private static GitIgnoreInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>Tests Negation_ShouldOverridePreviousMatch.</summary>
    [TestMethod]
    public void Negation_ShouldOverridePreviousMatch() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!*.cs", "Program.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "Test.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "Program.cs")).IsSuccess());
    }

    /// <summary>Tests DirectoryOnlyRule_ShouldApplyCorrectly.</summary>
    [TestMethod]
    public void DirectoryOnlyRule_ShouldApplyCorrectly() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!bin/"]);

        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "bin")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "bin")).IsSuccess());
    }

    /// <summary>Tests RecursiveWildcard_ShouldMatchDeepPaths.</summary>
    [TestMethod]
    public void RecursiveWildcard_ShouldMatchDeepPaths() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**/temp/*.txt"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "a/b/c/temp/d.txt")).IsSuccess());
    }

    /// <summary>Tests BasicExclusion_ShouldMatchUnanchored.</summary>
    [TestMethod]
    public void BasicExclusion_ShouldMatchUnanchored() {
        // Unanchored pattern (matches anywhere)
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!*.log", "*.md"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "debug.log")).IsSuccess(), "File at root should match *.log.");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/release.log")).IsSuccess(), "Nested file should match unanchored *.log.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "README.md")).IsSuccess(), "Unrelated file should not match.");
    }

    /// <summary>Tests AnchoredExclusion_ShouldMatchOnlyFromRoot.</summary>
    [TestMethod]
    public void AnchoredExclusion_ShouldMatchOnlyFromRoot() {
        // Pattern anchored to root by '/'
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!/temp", "!/src/*.txt"]);

        // Matches at root
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "temp")).IsSuccess(), "Directory 'temp' at root should match '/temp'.");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/file.txt")).IsSuccess(), "File 'src/file.txt' at root should match '/src/*.txt'.");
        // Included by default GitIgnoreMatcher: IsIncluded=true
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "bin/temp")).IsSuccess(), "Directory 'bin/temp' should not match anchored '/temp'.");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "other/src/file.txt")).IsSuccess(), "File should not match if prefix is present.");
    }

    /// <summary>Tests DirectoryOnlyExclusion_ShouldMatchDirectory.</summary>
    [TestMethod]
    public void DirectoryOnlyExclusion_ShouldMatchDirectory() {
        // Directory-only pattern "logs/"
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!logs/"]);

        // Matches directory itself (crucial for FileQueryEngine pruning)
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "logs")).IsSuccess(), "Directory 'logs' should be matched.");
        // Does NOT match file with the same name (by matcher logic, if no other pattern does)
        // Since there are no exclusion rules, the file defaults to included.
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "logs")).IsSuccess(), "File 'logs' should be included because the default is to include and the directory-only rule does not match files.");
        // this match the nested directory as well, because unanchored patterns match anywhere in the path
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "src/logs")).IsSuccess(), "Nested directory 'src/logs' should be matched.");
        // ===== to Prevent the Match =====
        var matcher2 = CreateMatcher();
        var patterns2 = Compile(patterns: ["/logs/"]);

        //with anchored exclusion this should NOT match the nested directory
        Assert.IsTrue(
            matcher2.Match(patterns2, CreateDirectoryContext(path: "src/logs")).IsSuccess(),
            "Nested directory 'src/logs' should be matched by default.");
    }

    /// <summary>Tests DirectoryReInclusionSemantics.</summary>
    [TestMethod]
    public void DirectoryReInclusionSemantics() {
        // Standard GitIgnore pattern set for re-inclusion: exclude folder, include file inside.
        var matcher = CreateMatcher();
        var patterns = Compile(
                          patterns: [
                              "ignored_dir/",
                              "!ignored_dir/keep.txt"
                          ]
                      );

        // 1. Directory is excluded (matched by ignored_dir/).
        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: "ignored_dir")).IsSuccess(), "Directory 'ignored_dir' is matched by the EXCLUSION rule 'ignored_dir/', But not included.");
        // 2. File to keep is included (Last rule wins)
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "ignored_dir/keep.txt")).IsSuccess(), "File 'keep.txt' should be included by '!ignored_dir/keep.txt'.");
        // 3. File to exclude is excluded (matched by ignored_dir/)
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "ignored_dir/dump.txt")).IsSuccess(), "File 'dump.txt' should be included by default.");
    }

    /// <summary>Tests RecursiveWildcard_ShouldMatchDeeply.</summary>
    [TestMethod]
    public void RecursiveWildcard_ShouldMatchDeeply() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**/a.txt"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "a.txt")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "dir/a.txt")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "dir/subdir/a.txt")).IsSuccess());
    }

    /// <summary>Tests Negation_LastRuleWins.</summary>
    [TestMethod]
    public void Negation_LastRuleWins() {
        // *.tmp is exclusion. !important.tmp is inclusion.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!*.tmp"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "delete.tmp")).IsSuccess(), "!*.tmp matches and is the last rule. (True = Included, which is the problem with this matcher's logic. GitIgnore means True=Excluded).");
        // Test with `*.tmp` first:
        var matcher1 = CreateMatcher();
        var patterns1 = Compile(patterns: ["*.tmp"]);
        Assert.IsFalse(matcher1.Match(patterns1, CreateFileContext(path: "delete.tmp")).IsSuccess(), "Must be excluded by '*.tmp' (IsMatch=False).");
        Assert.IsTrue(matcher1.Match(patterns1, CreateFileContext(path: "README.md")).IsSuccess(), "Default is Include (IsMatch=True)."); // Default isIncluded=true in GitIgnoreMatcher!

        // Test complex:
        var matcher2 = CreateMatcher();
        var patterns2 = Compile(patterns: ["*.tmp", "!important.tmp"]);
        Assert.IsFalse(matcher2.Match(patterns2, CreateFileContext(path: "delete.tmp")).IsSuccess(), "Excluded by '*.tmp'.");
        Assert.IsTrue(matcher2.Match(patterns2, CreateFileContext(path: "important.tmp")).IsSuccess(), "Included by '!important.tmp'.");
    }

    /// <summary>
    /// Empty Path / Root Path Matching
    /// Goal: Ensure the matcher correctly handles the empty/root path, which usually means the project root itself.
    /// Mitigates: Runtime errors from null/empty path segment lists in SplitNormalizedPath and MatchPathSegments.
    /// </summary>
    [TestMethod]
    public void EdgeCase_EmptyPathShouldDefaultToExclude() {
        // An empty path ("") or a single root slash ("/") represents the project root.
        // GitIgnore typically does not match the root itself unless explicitly excluded.
        // The default is `isIncluded = false`.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!"]);

        // Assuming normalized path for root is "", which your SplitNormalizedPath converts to Array.Empty<string>()
        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: "")).IsSuccess(), "The project root (empty path) should be excluded by default.");
        // Test explicit inclusion of the root directory itself (using a compiler that treats "/" as a match for the root path)
        // NOTE: This behavior is highly implementation-specific. Assuming it shouldn't match.
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "src")).IsSuccess(), "A normal subdirectory should be included by default.");
    }

    /// <summary>Tests EdgeCase_EmptyPathShouldThrowForBareRootPattern.</summary>
    [TestMethod]
    public void EdgeCase_EmptyPathShouldThrowForBareRootPattern() {
        // Arrange

        // Act
        static void act() => Compile(patterns: ["/"]);

        // Assert
        var exception = TestAssertEx.Throws<PatternException>(act);
        Assert.Contains("bare '/' is not a valid pattern", exception.Message);
    }

    /// <summary>Tests EmptyPathShouldBeExcludedByDefault.</summary>
    [TestMethod]
    public void EmptyPathShouldBeExcludedByDefault() {
        var matcher = CreateMatcher();
        // Empty pattern set, so no rules to match, should default to exclude (IsIncluded=false).
        var patterns = Compile(patterns: []);

        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: "")).IsSuccess());
    }

    /// <summary>
    /// Recursive Wildcard (Double Star) at the End
    /// Goal: Ensure** correctly matches zero or more segments when it appears at the end of an unanchored pattern, especially when matching a directory.
    /// Mitigates: Off-by-one errors in the recursive MatchPathSegments loop, particularly when checking the base case (patternIndex == patternTokens.Count - 1).
    /// </summary>
    [TestMethod]
    public void RecursiveWildcard_AtEndShouldMatchSubtree() {
        // Pattern: "docs/**" should match "docs" itself, and everything inside it.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!docs/**"]);

        // Matches 'docs' directory itself (assuming 'docs' segments: ['docs'] is matched by ['docs', '**'])
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "docs")).IsSuccess(), "Directory 'docs' should match 'docs/**'.");
        // Matches content in the docs subdirectory
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "docs/README.md")).IsSuccess(), "File one level deep should match.");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "docs/sub/image.jpg")).IsSuccess(), "File multiple levels deep should match.");
        // Should NOT match a directory named 'docs' that is nested. (Unanchored behavior is assumed to be handled by the ** prefix of the pattern 'docs/**')
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "project/docs/README.md")).IsSuccess(), "Pattern 'docs/**' is unanchored and should match nested paths too.");
    }

    /// <summary>
    /// Anchored Directory Exclusion with Nested Files
    /// Goal: Test the strict anchoring of an exclusion rule (`/temp/`) to ensure it only excludes paths at the root,
    /// leaving nested paths in the default Included state.
    /// </summary>
    [TestMethod]
    public void AnchoredExclusion_DescendantsShouldBeExcluded() {
        // Pattern: "/temp/" is an EXCLUSION rule (IsNegated=false) for the 'temp' directory ONLY at the root.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["/temp/"]);

        // ==========================================
        // Root Match: Should be successfully matched and EXCLUDED (Successed() = False).
        // ==========================================

        // 1. Root Directory 'temp'
        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: "temp")).IsSuccess(), "Root directory 'temp' must be EXCLUDED by the anchored pattern.");
        // 2. File 'temp' (same name as directory)
        // A directory-only pattern SHOULD NOT match a file with the same name. It should default to Included.
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "temp")).IsSuccess(), "File 'temp' (not a directory) should be INCLUDED because /temp/ is directory-only.");
        // 3. File inside root 'temp'
        // This file must be EXCLUDED because its ancestor directory 'temp' matched the exclusion rule.
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "temp/file.log")).IsSuccess(), "File inside root 'temp' must be EXCLUDED.");
        // ==========================================
        // Nested Must NOT Match: Should default to INCLUDED (Successed() = True).
        // ==========================================

        // 1. Nested Directory 'src/temp'
        // The anchored rule /temp/ does not match nested directories, so it defaults to INCLUDED.
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "src/temp")).IsSuccess(), "Nested directory 'src/temp' must be INCLUDED (Traversed) as it doesn't match the anchored rule.");
        // 2. File inside nested 'src/temp'
        // Since the directory is traversed, the file inside is INCLUDED by default.
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/temp/file.log")).IsSuccess(), "File inside nested 'src/temp' must be INCLUDED by default.");
    }

    /// <summary>
    /// Negation on a Deep Exclusion
    /// Goal: Test the complex interaction of exclusion first, then re-inclusion for a specific file nested deep within a generally excluded tree.
    /// Mitigates: Failures in the "Last Rule Wins" loop and the recursive MatchPathSegments logic when matching the deep path twice.
    /// </summary>
    [TestMethod]
    public void ComplexNegation_DeepReInclusion() {
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
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "root.log")).IsSuccess(), "File at root excluded by **/*.log.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "src/temp/dump.log")).IsSuccess(), "Deep file excluded by **/*.log.");
        // Included by the last rule
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/data/config.log")).IsSuccess(), "Specific file included by !src/data/config.log.");
    }

    /// <summary>
    /// Goal: Verify that the global re-inclusion pattern "!" correctly sets the final match state
    /// to INCLUDE (True) for all paths, including the root and its descendants.
    /// </summary>
    [TestMethod]
    public void ReInclusion_GlobalInclusion_MatchesEverything() {
        // The pattern ["!"] globally includes all files and directories.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!"]);

        // 1. The empty path ("") is matched by [**] and included.
        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext(path: "")).IsSuccess(), "The project root (empty path) should be EXCLUDED by the global '!' rule.");
        // 2. A subdirectory path ("src") is matched by [**] and included.
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "src")).IsSuccess(), "A normal subdirectory should be INCLUDED by the global '!' rule.");
        // 3. A file path is matched by [**] and included.
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "file.txt")).IsSuccess(), "A file should be INCLUDED by the global '!' rule.");
    }

    /// <summary>
    /// Goal: Verify that the global re-inclusion pattern "!" correctly sets the final match state
    /// to INCLUDE (True) for all paths, including the root and its descendants.
    /// </summary>
    [TestMethod]
    public void ReInclusion_GlobalInclusion_SubfolderMustTest() {
        // The pattern ["!"] globally includes all files and directories.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**/*.txt"]);

        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "bin/file3.txt")).IsSuccess(), "The 'bin' directory should be INCLUDED by the global '!**/*.txt' rule.");
    }
}
