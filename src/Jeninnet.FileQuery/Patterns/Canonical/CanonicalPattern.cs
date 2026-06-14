namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents a normalized, explicit pattern.
/// </summary>
/// <param name="Text">The pattern text.</param>
/// <param name="ExplicitType">The optional explicit type of the pattern.</param>
public sealed record CanonicalPattern(string Text, PatternKind? ExplicitType);
