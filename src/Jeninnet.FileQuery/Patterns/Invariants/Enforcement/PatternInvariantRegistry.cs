//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Enforcement;

/// <summary>
/// Executes pattern invariants grouped by compilation phase.
/// </summary>
internal sealed class PatternInvariantRegistry {
    private readonly IPatternInvariant[] _lexical;
    private readonly IPatternInvariant[] _structural;
    private readonly IPatternInvariant[] _semantic;

    public PatternInvariantRegistry(IEnumerable<IPatternInvariant> invariants) {
        var grouped = invariants.GroupBy(static i => i.Phase);

        _lexical = grouped.FirstOrDefault(static g => g.Key == PatternInvariantPhase.Lexical)?.ToArray() ?? [];
        _structural = grouped.FirstOrDefault(static g => g.Key == PatternInvariantPhase.Structural)?.ToArray() ?? [];
        _semantic = grouped.FirstOrDefault(static g => g.Key == PatternInvariantPhase.Semantic)?.ToArray() ?? [];
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
