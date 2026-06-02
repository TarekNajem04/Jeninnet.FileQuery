namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents a normalized, explicit pattern.
/// </summary>
public sealed record CanonicalPattern(string Text, PatternKind? ExplicitType);
