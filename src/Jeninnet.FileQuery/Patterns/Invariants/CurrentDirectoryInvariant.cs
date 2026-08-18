//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Ensures that patterns don’t illegally reference current directory sequences.
/// </summary>
internal sealed class CurrentDirectoryInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;
    public PatternKind? AppliesTo { get; }

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        if(context.Tokens!.Any(static seg => seg.Count == 1 && seg[0] is LiteralToken { Text: "." })) {
            return PatternInvariantResult.Fail("'.' is not a valid pattern segment.");
        }

        return PatternInvariantResult.Success;
    }
}
