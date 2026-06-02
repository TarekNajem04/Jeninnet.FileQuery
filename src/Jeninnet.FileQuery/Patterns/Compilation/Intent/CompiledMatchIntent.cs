namespace Jeninnet.FileQuery.Patterns.Compilation.Intent;

internal readonly struct CompiledMatchIntent {
    private CompiledMatchIntent(MatchOutcome outcome) => Outcome = outcome;

    public MatchOutcome Outcome { get; }

    public static CompiledMatchIntent Include { get; }
        = new(MatchOutcome.Include);

    public static CompiledMatchIntent Exclude { get; }
        = new(MatchOutcome.Exclude);

    public static CompiledMatchIntent FromNegation(bool isNegated)
        => isNegated ? Exclude : Include;
}
