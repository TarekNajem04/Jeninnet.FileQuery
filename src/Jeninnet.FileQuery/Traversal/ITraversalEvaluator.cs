namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Evaluates traversal decisions for filesystem entries.
/// </summary>
internal interface ITraversalEvaluator
{
    TraversalDecision Evaluate(
        MatchOutcome matchOutcome,
        PathKind pathKind,
        int depth
    );
}
