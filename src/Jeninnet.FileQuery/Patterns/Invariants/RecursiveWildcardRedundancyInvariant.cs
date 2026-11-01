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
            if(segment.Count(static t => t is RecursiveWildcardToken) > 1) {
                var first = PatternTextPosition.Find(context.Pattern.Text.AsSpan(), "**");
                var second = first is { } start
                    ? PatternTextPosition.FindAfter(context.Pattern.Text.AsSpan(), "**", start + 2)
                    : null;

                return PatternInvariantResult.Fail(
                    "Recursive wildcard (**) must not appear more than once in the same segment.",
                    second,
                    second is { } value ? PatternTextPosition.SegmentAt(context.Pattern.Text.AsSpan(), value) : null);
            }

            var currentIsRecursive =
                segment.Count == 1 &&
                segment[0] is RecursiveWildcardToken;

            // Rule 2: adjacent ** segments
            if(previousWasRecursive && currentIsRecursive) {
                var first = PatternTextPosition.Find(context.Pattern.Text.AsSpan(), "**");
                var second = first is { } start
                    ? PatternTextPosition.FindAfter(context.Pattern.Text.AsSpan(), "**", start + 2)
                    : null;

                return PatternInvariantResult.Fail(
                    "Redundant recursive wildcard (**/**) detected.",
                    second,
                    second is { } value ? PatternTextPosition.SegmentAt(context.Pattern.Text.AsSpan(), value) : null);
            }

            previousWasRecursive = currentIsRecursive;
        }

        return PatternInvariantResult.Success;
    }
}
