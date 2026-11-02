//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Locates the compiled pattern that decided a match outcome.
/// </summary>
/// <remarks>
/// This helper is shared by traversal diagnostics and the single-path matcher so both
/// surfaces report identical explanations.
/// </remarks>
internal static class MatchExplanation {
    /// <summary>
    /// Finds the pattern responsible for the given match outcome.
    /// </summary>
    /// <param name="patterns">The compiled pattern set.</param>
    /// <param name="context">The path match context.</param>
    /// <param name="finalOutcome">The final match outcome.</param>
    /// <returns>The deciding pattern, or <see langword="null"/> when no pattern matched.</returns>
    public static ICompiledPattern? FindResponsiblePattern(
        ICompiledPatternSet patterns,
        PathMatchContext context,
        MatchOutcome finalOutcome
    ) {
        if(patterns.Count == 0 || finalOutcome is MatchOutcome.NoMatch) {
            return null;
        }

        var lastGitIgnoreMatch = FindLastGitIgnoreMatch(patterns, context);

        if(finalOutcome is MatchOutcome.Include) {
            // A GitIgnore Include short-circuits resolution. It is produced either
            // by the last matching negation rule or by the default-include state
            // when no GitIgnore rule matched.
            if(lastGitIgnoreMatch is not null) {
                if(lastGitIgnoreMatch.IsNegated) {
                    return lastGitIgnoreMatch;
                }
            } else if(patterns.GitIgnoreSubSet is { Count: > 0 }) {
                // The GitIgnore subset returned its default Include without a rule
                // match, so Glob and Regex were never consulted.
                return null;
            }

            // GitIgnore excluded the path (or has no subset); a Glob/Regex rule
            // re-included it and is the deciding pattern.
            return FindMatchInBuckets(patterns, context);
        }

        // Only the GitIgnore subset can produce an Exclude. Glob and Regex act as
        // re-inclusion fallbacks, so the last applying GitIgnore rule is responsible.
        return finalOutcome is MatchOutcome.Exclude ? lastGitIgnoreMatch : null;
    }

    /// <summary>
    /// Gets the default diagnostic reason for a given match outcome.
    /// </summary>
    /// <param name="outcome">The match outcome.</param>
    /// <returns>The default explanation.</returns>
    public static string GetDefaultReason(MatchOutcome outcome) =>
        outcome switch {
            MatchOutcome.Include => "Included by default because no excluding pattern matched.",
            MatchOutcome.Exclude => "Excluded without source pattern metadata.",
            _ => "No pattern matched this entry."
        };

    private static ICompiledPattern? FindLastGitIgnoreMatch(ICompiledPatternSet patterns, PathMatchContext context) {
        if(patterns.GitIgnoreSubSet is null) {
            return null;
        }

        ICompiledPattern? lastMatch = null;
        for(var i = 0; i < patterns.GitIgnoreSubSet.Count; i++) {
            var pattern = patterns.GitIgnoreSubSet[i];
            if(MatchPrecedenceResolver.GitIgnoreMatcher.AppliesToPattern(pattern, context)) {
                lastMatch = pattern;
            }
        }

        return lastMatch;
    }

    private static ICompiledPattern? FindMatchInBuckets(ICompiledPatternSet patterns, PathMatchContext context) {
        if(patterns.GlobSubSet is not null) {
            for(var i = 0; i < patterns.GlobSubSet.Count; i++) {
                var pattern = patterns.GlobSubSet[i];
                if(MatchPrecedenceResolver.GlobMatcher.Match(pattern, context) is MatchOutcome.Include) {
                    return pattern;
                }
            }
        }

        if(patterns.RegexSubSet is not null) {
            for(var i = 0; i < patterns.RegexSubSet.Count; i++) {
                var pattern = patterns.RegexSubSet[i];
                if(MatchPrecedenceResolver.RegexMatcher.Match(pattern, context) is MatchOutcome.Include) {
                    return pattern;
                }
            }
        }

        return null;
    }
}
