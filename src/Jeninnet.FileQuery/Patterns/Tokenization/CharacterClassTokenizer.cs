namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Tokenizes character class expressions (<c>[...]</c>).
/// </summary>
/// <remarks>
/// <para>
/// This tokenizer recognizes the opening <c>'['</c> and delegates all parsing
/// to <see cref="CharacterClassParser"/>.
/// </para>
/// <para>
/// A <see cref="CharacterClassToken"/> is always added when <c>'['</c> is seen
/// (subject to <see cref="PatternSyntaxProfile.SupportsCharacterClasses"/>).
/// Malformed classes produce a token whose
/// <see cref="CharacterClass.Elements"/> list contains a
/// <see cref="CharacterClassParseError"/> sentinel; the invariant system
/// converts that sentinel into a <see cref="PatternException"/>
/// during the structural phase.
/// </para>
/// <para>
/// <strong>Architectural constraint:</strong> this tokenizer contains no
/// matching logic, no pattern validation, and no fallback behavior. It is a
/// pure lexical recognizer.
/// </para>
/// </remarks>
internal sealed class CharacterClassTokenizer : IPatternTokenizer
{
    /// <inheritdoc/>
    public bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    )
    {
        if(!syntax.SupportsCharacterClasses)
        {
            return false;
        }

        if(input[index] != '[')
        {
            return false;
        }

        // Delegate all parsing to CharacterClassParser.
        // The parser advances `index` past the entire class (including the closing ']'),
        // or to input.Length if the class is unterminated.
        // It never throws; malformed input becomes a CharacterClassParseError element.
        var charClass = CharacterClassParser.Parse(input, ref index);
        tokens.Add(new CharacterClassToken(charClass));
        return true;
    }
}
