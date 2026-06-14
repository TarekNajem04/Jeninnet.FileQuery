namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Translates matcher results and options into traversal decisions.
/// </summary>
/// <param name="provider">The traversal decision provider.</param>
internal sealed class TraversalEvaluator(ITraversalDecisionProvider provider) : ITraversalEvaluator
{
    /// <summary>
    /// Evaluates the match outcome to determine the traversal decision.
    /// </summary>
    /// <param name="matchOutcome">The outcome of the path match.</param>
    /// <param name="pathKind">The kind of path being evaluated.</param>
    /// <param name="depth">The current depth in the directory traversal.</param>
    public TraversalDecision Evaluate(
        MatchOutcome matchOutcome,
        PathKind pathKind,
        int depth
    ) => provider.Decide(matchOutcome, pathKind, depth);
}
