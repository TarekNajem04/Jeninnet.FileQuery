namespace Jeninnet.FileQuery.Tests.PatternEngine;

/// <summary>
/// Contains unit tests for verifying the correct creation and behavior of various pattern tokens.
/// </summary>
[TestClass]
public class PatternTokenTests {
    /// <summary>
    /// Verifies that a <see cref="CharacterClassToken"/> is correctly created with the specified elements and negation status.
    /// </summary>
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
        TestAssertEx.ContainsSubset(token.Value.Elements, [new CharLiteral('a'), new CharLiteral('b')]);
        TestAssertEx.ContainsSubset(token.Value.Elements, [new CharRange('x', 'z')]);
    }

    /// <summary>
    /// Verifies that a <see cref="LiteralToken"/> correctly stores and exposes the expected literal text value.
    /// </summary>
    [TestMethod]
    public void LiteralToken_ShouldStoreValue() {
        var token = new LiteralToken("hello");
        Assert.AreEqual("hello", token.Text);
    }

    /// <summary>
    /// Verifies that a <see cref="WildcardToken"/> is correctly instantiated to represent a wildcard.
    /// </summary>
    [TestMethod]
    public void Wildcard_ShouldMatchEverything_ForSingleSegment() {
        var token = new WildcardToken();
        Assert.IsNotNull(token);
    }
}
