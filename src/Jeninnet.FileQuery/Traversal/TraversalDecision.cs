namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Represents the outcome of evaluating a filesystem entry from the perspective of traversal.
/// </summary>
internal readonly record struct TraversalDecision(
    bool ShouldYield,
    bool ShouldTraverse
);
