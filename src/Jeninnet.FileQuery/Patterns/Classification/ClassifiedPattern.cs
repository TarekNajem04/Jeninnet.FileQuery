namespace Jeninnet.FileQuery.Patterns.Classification;

/// <summary>
/// Represents a pattern with fully resolved semantics.
/// </summary>
/// <param name="Text">The pattern text.</param>
/// <param name="Type">The pattern kind.</param>
/// <param name="SourceIndex">The index of the pattern in the source.</param>
internal sealed record ClassifiedPattern(string Text, PatternKind Type, int SourceIndex = -1);
