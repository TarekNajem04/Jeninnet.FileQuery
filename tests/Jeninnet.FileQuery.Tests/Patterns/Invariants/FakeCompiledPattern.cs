namespace Jeninnet.FileQuery.Tests.Patterns.Invariants;

internal sealed class FakeCompiledPattern : ICompiledPattern {
    public bool IsNegated => false;
    public bool DirectoryOnly => false;
    public bool AnchoredToRoot => false;
    public IReadOnlyList<IReadOnlyList<IPatternToken>> Segments => [];
    public PatternKind PatternKind => PatternKind.Glob;
    public CompiledMatchIntent Intent => CompiledMatchIntent.Include;
    public string SourceText => "test";
    public int SourceIndex => 0;
    public string ConcretePathAnchor => "";
    public string LiteralSuffix => string.Empty;
    public string? RegexText => null;
}

