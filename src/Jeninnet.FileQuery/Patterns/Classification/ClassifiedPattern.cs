namespace Jeninnet.FileQuery.Patterns.Classification;

/// <summary>
/// Represents a pattern with fully resolved semantics.
/// </summary>
internal sealed record ClassifiedPattern(string Text, PatternKind Type);
