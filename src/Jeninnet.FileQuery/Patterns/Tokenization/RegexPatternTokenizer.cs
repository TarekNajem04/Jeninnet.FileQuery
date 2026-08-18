//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

internal sealed class RegexPatternTokenizer : IWholePatternTokenizer {
    public bool TryTokenize(
        ReadOnlySpan<char> pattern,
        PatternSyntaxProfile syntax,
        out List<List<IPatternToken>> tokens,
        out PatternContext context
    ) {
        tokens = null!;
        context = default;

        if(!syntax.IsRegularExpression) {
            return false;
        }

        if(pattern.Length < 3 || pattern[0] != 'r' || pattern[1] != ':') {
            return false;
        }

        context = new PatternContext(
            IsNegated: false,
            IsRootAnchored: false,
            IsDirectoryOnly: false,
            Start: 0,
            End: pattern.Length
        );

        tokens = [[new RegularExpressionToken(pattern[2..].ToString())]];

        return true;
    }
}
