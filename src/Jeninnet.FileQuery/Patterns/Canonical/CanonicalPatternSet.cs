namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents the fully canonicalized pattern input.
/// </summary>
/// <remarks>
/// This is the only input accepted by the classifier.
/// </remarks>
public sealed record CanonicalPatternSet {
    /// <summary>
    /// Gets the list of canonical patterns.
    /// </summary>
    public IReadOnlyList<CanonicalPattern> Patterns { get; init; } = [];
}
