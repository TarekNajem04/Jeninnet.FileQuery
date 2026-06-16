namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Evaluates traversal decisions for filesystem entries.
/// </summary>
internal interface ITraversalEvaluator
{
    /// <summary>
    /// Evaluates traversal decisions for filesystem entries based on the match outcome,
    /// entry kind, and current traversal depth.
    /// </summary>
    /// <param name="matchOutcome">The outcome from pattern matching.</param>
    /// <param name="pathKind">The kind of filesystem entry (file/directory).</param>
    /// <param name="depth">The current depth in the traversal tree.</param>
    /// <returns>A traversal decision.</returns>
    TraversalDecision Evaluate(
        MatchOutcome matchOutcome,
        PathKind pathKind,
        int depth
    );
}
