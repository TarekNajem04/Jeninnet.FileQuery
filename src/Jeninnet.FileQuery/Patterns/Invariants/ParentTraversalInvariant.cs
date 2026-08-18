//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

internal sealed class ParentTraversalInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;
    public PatternKind? AppliesTo { get; }

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        if(context.Tokens!.Any(static seg => seg.Count == 1 &&
                                     seg[0] is LiteralToken { Text: ".." })) {
            return PatternInvariantResult.Fail("'..' traversal is not allowed.");
        }

        return PatternInvariantResult.Success;
    }
}
