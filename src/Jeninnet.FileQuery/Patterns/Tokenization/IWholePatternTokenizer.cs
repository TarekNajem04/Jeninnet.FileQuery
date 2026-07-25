namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Defines an interface for tokenizers that process an entire pattern at once (e.g., Regex).
/// </summary>
internal interface IWholePatternTokenizer {
    bool TryTokenize(
        ReadOnlySpan<char> pattern,
        PatternSyntaxProfile syntax,
        out List<List<IPatternToken>> tokens,
        out PatternContext context
    );
}
