//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Represents the outcome of evaluating a filesystem entry from the perspective of traversal.
/// </summary>
/// <param name="ShouldYield">Whether the entry should be included in the results.</param>
/// <param name="ShouldTraverse">Whether the entry should be traversed into.</param>
internal readonly record struct TraversalDecision(
    bool ShouldYield,
    bool ShouldTraverse
);
