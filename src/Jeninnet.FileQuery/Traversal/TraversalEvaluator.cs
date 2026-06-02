namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Translates matcher results and options into traversal decisions.
/// </summary>
internal sealed class TraversalEvaluator(ITraversalDecisionProvider provider) : ITraversalEvaluator {
    public TraversalDecision Evaluate(
        MatchOutcome matchOutcome,
        PathKind pathKind,
        int depth
    ) => provider.Decide(matchOutcome, pathKind, depth);
}
