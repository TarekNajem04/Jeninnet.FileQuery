//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Defines an interface for reading a specific token type from a pattern span.
/// </summary>
internal interface ITokenReader {
    bool TryRead(ReadOnlySpan<char> pattern, ref int i, out PatternToken token);
}
