namespace Jeninnet.FileQuery.Patterns.Classification;

/// <summary>
/// Represents the output of pattern classification.
/// </summary>
/// <remarks>
/// This structure is the only valid input to pattern compilation.
/// </remarks>
internal sealed record ClassifiedPatternSet {
    public IReadOnlyList<ClassifiedPattern> Patterns { get; init; } = [];
}
