namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Tokenizes the single-character wildcard (<c>*</c>).
/// </summary>
/// <remarks>
/// <para>
/// This tokenizer recognizes the non-recursive wildcard token.
/// It must be evaluated <strong>after</strong> <see cref="RecursiveWildcardTokenizer"/>
/// to avoid misinterpreting <c>**</c>.
/// </para>
/// <para>
/// Architectural rules:
/// <list type="bullet">
/// <item>No backtracking</item>
/// <item>No semantic validation</item>
/// <item>No allocations beyond the token itself</item>
/// </list>
/// </para>
/// </remarks>
internal sealed class WildcardTokenizer : IPatternTokenizer
{
    public bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    )
    {
        if(input[index] != '*')
        {
            return false;
        }

        tokens.Add(new WildcardToken());
        index++;
        return true;
    }
}
