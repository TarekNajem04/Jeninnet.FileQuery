namespace Jeninnet.FileQuery.Tests.Architecture;

/// <summary>
/// Contains architectural tests for pattern tokenizers.
/// </summary>
[TestClass]
public sealed class TokenizerArchitectureTests {
    private static readonly IPatternTokenizer[] _tokenizers =
    [
        new EscapeTokenizer(),
        new RecursiveWildcardTokenizer(),
        new WildcardTokenizer(),
        new SingleCharWildcardTokenizer(),
        new CharacterClassTokenizer(),
        new LiteralTokenizer()
    ];

    private static readonly PatternSyntaxProfile _syntax =
        PatternSyntaxProfile.GitIgnore;

    /// <summary>Tests Tokenizers_MustAdvanceIndex_OrDecline.</summary>
    [TestMethod]
    public void Tokenizers_MustAdvanceIndex_OrDecline() {
        const string input = "*?[abc]\\x**foo";

        foreach(var tokenizer in _tokenizers) {
            for(var i = 0; i < input.Length; i++) {
                var span = input.AsSpan();
                var index = i;
                var tokens = new List<IPatternToken>();

                var accepted = tokenizer.TryTokenize(
                    span,
                    ref index,
                    _syntax,
                    tokens);

                if(accepted) {
                    Assert.IsGreaterThan(
                        i,
                        index, $"""
                        Tokenizer {tokenizer.GetType().Name} accepted input
                        but did not advance index.

                        Position: {i}
                        Input: "{input}"
                        """
                    );
                } else {
                    Assert.AreEqual(
                        i,
                        index,
                        $"""
                        Tokenizer {tokenizer.GetType().Name} declined input
                        but modified index.

                        Position: {i}
                        Input: "{input}"
                        """
                    );
                }
            }
        }
    }
}
