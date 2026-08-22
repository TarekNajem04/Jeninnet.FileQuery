//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents the fully canonicalized pattern input.
/// </summary>
/// <remarks>
/// This is the only input accepted by the classifier.
/// </remarks>
public sealed record CanonicalPatternSet {
    /// <summary>
    /// Gets the list of canonical patterns.
    /// </summary>
    public IReadOnlyList<CanonicalPattern> Patterns { get; init; } = [];
}
