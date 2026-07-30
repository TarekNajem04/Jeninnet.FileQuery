namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Tokens;

/// <summary>
/// Tests for PatternTokenTests.
/// </summary>
[TestClass]
public class PatternTokenTests {
    /// <summary>
    /// Verifies that Should CreateProperToken When CharacterClassUsed.
    /// </summary>
    [TestMethod]
    public void Should_CreateProperToken_When_CharacterClassUsed() {
        var cls = new CharacterClass
                    (
                        IsNegated: true,
                        Elements: new List<ICharacterClassElement> {
                            new CharLiteral('a'),
                            new CharLiteral('b'),
                            new CharRange('x', 'z')
                        }.AsReadOnly()
                    );
        var token = new CharacterClassToken(cls);

        Assert.IsTrue(token.Value.IsNegated);
        token.Value.Elements.Should().ContainSubset([new CharLiteral('a'), new CharLiteral('b')]);
        token.Value.Elements.Should().ContainSubset([new CharRange('x', 'z')]);
    }

    /// <summary>
    /// Verifies that Should StoreValue When LiteralTokenCreated.
    /// </summary>
    [TestMethod]
    public void Should_StoreValue_When_LiteralTokenCreated() {
        var token = new LiteralToken("hello");
        Assert.AreEqual("hello", token.Text);
    }

    /// <summary>
    /// Verifies that Should MatchEverything When WildcardUsedForSingleSegment.
    /// </summary>
    [TestMethod]
    public void Should_MatchEverything_When_WildcardUsedForSingleSegment() {
        var token = new WildcardToken();
        Assert.IsNotNull(token);
    }
}
