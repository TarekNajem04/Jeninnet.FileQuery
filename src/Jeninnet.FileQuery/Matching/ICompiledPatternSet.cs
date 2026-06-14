namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Represents an ordered, immutable set of compiled patterns that can be evaluated against paths.
/// </summary>
internal interface ICompiledPatternSet : IReadOnlyList<ICompiledPattern>
{
    IReadOnlyList<ICompiledPattern> Patterns { get; }

    [MemberNotNullWhen(true, nameof(HasGitIgnore))]
    ICompiledPatternSet? GitIgnoreSubSet { get; }

    [MemberNotNullWhen(true, nameof(HasGlob))]
    ICompiledPatternSet? GlobSubSet { get; }

    [MemberNotNullWhen(true, nameof(HasRegex))]
    ICompiledPatternSet? RegexSubSet { get; }

    bool HasGitIgnore { get; }
    bool HasGlob { get; }
    bool HasRegex { get; }

    IEnumerable<(PatternKind PatternKind, ICompiledPatternSet Patterns)> GroupByType();
    IEnumerable<ICompiledPattern> AnchoredToRoot();
    IEnumerable<ICompiledPattern> DirectoryOnly();
    IEnumerable<ICompiledPattern> FindNegated();
    IEnumerable<ICompiledPattern> FindPositive();
    IEnumerable<ICompiledPattern> OfType(PatternKind type);
}
