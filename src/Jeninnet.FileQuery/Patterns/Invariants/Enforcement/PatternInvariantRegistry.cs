namespace Jeninnet.FileQuery.Patterns.Invariants.Enforcement;

/// <summary>
/// Executes pattern invariants grouped by compilation phase.
/// </summary>
internal sealed class PatternInvariantRegistry {
    private readonly IPatternInvariant[] _lexical;
    private readonly IPatternInvariant[] _structural;
    private readonly IPatternInvariant[] _semantic;

    public PatternInvariantRegistry(IEnumerable<IPatternInvariant> invariants) {
        var grouped = invariants.GroupBy(i => i.Phase);

        _lexical = grouped.FirstOrDefault(g => g.Key == PatternInvariantPhase.Lexical)?.ToArray() ?? [];
        _structural = grouped.FirstOrDefault(g => g.Key == PatternInvariantPhase.Structural)?.ToArray() ?? [];
        _semantic = grouped.FirstOrDefault(g => g.Key == PatternInvariantPhase.Semantic)?.ToArray() ?? [];
    }

    public void ValidateLexical(PatternCompilationContext context) => Validate(_lexical, context);

    public void ValidateStructural(PatternCompilationContext context) => Validate(_structural, context);

    public void ValidateSemantic(PatternCompilationContext context) => Validate(_semantic, context);

    private static void Validate(IPatternInvariant[] invariants, PatternCompilationContext context) {
        foreach(var invariant in invariants) {
            if(!AppliesTo(invariant, context)) {
                continue;
            }

            var result = invariant.Validate(context);
            if(!result.IsSuccess) {
                throw new PatternException(result.Message!);
            }
        }
    }

    private static bool AppliesTo(IPatternInvariant invariant, PatternCompilationContext context) =>
        invariant.AppliesTo is null ||
        invariant.AppliesTo == context.Pattern.Type;
}
