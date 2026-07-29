namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for the segment instruction matcher, validating POSIX character classes, unknown classes,
/// and negated character class matching.
/// </summary>
[TestClass]
public class SegmentInstructionMatcherTests {
    /// <summary>
    /// Verifies that a POSIX digit character class matches numeric characters and rejects non-numeric characters.
    /// </summary>
    [TestMethod]
    public void Should_Match_When_PosixDigitClassUsed() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new PosixClass("digit")]))
        };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("1", tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that an unknown POSIX character class fails to match any character.
    /// </summary>
    [TestMethod]
    public void Should_NotMatch_When_UnknownPosixClassUsed() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, [new PosixClass("unknown")]))
        };
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a negated character class matches any character not in the class.
    /// </summary>
    [TestMethod]
    public void Should_Match_When_NegatedCharacterClassUsed() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(true, [new CharLiteral('a')]))
        };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("b", tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }
}

