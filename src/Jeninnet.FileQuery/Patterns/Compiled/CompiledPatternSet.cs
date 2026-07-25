namespace Jeninnet.FileQuery.Patterns.Compiled;

/// <summary>
/// Represents an ordered, immutable set of compiled patterns produced by an <see cref="IPatternCompiler"/>.
/// </summary>
/// <remarks>
/// <para>
/// Pattern order is semantically significant and must be preserved.
/// </para>
///
/// <para>
/// <strong>Sub-set performance design:</strong>
/// Sub-sets (<see cref="GitIgnoreSubSet"/>, <see cref="GlobSubSet"/>,
/// <see cref="RegexSubSet"/>) are built with a single pass over thepattern list using lazy list allocation —
/// a sub-list is only created when the first pattern of that kind is encountered.
/// This eliminates the two wasted list allocations that previously occurred for every
/// unused kind (e.g., compiling two GitIgnore patterns no longer allocates Glob and Regex lists).
/// </para>
/// </remarks>
internal sealed record CompiledPatternSet : ICompiledPatternSet {
    private static readonly Lazy<CompiledPatternSet> _emptyInstance =
        new(static () => new CompiledPatternSet(patterns: []));

    /// <summary>Gets the shared singleton empty set.</summary>
    public static readonly CompiledPatternSet Empty = _emptyInstance.Value;

    /// <inheritdoc/>
    public IReadOnlyList<ICompiledPattern> Patterns { get; }

    /// <inheritdoc/>
    public ICompiledPatternSet? GitIgnoreSubSet { get; }

    /// <inheritdoc/>
    public ICompiledPatternSet? GlobSubSet { get; }

    /// <inheritdoc/>
    public ICompiledPatternSet? RegexSubSet { get; }

    /// <inheritdoc/>
    public bool HasGitIgnore => GitIgnoreSubSet is not null;

    /// <inheritdoc/>
    public bool HasGlob => GlobSubSet is not null;

    /// <inheritdoc/>
    public bool HasRegex => RegexSubSet is not null;

    /// <summary>
    /// Initializes a new <see cref="CompiledPatternSet"/> and partitions
    /// <paramref name="patterns"/> into per-kind sub-sets.
    /// </summary>
    /// <param name="patterns">The ordered compiled patterns. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="patterns"/> is <see langword="null"/>.
    /// </exception>
    internal CompiledPatternSet(IReadOnlyList<ICompiledPattern> patterns)
        : this(patterns, createSubSets: true) { }

    /// <summary>
    /// Private constructor used when building sub-sets to avoid recursive
    /// sub-set creation.
    /// </summary>
    /// <param name="patterns">The ordered compiled patterns.</param>
    /// <param name="createSubSets">Indicates whether to partition patterns into sub-sets.</param>
    private CompiledPatternSet(IReadOnlyList<ICompiledPattern> patterns, bool createSubSets) {
        ArgumentNullException.ThrowIfNull(patterns);

        Patterns = patterns;

        if(!createSubSets) {
            return;
        }

        // LAZY ALLOCATION: sub-lists are only created when the first pattern of
        // that kind is encountered. For a pure GitIgnore set of N patterns, the
        // previous implementation allocated three List<T> of capacity N each
        // (one for git, glob, regex) and then discarded two of them. This
        // implementation allocates zero lists for unused kinds.
        List<ICompiledPattern>? git = null;
        List<ICompiledPattern>? glob = null;
        List<ICompiledPattern>? regex = null;

        for(var i = 0; i < patterns.Count; i++) {
            var pattern = patterns[i];

            switch(pattern.PatternKind) {
                case PatternKind.GitIgnore:
                    (git ??= []).Add(pattern);
                    break;

                case PatternKind.Glob:
                    (glob ??= []).Add(pattern);
                    break;

                case PatternKind.Regex:
                    (regex ??= []).Add(pattern);
                    break;
            }
        }

        if(git is not null) {
            GitIgnoreSubSet = new CompiledPatternSet(git.AsReadOnly(), createSubSets: false);
        }

        if(glob is not null) {
            GlobSubSet = new CompiledPatternSet(glob.AsReadOnly(), createSubSets: false);
        }

        if(regex is not null) {
            RegexSubSet = new CompiledPatternSet(regex.AsReadOnly(), createSubSets: false);
        }
    }

    /// <inheritdoc/>
    public int Count => Patterns.Count;

    /// <inheritdoc/>
    public ICompiledPattern this[int index] => Patterns[index];

    /// <inheritdoc/>
    public IEnumerator<ICompiledPattern> GetEnumerator() => Patterns.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerable<(PatternKind PatternKind, ICompiledPatternSet Patterns)> GroupByType() => Patterns.GroupBy(static p => p.PatternKind)
                .Select(static g =>
                    (g.Key, (ICompiledPatternSet)new CompiledPatternSet(g.ToList().AsReadOnly())));

    /// <inheritdoc/>
    public IEnumerable<ICompiledPattern> FindNegated() => Patterns.Where(static p => p.IsNegated);

    /// <inheritdoc/>
    public IEnumerable<ICompiledPattern> FindPositive() => Patterns.Where(static p => !p.IsNegated);

    /// <inheritdoc/>
    public IEnumerable<ICompiledPattern> OfType(PatternKind type) {
        if(type is PatternKind.Unknown) {
            return [];
        }

        var buffer = new List<ICompiledPattern>(Patterns.Count);

        foreach(var p in Patterns) {
            if(p.PatternKind == type) {
                buffer.Add(p);
            }
        }

        return buffer;
    }

    /// <inheritdoc/>
    public IEnumerable<ICompiledPattern> DirectoryOnly() => Patterns.Where(static p => p.DirectoryOnly);

    /// <inheritdoc/>
    public IEnumerable<ICompiledPattern> AnchoredToRoot() => Patterns.Where(static p => p.AnchoredToRoot);

    /// <inheritdoc/>
    public bool Equals(CompiledPatternSet? other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        if(Count != other.Count) {
            return false;
        }

        for(var i = 0; i < Count; i++) {
            if(!Equals(this[i], other[i])) {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
        unchecked {
            var hash = 17;

            foreach(var p in Patterns) {
                hash = (hash * 31) + (p?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
