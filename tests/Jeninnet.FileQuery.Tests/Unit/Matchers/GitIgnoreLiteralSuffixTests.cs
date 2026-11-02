//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for the LiteralSuffix rejection fast path applied to GitIgnore wildcard
/// filename patterns. The fast path is semantically transparent: every assertion
/// here validates end-to-end match results, and the suffix resolution itself is
/// verified directly through the compiled pattern metadata.
/// </summary>
[TestClass]
public class GitIgnoreLiteralSuffixTests {
    private static GitIgnoreInstructionMatcher CreateMatcher() => new();

    private static CompiledPatternSet Compile(IEnumerable<string> patterns) =>
        (CompiledPatternSet)CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>
    /// Verifies that the trailing literal run of the last segment is resolved as the
    /// suffix (e.g. <c>**/*.cs</c> → <c>.cs</c>, <c>**/*.generated.cs</c> → <c>.generated.cs</c>).
    /// </summary>
    [TestMethod]
    public void Should_ResolveTrailingLiteralRun_When_LastSegmentEndsWithLiterals() {
        Compile(["**/*.cs"]).Patterns[0].LiteralSuffix.Should().Be(".cs");
        Compile(["!**/*.generated.cs"]).Patterns[0].LiteralSuffix.Should().Be(".generated.cs");
        Compile(["**/build"]).Patterns[0].LiteralSuffix.Should().Be("build");
    }

    /// <summary>
    /// Verifies that no suffix is resolved when the last segment ends with a wildcard,
    /// single-character wildcard, or recursive wildcard token.
    /// </summary>
    [TestMethod]
    public void Should_ResolveNoSuffix_When_LastSegmentEndsWithWildcard() {
        Compile(["**/*"]).Patterns[0].LiteralSuffix.Should().Be(string.Empty);
        Compile(["**/*?"]).Patterns[0].LiteralSuffix.Should().Be(string.Empty);
        Compile(["**"]).Patterns[0].LiteralSuffix.Should().Be(string.Empty);
        Compile(["src/**/*.[ab]"]).Patterns[0].LiteralSuffix.Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that the trailing run stops at the first non-literal token scanning
    /// backwards, so only the true tail of the segment forms the suffix, while literal
    /// runs that precede a wildcard are excluded.
    /// </summary>
    [TestMethod]
    public void Should_ResolveOnlyTrailingTokens_When_EarlierTokensAreWildcards() {
        LiteralSuffixResolver.Resolve([[new WildcardToken(), new LiteralToken(".cs")]]).Should().Be(".cs");
        LiteralSuffixResolver.Resolve([[new LiteralToken("a"), new WildcardToken(), new LiteralToken("b.cs")]]).Should().Be("b.cs");
        LiteralSuffixResolver.Resolve([[new LiteralToken("log"), new WildcardToken()]]).Should().Be(string.Empty);
        LiteralSuffixResolver.Resolve([[new WildcardToken(), new SingleCharToken(), new LiteralToken("x")]]).Should().Be("x");
    }

    /// <summary>
    /// Verifies that escaped characters participate in the resolved suffix.
    /// </summary>
    [TestMethod]
    public void Should_ResolveSuffix_When_LiteralRunContainsEscapedCharacters() {
        LiteralSuffixResolver.Resolve([[new WildcardToken(), new EscapeToken('*')]]).Should().Be("*");
        LiteralSuffixResolver.Resolve([[new EscapeToken('*'), new LiteralToken(".cs")]]).Should().Be("*.cs");
    }

    /// <summary>
    /// Verifies that directory-only patterns never carry a suffix; their last segment
    /// may match an ancestor segment rather than the path's final segment.
    /// </summary>
    [TestMethod]
    public void Should_ResolveNoSuffix_When_PatternIsDirectoryOnly() {
        Compile(["*.cs/"]).Patterns[0].LiteralSuffix.Should().Be(string.Empty);
        Compile(["/a/"]).Patterns[0].LiteralSuffix.Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that <c>**/*.cs</c> matches both root and deeply nested files while
    /// rejecting files with other extensions via the suffix fast path.
    /// </summary>
    [TestMethod]
    public void Should_MatchExtensionFiles_When_RecursiveWildcardSuffixPatternUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/*.cs"]);

        matcher.Match(patterns, CreateFileContext(path: "file.cs")).IsSuccess().Should().BeFalse(because: "file.cs should be excluded.");
        matcher.Match(patterns, CreateFileContext(path: "dir/sub/file.cs")).IsSuccess().Should().BeFalse(because: "Deep file.cs should be excluded.");
        matcher.Match(patterns, CreateFileContext(path: "file.txt")).IsSuccess().Should().BeTrue(because: "file.txt should not be excluded (fast-path rejection).");
        matcher.Match(patterns, CreateFileContext(path: "dir/file.txt")).IsSuccess().Should().BeTrue(because: "Nested file.txt should not be excluded (fast-path rejection).");
        matcher.Match(patterns, CreateFileContext(path: "a.cs/file.txt")).IsSuccess().Should().BeTrue(because: "file.txt under a .cs directory should not be excluded.");
    }

    /// <summary>
    /// Verifies that a negated suffix pattern re-includes only the files carrying the
    /// suffix, while all other files remain excluded.
    /// </summary>
    [TestMethod]
    public void Should_ReincludeSuffixedFiles_When_NegatedWildcardSuffixPatternUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/*.cs", "!**/*.generated.cs"]);

        matcher.Match(patterns, CreateFileContext(path: "a.generated.cs")).IsSuccess().Should().BeTrue(because: "generated file should be re-included.");
        matcher.Match(patterns, CreateFileContext(path: "dir/sub/b.generated.cs")).IsSuccess().Should().BeTrue(because: "Deep generated file should be re-included.");
        matcher.Match(patterns, CreateFileContext(path: "b.cs")).IsSuccess().Should().BeFalse(because: "Plain .cs file should stay excluded.");
        matcher.Match(patterns, CreateFileContext(path: "b.generated.txt")).IsSuccess().Should().BeTrue(because: "Non-.cs file matches no rule and stays included.");
    }

    /// <summary>
    /// Verifies that the fast path honors the active case-sensitivity mode.
    /// </summary>
    [TestMethod]
    public void Should_HonorCaseSensitivity_When_FastPathRejects() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/*.CS"]);

        matcher.Match(patterns, CreateFileContext(path: "file.cs")).IsSuccess().Should().BeTrue(because: "Case-sensitive: 'file.cs' must not be excluded by '**/*.CS'.");
        matcher.Match(patterns, CreateFileContext(path: "file.cs", CaseSensitivity.Insensitive)).IsSuccess().Should().BeFalse(because: "Case-insensitive: 'file.cs' must be excluded by '**/*.CS'.");
    }

