namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Represents an ordered, immutable set of compiled patterns that can be evaluated against paths.
/// </summary>
internal interface ICompiledPatternSet : IReadOnlyList<ICompiledPattern> {
    /// <summary>Gets the underlying pattern collection.</summary>
    IReadOnlyList<ICompiledPattern> Patterns { get; }

    /// <summary>Gets a subset of GitIgnore patterns.</summary>
    [MemberNotNullWhen(true, nameof(HasGitIgnore))]
    ICompiledPatternSet? GitIgnoreSubSet { get; }

    /// <summary>Gets a subset of Glob patterns.</summary>
    [MemberNotNullWhen(true, nameof(HasGlob))]
    ICompiledPatternSet? GlobSubSet { get; }

    /// <summary>Gets a subset of Regex patterns.</summary>
    [MemberNotNullWhen(true, nameof(HasRegex))]
    ICompiledPatternSet? RegexSubSet { get; }

    /// <summary>Checks if this set contains GitIgnore patterns.</summary>
    bool HasGitIgnore { get; }
    /// <summary>Checks if this set contains Glob patterns.</summary>
    bool HasGlob { get; }
    /// <summary>Checks if this set contains Regex patterns.</summary>
    bool HasRegex { get; }

    /// <summary>Groups patterns by their <see cref="PatternKind"/>.</summary>
    IEnumerable<(PatternKind PatternKind, ICompiledPatternSet Patterns)> GroupByType();

    /// <summary>Finds patterns anchored to the root directory.</summary>
    IEnumerable<ICompiledPattern> AnchoredToRoot();

    /// <summary>Finds patterns that apply only to directories.</summary>
    IEnumerable<ICompiledPattern> DirectoryOnly();

    /// <summary>Finds patterns that explicitly negate matches.</summary>
    IEnumerable<ICompiledPattern> FindNegated();

    /// <summary>Finds patterns that provide positive matches.</summary>
    IEnumerable<ICompiledPattern> FindPositive();

    /// <summary>Finds all patterns of the specified <paramref name="type"/>.</summary>
    /// <param name="type">The type of patterns to find.</param>
    IEnumerable<ICompiledPattern> OfType(PatternKind type);
}
