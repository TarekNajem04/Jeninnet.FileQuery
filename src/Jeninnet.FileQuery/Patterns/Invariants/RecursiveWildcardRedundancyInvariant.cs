//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Ensures recursive wildcards (**) are not redundantly specified.
/// </summary>
internal sealed class RecursiveWildcardRedundancyInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Semantic;
    public PatternKind? AppliesTo => null;

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Tokens);

        var segments = context.Tokens;
        var previousWasRecursive = false;

        for(var i = 0; i < segments.Count; i++) {
            var segment = segments[i];

            // Rule 1: multiple ** in same segment
            if(HasMultipleRecursiveWildcards(segment)) {
                return CreateFailure(
                    context,
                    "Recursive wildcard (**) must not appear more than once in the same segment.");
            }

            var currentIsRecursive = IsWholeRecursiveWildcard(segment);

            // Rule 2: adjacent ** segments
            if(previousWasRecursive && currentIsRecursive) {
                return CreateFailure(
                    context,
                    "Redundant recursive wildcard (**/**) detected.");
            }

            previousWasRecursive = currentIsRecursive;
        }

        return PatternInvariantResult.Success;
    }

    /// <summary>
    /// Determines whether a segment contains more than one recursive wildcard token.
    /// </summary>
    /// <param name="segment">The pattern segment.</param>
    /// <returns><see langword="true"/> when the segment contains duplicate recursive wildcards.</returns>
    private static bool HasMultipleRecursiveWildcards(List<IPatternToken> segment) =>
        segment.Count(static token => token is RecursiveWildcardToken) > 1;

    /// <summary>
    /// Determines whether a segment consists of exactly one recursive wildcard token.
    /// </summary>
    /// <param name="segment">The pattern segment.</param>
    /// <returns><see langword="true"/> when the segment is a standalone recursive wildcard.</returns>
    private static bool IsWholeRecursiveWildcard(List<IPatternToken> segment) =>
        segment.Count == 1 && segment[0] is RecursiveWildcardToken;

    /// <summary>
    /// Creates a failure result for the second recursive wildcard occurrence.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>The failure result.</returns>
    private static PatternInvariantResult CreateFailure(PatternCompilationContext context, string message) {
        var span = context.Pattern.Text.AsSpan();
        var first = PatternTextPosition.Find(span, "**");
        var second = first is { } start
            ? PatternTextPosition.FindAfter(span, "**", start + 2)
            : (int?)null;
        var segment = second is { } value ? PatternTextPosition.SegmentAt(span, value) : (int?)null;

        return PatternInvariantResult.Fail(message, second, segment);
    }
}