    /// <summary>
    /// Verifies that escaped wildcard characters in the suffix are matched literally
    /// and only as a necessary condition (the full matcher still rules).
    /// </summary>
    [TestMethod]
    public void Should_MatchEscapedWildcardSuffix_When_PatternEndsWithEscapedCharacter() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/*\\*"]);

        matcher.Match(patterns, CreateFileContext(path: "dir/a*")).IsSuccess().Should().BeFalse(because: "File ending with literal '*' should be excluded.");
        matcher.Match(patterns, CreateFileContext(path: "dir/ab")).IsSuccess().Should().BeTrue(because: "File without '*' should not be excluded (fast-path rejection).");
    }

    /// <summary>
    /// Verifies that patterns with no resolvable suffix fall back to the full matcher
    /// and behave exactly as before.
    /// </summary>
    [TestMethod]
    public void Should_FallBackToFullMatcher_When_NoSuffixResolved() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/*.cs", "!**/*.cs?"]);

        matcher.Match(patterns, CreateFileContext(path: "deep/a.csx")).IsSuccess().Should().BeTrue(because: "a.csx matches '!**/*.cs?' and is re-included.");
        matcher.Match(patterns, CreateFileContext(path: "deep/a.cs")).IsSuccess().Should().BeFalse(because: "a.cs matches '**/*.cs' only and stays excluded.");
    }

    /// <summary>
    /// Verifies that directory-only and anchored directory-only patterns are untouched
    /// by the fast path, including the anchored subtree semantics where a directory-only
    /// pattern matches descendant files.
    /// </summary>
    [TestMethod]
    public void Should_PreserveDirectoryOnlySemantics_When_SuffixPatternPresent() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**", "!*.cs/"]);

        matcher.Match(patterns, CreateDirectoryContext(path: "file.cs")).IsSuccess().Should().BeTrue(because: "Directory 'file.cs' should be re-included by '!*.cs/'.");
        matcher.Match(patterns, CreateFileContext(path: "file.cs")).IsSuccess().Should().BeFalse(because: "File 'file.cs' should not be affected by the directory-only rule.");

        var anchored = Compile(patterns: ["/a/"]);
        matcher.Match(anchored, CreateFileContext(path: "a/b")).IsSuccess().Should().BeFalse(because: "Existing semantics: anchored '/a/' excludes descendant file 'a/b'.");
    }

    /// <summary>
    /// Verifies that multi-segment unanchored patterns still require the suffix on the
    /// path's final segment only.
    /// </summary>
    [TestMethod]
    public void Should_RequireSuffixOnFinalSegment_When_UnanchoredMultiSegmentPatternUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["src/*.generated.cs"]);

        matcher.Match(patterns, CreateFileContext(path: "src/foo.generated.cs")).IsSuccess().Should().BeFalse(because: "src/foo.generated.cs should be excluded.");
        matcher.Match(patterns, CreateFileContext(path: "src/foo.cs")).IsSuccess().Should().BeTrue(because: "src/foo.cs should not be excluded (fast-path rejection).");
        matcher.Match(patterns, CreateFileContext(path: "lib/foo.generated.cs")).IsSuccess().Should().BeTrue(because: "Other directory should not be excluded.");
        matcher.Match(patterns, CreateFileContext(path: "src/deep/foo.generated.cs")).IsSuccess().Should().BeTrue(because: "Deeper nesting should not be excluded without a recursive wildcard.");
    }

    /// <summary>
    /// Verifies that directory paths (which carry a trailing separator) are evaluated
    /// correctly by the fast path.
    /// </summary>
    [TestMethod]
    public void Should_HandleTrailingSeparator_When_PathIsDirectory() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/*.cs"]);

        matcher.Match(patterns, CreateDirectoryContext(path: "generated.cs")).IsSuccess().Should().BeFalse(because: "Directory 'generated.cs' should be excluded.");
        matcher.Match(patterns, CreateDirectoryContext(path: "sub/generated.cs/")).IsSuccess().Should().BeFalse(because: "Trailing-slash directory 'sub/generated.cs/' should be excluded.");
        matcher.Match(patterns, CreateDirectoryContext(path: "sub/readme/")).IsSuccess().Should().BeTrue(because: "Directory 'sub/readme/' should not be excluded (fast-path rejection).");
    }
}
