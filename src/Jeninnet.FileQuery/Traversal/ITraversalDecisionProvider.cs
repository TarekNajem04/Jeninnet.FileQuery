namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Defines an interface for providing traversal decisions based on match outcomes and current traversal state.
/// </summary>
internal interface ITraversalDecisionProvider
{
    TraversalDecision Decide(MatchOutcome outcome, PathKind kind, int depth);
}
