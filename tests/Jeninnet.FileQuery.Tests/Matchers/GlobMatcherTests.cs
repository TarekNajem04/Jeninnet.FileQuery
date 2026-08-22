//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// Provides unit tests for the <see cref="GlobInstructionMatcher"/> class.
/// </summary>
[TestClass]
public class GlobMatcherTests {
    private static GlobInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.Glob, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    /// <summary>Tests AnchoredMatch_ShouldNotBeUnanchored.</summary>
    [TestMethod]
    public void AnchoredMatch_ShouldNotBeUnanchored() {
        // Glob is implicitly anchored to the root unless '**/'' is used.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["src/*.cs"]);

        // Matches from root
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/File.cs", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        // Does NOT match if nested (no implicit unanchored check)
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "other/src/File.cs", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>Tests RecursiveWildcard_ShouldMatchDeeply_WhenAnchoredToRoot.</summary>
    [TestMethod]
    public void RecursiveWildcard_ShouldMatchDeeply_WhenAnchoredToRoot() {
        // Pattern `**/` at the start provides the unanchored-like behavior
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["**/config.json"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "config.json", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/config.json", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/main/config.json", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>Tests ComplexGlobbing.</summary>
    [TestMethod]
    public void ComplexGlobbing() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["data/??.log"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "data/01.log", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "data/abc.log", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "?? only matches two characters.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "other/data/01.log", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "Not anchored.");
    }

    /// <summary>Tests Wildcard_CharacterSet_Digits.</summary>
    [TestMethod]
    public void Wildcard_CharacterSet_Digits() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[0-9]"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.0", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.5", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.b", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "The category includes only digits, therefore it does not include the letter b.");
    }

    /// <summary>Tests Wildcard_CharacterSet_Characters.</summary>
    [TestMethod]
    public void Wildcard_CharacterSet_Characters() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[abc]"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.a", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.b", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.c", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.h", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess(), "The category includes only letters a, b, and c.");
    }

    /// <summary>Tests Wildcard_CharacterSet_Complex_2.</summary>
    [TestMethod]
    public void Wildcard_CharacterSet_Complex_2() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[CB]at"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.Cat")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.Bat")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.BAT")).IsSuccess(), "The category includes only letters C and B, followed by 'at'.");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.rat")).IsSuccess(), "The category includes only letters C and B, followed by 'at'.");
    }

    /// <summary>Tests Wildcard_CharacterSet_Negate.</summary>
    [TestMethod]
    public void Wildcard_CharacterSet_Negate() {
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: ["example.[!0-9]"]);

        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.a")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "example.t")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "example.9")).IsSuccess(), "The category excludes digits.");
    }

    /// <summary>Tests Wildcard_CharacterSet_Asterix.</summary>
    [TestMethod]
    public void Wildcard_CharacterSet_Asterix() {
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

    /// <summary>Tests MultiPattern_SecondPatternShouldMatch.</summary>
    [TestMethod]
    public void MultiPattern_SecondPatternShouldMatch() {
        var matcher = CreateMatcher();
        var patterns = Compile(["src/*.cs", "test/*.cs"]);
        var ctx = CreateFileContext(path: "test/helpers.cs");

        Assert.AreEqual(MatchOutcome.Include, matcher.Match(patterns, ctx));
    }
}
