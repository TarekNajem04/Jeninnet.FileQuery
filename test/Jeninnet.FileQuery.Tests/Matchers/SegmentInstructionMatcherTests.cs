namespace Jeninnet.FileQuery.Tests.Matchers;

[TestClass]
public class SegmentInstructionMatcherTests {
    [TestMethod]
    public void MatchSegment_PosixClass_Digit_Matches() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, new List<ICharacterClassElement> { new PosixClass("digit") }))
        };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("1", tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    [TestMethod]
    public void MatchSegment_PosixClass_Unknown_DoesNotMatch() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(false, new List<ICharacterClassElement> { new PosixClass("unknown") }))
        };
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }

    [TestMethod]
    public void MatchSegment_CharacterClass_Negated_Matches() {
        var tokens = new List<IPatternToken>
        {
            new CharacterClassToken(new CharacterClass(true, new List<ICharacterClassElement> { new CharLiteral('a') }))
        };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("b", tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a", tokens, StringComparison.Ordinal));
    }
}
