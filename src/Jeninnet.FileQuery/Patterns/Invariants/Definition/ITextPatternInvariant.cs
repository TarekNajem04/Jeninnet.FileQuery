//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Validates invariants on raw pattern text before compilation.
/// </summary>
internal interface ITextPatternInvariant {
    PatternInvariantResult Validate(string pattern);
}
