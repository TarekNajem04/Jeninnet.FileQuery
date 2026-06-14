namespace Jeninnet.FileQuery.Patterns.Tokenization;

internal sealed class LiteralReader : ITokenReader
{
    public bool TryRead(ReadOnlySpan<char> pattern, ref int i, out PatternToken token)
    {
        token = null!;

        var start = i;

        while(i < pattern.Length && IsLiteral(pattern[i]))
        {
            i++;
        }

        if(start == i)
        {
            return false;
        }

        token = new LiteralToken(pattern[start..i].ToString());
        return true;
    }

    private static bool IsLiteral(char c)
        => c is not ('*' or '?' or '[' or ']' or '/' or '\\');
}
