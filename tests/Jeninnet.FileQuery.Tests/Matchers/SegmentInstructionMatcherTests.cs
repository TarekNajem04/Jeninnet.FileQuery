//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// Provides test cases for <see cref="SegmentInstructionMatcher"/>.
/// </summary>
[TestClass]
public class SegmentInstructionMatcherTests {
    /// <summary>
    /// Verifies that a digit POSIX class token matches a digit character.
    /// </summary>
    [TestMethod]
    public void MatchSegment_PosixClass_Digit_Matches() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new PosixClass("digit")]))
        };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("1", tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that an unknown POSIX class token does not match.
    /// </summary>
    [TestMethod]
    public void MatchSegment_PosixClass_Unknown_DoesNotMatch() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new PosixClass("unknown")]))
        };
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a negated character class correctly matches characters not in the set.
    /// </summary>
    [TestMethod]
    public void MatchSegment_CharacterClass_Negated_Matches() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(true, [new CharLiteral('a')]))
        };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("b", tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a range is folded when the comparison ignores case.
    /// </summary>
    [TestMethod]
    public void MatchSegment_CharacterRange_IgnoreCase_MatchesOppositeCase() {
        var lowerRange = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new CharRange('a', 'z')]))
        };
        var upperRange = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new CharRange('A', 'Z')]))
        };

        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("A", lowerRange, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("a", upperRange, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("A", lowerRange, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", upperRange, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a literal inside a character class is folded when the comparison ignores case.
    /// </summary>
    [TestMethod]
    public void MatchSegment_CharacterClassLiteral_IgnoreCase_MatchesOppositeCase() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new CharLiteral('a')]))
        };

        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("A", tokens, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("A", tokens, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that negation is applied after case folding.
    /// </summary>
    [TestMethod]
    public void MatchSegment_CharacterClass_Negated_IgnoreCase_FoldsBeforeNegation() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(true, [new CharRange('a', 'z')]))
        };

        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("A", tokens, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("1", tokens, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that POSIX categories keep their category semantics under ignore-case comparisons.
    /// </summary>
    [TestMethod]
    public void MatchSegment_PosixUpper_IgnoreCase_RemainsCategoryBased() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new PosixClass("upper")]))
        };

        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("A", tokens, StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.OrdinalIgnoreCase));
    }
}
