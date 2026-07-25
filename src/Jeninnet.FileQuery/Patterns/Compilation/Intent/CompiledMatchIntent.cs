namespace Jeninnet.FileQuery.Patterns.Compilation.Intent;

/// <summary>
/// Represents the match intent of a compiled pattern (either to include or exclude).
/// </summary>
internal readonly struct CompiledMatchIntent {
    private CompiledMatchIntent(MatchOutcome outcome) => Outcome = outcome;

    /// <summary>
    /// Gets the underlying match outcome associated with this intent.
    /// </summary>
    public MatchOutcome Outcome { get; }

    /// <summary>
    /// Gets the match intent that specifies inclusion.
    /// </summary>
    public static CompiledMatchIntent Include { get; }
        = new(MatchOutcome.Include);

    /// <summary>
    /// Gets the match intent that specifies exclusion.
    /// </summary>
    public static CompiledMatchIntent Exclude { get; }
        = new(MatchOutcome.Exclude);

    /// <summary>
    /// Creates a compiled match intent based on whether the pattern is negated.
    /// </summary>
    /// <param name="isNegated">True if the pattern is negated; otherwise, false.</param>
    /// <returns>A <see cref="CompiledMatchIntent"/> instance.</returns>
    public static CompiledMatchIntent FromNegation(bool isNegated) => isNegated ? Exclude : Include;
}
