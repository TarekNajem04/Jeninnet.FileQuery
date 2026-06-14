namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

internal interface IPatternInvariantRegistry
{
    PatternInvariantResult Validate(ReadOnlySpan<char> pattern);
}
