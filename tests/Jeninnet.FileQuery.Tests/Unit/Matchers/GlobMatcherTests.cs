//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for the Glob instruction matcher, validating anchoring, recursive wildcards,
/// character sets, negated sets, and multi-pattern matching.
/// </summary>
[TestClass]
public class GlobMatcherTests {
    private static GlobInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.Glob, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    /// <summary>
    /// Verifies that a glob pattern without '**/' is implicitly anchored to the root and does not match nested paths.
    /// </summary>
    [TestMethod]
    public void Should_NotBeUnanchored_When_AnchoredMatchUsed() {
        // Glob is implicitly anchored to the root unless '**/'' is used.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["src/*.cs"]);

        // Matches from root
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/File.cs", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        // Does NOT match if nested (no implicit unanchored check)
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "other/src/File.cs", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>
    /// Verifies that a glob pattern prefixed with '**/' matches files at any depth in the directory tree.
    /// </summary>
    [TestMethod]
    public void Should_MatchDeeply_When_AnchoredRecursiveWildcardUsed() {
        // Pattern `**/` at the start provides the unanchored-like behavior
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/config.json"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "config.json", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/config.json", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/main/config.json", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>
    /// Verifies that complex glob patterns with '?' wildcards match exactly the specified number of characters.
    /// </summary>
    [TestMethod]
    public void Should_MatchComplexPatterns_When_Globbing() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["data/??.log"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "data/01.log", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "data/abc.log", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "?? only matches two characters.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "other/data/01.log", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "Not anchored.");
    }

    /// <summary>
    /// Verifies that a digit character set '[0-9]' only matches numeric characters.
    /// </summary>
    [TestMethod]
    public void Should_MatchDigits_When_CharacterSetUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[0-9]"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.0", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.5", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.b", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "The category includes only digits, therefore it does not include the letter b.");
    }

    /// <summary>
    /// Verifies that a character set '[abc]' only matches the specified characters.
    /// </summary>
    [TestMethod]
    public void Should_MatchCharacters_When_CharacterSetUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[abc]"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.a", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.b", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.c", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.h", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "The category includes only letters a, b, and c.");
    }

    /// <summary>
    /// Verifies that a character set combined with literal characters matches correctly.
    /// </summary>
    [TestMethod]
    public void Should_MatchComplexSet_When_CharacterSetUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[CB]at"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.Cat")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.Bat")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.BAT")).IsSuccess(), "The category includes only letters C and B, followed by 'at'.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.rat")).IsSuccess(), "The category includes only letters C and B, followed by 'at'.");
    }

    /// <summary>
    /// Verifies that a negated character set '[!0-9]' excludes the specified characters.
    /// </summary>
    [TestMethod]
    public void Should_NegateSet_When_NegatedCharacterSetUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[!0-9]"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.a")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.t")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.9")).IsSuccess(), "The category excludes digits.");
    }

    /// <summary>
    /// Verifies that the asterisk wildcard matches zero or more characters after a literal prefix.
    /// </summary>
    [TestMethod]
    public void Should_MatchAsterisk_When_CharacterSetUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.Law*"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.Law")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.Lawyer")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.GrokLaw")).IsSuccess(), "Asterix only matches suffixes after 'Law'.");
        var matcher1 = CreateMatcher();
        var patterns1 = Compile(patterns: ["example.*Law*"]);

        Assert.IsTrue(matcher1.Match(patterns1, CreateFileContext(path: "example.Law")).IsSuccess());
        Assert.IsTrue(matcher1.Match(patterns1, CreateFileContext(path: "example.Lawyer")).IsSuccess());
        Assert.IsTrue(matcher1.Match(patterns1, CreateFileContext(path: "example.GrokLaw5")).IsSuccess());
        Assert.IsFalse(matcher1.Match(patterns1, CreateFileContext(path: "example.law")).IsSuccess(), "Asterix only matches letters after 'Law' with exact casing.");
        Assert.IsFalse(matcher1.Match(patterns1, CreateFileContext(path: "example.aw")).IsSuccess(), "Asterix only matches letters before 'Law' and after 'Law'.");
    }

    /// <summary>
    /// Verifies that when multiple glob patterns are compiled, a file matching any of them is included.
    /// </summary>
    [TestMethod]
    public void Should_MatchSecondPattern_When_MultiplePatternsUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(["src/*.cs", "test/*.cs"]);
        var ctx = CreateFileContext(path: "test/helpers.cs");

        Assert.AreEqual(MatchOutcome.Include, matcher.Match(patterns, ctx));
    }
}
