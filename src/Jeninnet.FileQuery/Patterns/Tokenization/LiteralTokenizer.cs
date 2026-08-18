//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Tokenizes literal character sequences.
/// </summary>
/// <remarks>
/// <para>
/// This tokenizer is the final fallback in the tokenization pipeline.
/// It consumes all characters that are not recognized by earlier tokenizers
/// and groups them into a single <see cref="LiteralToken"/>.
/// </para>
/// <para>
/// Architectural contract:
/// <list type="bullet">
/// <item>Must always consume at least one character</item>
/// <item>Must never fail</item>
/// <item>Must never inspect other token types</item>
/// </list>
/// </para>
/// </remarks>
internal sealed class LiteralTokenizer : IPatternTokenizer {
    /// <inheritdoc />
    public bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    ) {
        var start = index;

        while(index < input.Length) {
            var c = input[index];

            if(IsTokenBoundary(c, syntax)) {
                break;
            }

            index++;
        }

        // Safety net: always consume at least one character
        if(index == start) {
            index++;
        }

        tokens.Add(new LiteralToken(input[start..index].ToString()));

        return true;
    }

    private static bool IsTokenBoundary(char c, PatternSyntaxProfile syntax) =>
        c switch {
            '\\' when syntax.SupportsEscaping => true,
            '*' => true,
            '?' when syntax.SupportsSingleCharWildcard => true,
            '[' when syntax.SupportsCharacterClasses => true,
            _ => false
        };
}
