//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Tokenizes the single-character wildcard (<c>?</c>).
/// </summary>
/// <remarks>
/// <para>
/// This tokenizer is feature-gated by
/// <see cref="PatternSyntaxProfile.SupportsSingleCharWildcard"/>.
/// </para>
/// <para>
/// Architectural constraints:
/// <list type="bullet">
/// <item>No semantic validation</item>
/// <item>No matching logic</item>
/// <item>No memory allocation beyond token emission</item>
/// </list>
/// </para>
/// </remarks>
internal sealed class SingleCharWildcardTokenizer : IPatternTokenizer {
    public bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    ) {
        if(!syntax.SupportsSingleCharWildcard) {
            return false;
        }

        if(input[index] != '?') {
            return false;
        }

        tokens.Add(new SingleCharToken());
        index++;
        return true;
    }
}
