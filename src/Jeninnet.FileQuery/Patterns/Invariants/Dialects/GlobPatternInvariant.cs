//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Dialects;

/// <summary>
/// Glob-specific invariants.
/// </summary>
internal sealed class GlobPatternInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Semantic;
    public PatternKind? AppliesTo => PatternKind.Glob;

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        // Example: recursive wildcard must be standalone segment.
        foreach(var segment in context.Tokens ?? []) {
            var recursive = segment.Where(static token => token is RecursiveWildcardToken).ToList();
            if(recursive.Count > 0 && segment.Count > 1) {
                return PatternInvariantResult.Fail("In Glob patterns, '**' must appear as a standalone segment.");
            }
        }

        return PatternInvariantResult.Success;
    }
}
