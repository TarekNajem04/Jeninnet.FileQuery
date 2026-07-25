namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Validates invariants on raw pattern text before compilation.
/// </summary>
internal interface ITextPatternInvariant {
    PatternInvariantResult Validate(string pattern);
}
