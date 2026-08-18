//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
