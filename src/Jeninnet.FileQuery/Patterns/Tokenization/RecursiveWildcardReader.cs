namespace Jeninnet.FileQuery.Patterns.Tokenization;

internal sealed class RecursiveWildcardReader : ITokenReader
{
    public bool TryRead(ReadOnlySpan<char> pattern, ref int i, out PatternToken token)
    {
        token = null!;

        if(pattern[i] == '*' && i + 1 < pattern.Length && pattern[i + 1] == '*')
        {
            token = new RecursiveWildcardToken();
            i += 2;
            return true;
        }

        return false;
    }
}
