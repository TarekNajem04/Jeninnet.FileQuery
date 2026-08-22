//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
