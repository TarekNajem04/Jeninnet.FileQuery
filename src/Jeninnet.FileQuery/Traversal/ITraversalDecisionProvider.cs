//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Defines an interface for providing traversal decisions based on match outcomes and current traversal state.
/// </summary>
internal interface ITraversalDecisionProvider {
    TraversalDecision Decide(MatchOutcome outcome, PathKind kind, int depth);
}
