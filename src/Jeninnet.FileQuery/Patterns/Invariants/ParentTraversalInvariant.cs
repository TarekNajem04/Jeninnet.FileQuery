namespace Jeninnet.FileQuery.Patterns.Invariants;

internal sealed class ParentTraversalInvariant : IPatternInvariant
{
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;
    public PatternKind? AppliesTo { get; }

    public PatternInvariantResult Validate(PatternCompilationContext context)
    {
        if(context.Tokens!.Any(seg => seg.Count == 1 &&
                                     seg[0] is LiteralToken { Text: ".." }))
        {
            return PatternInvariantResult.Fail("'..' traversal is not allowed.");
        }

        return PatternInvariantResult.Success;
    }
}
