namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// Provides unit tests for the <see cref="HybridPathMatcher"/> class.
/// </summary>
[TestClass]
public class HybridPathMatcherTests {
    private static HybridPathMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(ClassifiedPatternSet patterns) => CompiledPatternFactory.Compile(patterns);

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) =>
        Compile(
            new ClassifiedPatternSet() {
                Patterns = [.. patterns.Select(pattern => new ClassifiedPattern(Text: pattern, Type: PatternClassifier.Classify(pattern)))]
            }
        );

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>Tests ShouldMatchSingleLiteral.</summary>
    [TestMethod]
    public void ShouldMatchSingleLiteral() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!foo.txt"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "foo.txt")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "bar.txt")).IsSuccess());
    }

    /// <summary>Tests ShouldSupportNegation.</summary>
    [TestMethod]
    public void ShouldSupportNegation() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!*.cs", "Program.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "Test.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "Program.cs")).IsSuccess());
    }

    /// <summary>Tests ShouldSupportDirectoryOnlyRules.</summary>
    [TestMethod]
    public void ShouldSupportDirectoryOnlyRules() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!obj/"]);

        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "obj")).IsSuccess(), "the directory 'obj' should be matched"); // directory
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "obj")).IsSuccess(), "the file 'obj' should not be matched excluded by pattern '**'"); // file
    }

    /// <summary>Tests ShouldMatchWildcardPatterns.</summary>
    [TestMethod]
    public void ShouldMatchWildcardPatterns() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["*.txt", "!*.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "Program.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "Program.txt")).IsSuccess());
    }

    /// <summary>Tests ShouldMatchRecursiveWildcardPatterns.</summary>
    [TestMethod]
    public void ShouldMatchRecursiveWildcardPatterns() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["*.txt", "!src/**/*.cs"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/Program.cs")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/utils/Helper.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "src/utils/Helper.txt")).IsSuccess());
    }

    /// <summary>Tests ShouldHandleMultiPatternCliInput.</summary>
    [TestMethod]
    public void ShouldHandleMultiPatternCliInput() {
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

    /// <summary>Tests ShouldSupportIgnoreCase.</summary>
    [TestMethod]
    public void ShouldSupportIgnoreCase() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["*.txt", "!Foo.TXT"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "foo.txt", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "FOO.TXT", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "bar.txt", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>Tests ShouldReturnFalseForEmptyOrNullPath.</summary>
    [TestMethod]
    public void ShouldReturnFalseForEmptyOrNullPath() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["!**"]);

        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: null!)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "")).IsSuccess());
    }

    /// <summary>
    /// Tests the complex GitIgnore scenario: Directory exclusion with subsequent
    /// subdirectory re-inclusion, verifying the 'last rule wins' and traversal semantics
    /// are correctly handled by the HybridPathMatcher.
    /// </summary>
    [TestMethod]
    public void HybridPathMatcher_DirectoryOnly_RestoreSubDir() {
        // ARRANGE
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [
                "**",                           // 1. Global Exclude: Match anything (Default Excluded)
                "ignore_me/**",                 // 2. Exclusion: Explicitly exclude ALL files/subdirs under 'ignore_me' (Pruning Rule)
                "!ignore_me/recover/**",        // 3. Re-include the SUBTREE 'recover/' (Allows matching and traversal into 'recover')
                //"!ignore_me/recover/file.txt"   // 4. Explicitly re-include the specific file
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

    /// <summary>Tests HybridPathMatcher_SimpleNegated.</summary>
    [TestMethod]
    public void HybridPathMatcher_SimpleNegated() {
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
        TestAssertEx.HasCount(result, 2);
        TestAssertEx.Contains(result, x => x.EndsWith(".bin", StringComparison.Ordinal));
        TestAssertEx.Contains(result, x => x.EndsWith("file.txt", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(result, x => x.EndsWith("file1.txt", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(result, x => x.EndsWith("file2.txt", StringComparison.Ordinal));
    }

    /// <summary>Tests DirectoryOnly_Inclusion_ShouldReturnFilesInsideSubdirectories.</summary>
    [TestMethod]
    public void DirectoryOnly_Inclusion_ShouldReturnFilesInsideSubdirectories() {
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
    /// Anchored Directory Exclusion with Nested Files
    /// Goal: Test the strict anchoring of an exclusion rule (`/temp/`) and ensure it only excludes paths at the root.
    /// </summary>
    [TestMethod]
    public void AnchoredExclusion_DescendantsShouldBeExcluded() {
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

