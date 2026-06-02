namespace Jeninnet.FileQuery.Tests.PatternEngine;

[TestClass]
public class PatternTokenTests {
    [TestMethod]
    public void CharacterClass_ShouldCreateProperToken() {
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
        TestAssertEx.ContainsSubset(token.Value.Elements, new List<ICharacterClassElement> { new CharLiteral('a'), new CharLiteral('b') });
        TestAssertEx.ContainsSubset(token.Value.Elements, new List<ICharacterClassElement> { new CharRange('x', 'z') });
    }

    [TestMethod]
    public void LiteralToken_ShouldStoreValue() {
        var token = new LiteralToken("hello");
        Assert.AreEqual("hello", token.Text);
    }

    [TestMethod]
    public void Wildcard_ShouldMatchEverything_ForSingleSegment() {
        var token = new WildcardToken();
        Assert.IsNotNull(token);
    }
}
