namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents the fully canonicalized pattern input.
/// </summary>
/// <remarks>
/// This is the only input accepted by the classifier.
/// </remarks>
public sealed record CanonicalPatternSet {
    public IReadOnlyList<CanonicalPattern> Patterns { get; init; } = [];
}
