namespace Jeninnet.FileQuery.Patterns.Tokenization;

internal sealed class EscapeReader : ITokenReader {
    public bool TryRead(ReadOnlySpan<char> pattern, ref int i, out PatternToken token) {
        token = null!;

        if(pattern[i] != '\\' || i + 1 >= pattern.Length) {
            return false;
        }

        var next = pattern[i + 1];

        if(!IsEscapable(next)) {
            return false;
        }

        token = new EscapeToken(next);
        i += 2;
        return true;
    }

    private static bool IsEscapable(char c) => c is '*' or '?' or '!' or '#' or '[' or ']' or '\\';
}
