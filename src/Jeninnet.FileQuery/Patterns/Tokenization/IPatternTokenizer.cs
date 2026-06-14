namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Represents a single tokenization rule capable of recognizing
/// a specific <see cref="PatternToken"/> at a given input position.
/// </summary>
/// <remarks>
/// <para>
/// Tokenizers are ordered and evaluated sequentially.
/// The first tokenizer that successfully consumes input wins.
/// </para>
/// <para>
/// This design allows new pattern syntax to be introduced
/// without modifying the central scanner logic.
/// </para>
/// </remarks>
internal interface IPatternTokenizer
{
    /// <summary>
    /// Attempts to tokenize input starting at <paramref name="index"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The parser advances the shared <paramref name="index"/> in place via <c>ref</c>,
    /// ensuring that the tokenizer loop in <see cref="PatternScanner"/> always
    /// observes the correct next position after a character class — whether the
    /// class was valid or malformed.
    /// </para>
    /// </remarks>
    /// <param name="input">The full segment being tokenized.</param>
    /// <param name="index">
    /// The current position in <paramref name="input"/>.
    /// Must be advanced if tokenization succeeds.
    /// </param>
    /// <param name="syntax">Active pattern interpretation flags.</param>
    /// <param name="tokens">The token list to append to.</param>
    /// <returns>
    /// <c>true</c> if this tokenizer consumed input and emitted a token;
    /// otherwise <c>false</c>.
    /// </returns>
    bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    );
}
