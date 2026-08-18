//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Tokenizes escaped characters (e.g. <c>\*</c>, <c>\?</c>, <c>\[</c>).
/// </summary>
/// <remarks>
/// <para>
/// Escaping has the highest priority during tokenization.
/// If enabled, a backslash causes the following character
/// to be treated as a literal, regardless of its usual meaning.
/// </para>
/// <para>
/// If the backslash appears at the end of the segment,
/// it is treated as a literal backslash.
/// </para>
/// </remarks>
internal sealed class EscapeTokenizer : IPatternTokenizer {
    public bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    ) {
        if(!syntax.SupportsEscaping) {
            return false;
        }

        if(input[index] != '\\' || index + 1 >= input.Length) {
            return false;
        }

        tokens.Add(new LiteralToken(input[index + 1].ToString()));
        index += 2;
        return true;
    }
}
