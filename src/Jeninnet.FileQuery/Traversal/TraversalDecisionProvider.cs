//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

internal sealed class TraversalDecisionProvider : ITraversalDecisionProvider {
    private readonly TraversalConfiguration _traversalConfiguration;

    internal TraversalDecisionProvider(TraversalConfiguration traversalConfiguration) => _traversalConfiguration = traversalConfiguration;

    public TraversalDecision Decide(MatchOutcome outcome, PathKind kind, int depth) {
        if(kind is PathKind.Directory) {
            // Determines whether a directory should be traversed based on match result and recursion constraints.
            var shouldTraverse =
                // traverse if recursion is enabled
                outcome is not MatchOutcome.Exclude &&
                _traversalConfiguration.RecurseSubdirectories &&
                (
                    _traversalConfiguration.MaxRecursionDepth is TraversalConfiguration.UNLIMITED_RECURSION_DEPTH ||
                    depth < _traversalConfiguration.MaxRecursionDepth
                );

            return new TraversalDecision(
                ShouldYield: false,
                ShouldTraverse: shouldTraverse
            );
        }

        var shouldYield = outcome is MatchOutcome.Include;
        return new TraversalDecision(ShouldYield: shouldYield, ShouldTraverse: false);
    }
}
