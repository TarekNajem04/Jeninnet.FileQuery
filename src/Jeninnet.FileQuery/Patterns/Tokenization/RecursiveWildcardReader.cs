//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

internal sealed class RecursiveWildcardReader : ITokenReader {
    public bool TryRead(ReadOnlySpan<char> pattern, ref int i, out PatternToken token) {
        token = null!;

        if(pattern[i] == '*' && i + 1 < pattern.Length && pattern[i + 1] == '*') {
            token = new RecursiveWildcardToken();
            i += 2;
            return true;
        }

        return false;
    }
}
