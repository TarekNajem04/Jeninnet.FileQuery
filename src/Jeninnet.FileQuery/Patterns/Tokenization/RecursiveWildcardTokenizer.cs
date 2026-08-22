//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Tokenizes recursive wildcard syntax (<c>**</c>).
/// </summary>
/// <remarks>
/// <para>
/// This tokenizer is purely lexical:
/// <list type="bullet">
/// <item>It recognizes <c>**</c> only.</item>
/// <item>It does not validate placement.</item>
/// <item>It does not enforce segment isolation.</item>
/// </list>
/// </para>
/// <para>
/// All semantic and structural rules are enforced by invariants.
/// </para>
/// </remarks>
internal sealed class RecursiveWildcardTokenizer : IPatternTokenizer {
    public bool TryTokenize(
        ReadOnlySpan<char> input,
        ref int index,
        PatternSyntaxProfile syntax,
        List<IPatternToken> tokens
    ) {
        // Feature disabled → decline fast
        if(!syntax.SupportsRecursiveWildcard) {
            return false;
        }

        // Need at least "**"
        if(index + 1 >= input.Length) {
            return false;
        }

        if(input[index] != '*' || input[index + 1] != '*') {
            return false;
        }

        tokens.Add(new RecursiveWildcardToken());
        index += 2;
        return true;
    }
}
