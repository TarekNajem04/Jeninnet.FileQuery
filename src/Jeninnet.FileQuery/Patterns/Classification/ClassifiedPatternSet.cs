//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Classification;

/// <summary>
/// Represents the output of pattern classification.
/// </summary>
/// <remarks>
/// This structure is the only valid input to pattern compilation.
/// </remarks>
internal sealed record ClassifiedPatternSet {
    public IReadOnlyList<ClassifiedPattern> Patterns { get; init; } = [];
}
