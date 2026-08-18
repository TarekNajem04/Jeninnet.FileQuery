//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

internal sealed class EmptyPatternInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Lexical;

    public PatternKind? AppliesTo { get; }

    public PatternInvariantResult Validate(PatternCompilationContext context) =>
        string.IsNullOrWhiteSpace(context.Pattern.Text)
            ? PatternInvariantResult.Fail("Pattern cannot be empty.")
            : PatternInvariantResult.Success;
}
