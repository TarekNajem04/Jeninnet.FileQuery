namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Ensures that patterns don’t illegally reference current directory sequences.
/// </summary>
internal sealed class CurrentDirectoryInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;
    public PatternKind? AppliesTo { get; }

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        if(context.Tokens!.Any(seg => seg.Count == 1 && seg[0] is LiteralToken { Text: "." })) {
            return PatternInvariantResult.Fail("'.' is not a valid pattern segment.");
        }

        return PatternInvariantResult.Success;
    }
}
