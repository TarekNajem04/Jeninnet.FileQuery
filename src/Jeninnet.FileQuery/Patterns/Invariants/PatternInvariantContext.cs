namespace Jeninnet.FileQuery.Patterns.Invariants;

internal sealed record PatternInvariantContext
{
    public string? Text { get; init; }
    public IReadOnlyList<IReadOnlyList<IPatternToken>>? Segments { get; init; }
    public ClassifiedPattern? Classified { get; init; }
    public ICompiledPattern? Compiled { get; init; }
}
