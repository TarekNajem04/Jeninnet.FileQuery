namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Defines an interface for reading a specific token type from a pattern span.
/// </summary>
internal interface ITokenReader
{
    bool TryRead(ReadOnlySpan<char> pattern, ref int i, out PatternToken token);
}
