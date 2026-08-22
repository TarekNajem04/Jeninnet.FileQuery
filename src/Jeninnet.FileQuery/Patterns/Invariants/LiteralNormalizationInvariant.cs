//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Ensures literals are normalized (case or unicode normalization).
/// </summary>
internal sealed class LiteralNormalizationInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Lexical;
    public PatternKind? AppliesTo { get; }

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        var pattern = context.Pattern.Text.AsSpan();
        for(var i = 0; i < pattern.Length; i++) {
            if(char.IsControl(pattern[i])) {
                return PatternInvariantResult.Fail("Pattern contains invalid control characters.");
            }
        }

        return PatternInvariantResult.Success;
    }
}
