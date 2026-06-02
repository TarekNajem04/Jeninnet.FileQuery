namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Represents a single pattern invariant that can validate a pattern.
/// </summary>
internal interface IPatternInvariant {
    /// <summary>
    /// The execution phase of this invariant.
    /// </summary>
    PatternInvariantPhase Phase { get; }

    /// <summary>
    /// Optional filter — invariant applies only to specific pattern types.
    /// </summary>
    PatternKind? AppliesTo { get; }

    /// <summary>
    /// Validates the given pattern.
    /// </summary>
    PatternInvariantResult Validate(PatternCompilationContext context);
}
