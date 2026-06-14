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
