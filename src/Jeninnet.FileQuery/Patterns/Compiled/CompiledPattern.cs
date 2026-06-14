namespace Jeninnet.FileQuery.Patterns.Compiled;

/// <summary>
/// Represents a fully compiled, immutable pattern expression produced by a
/// <see cref="PatternScanner"/> implementation (GitIgnore, Glob, etc.).
/// </summary>
/// <remarks>
/// ARCHITECTURAL CONTRACT:
/// <list type="bullet">
/// <item>This type represents a fully validated, immutable pattern.</item>
/// <item>Instances must only be created by PatternCompilerBase.</item>
/// <item>Consumers must treat this as a read-only data structure.</item>
/// </list>
/// </remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed record CompiledPattern : ICompiledPattern
{
    /// <summary>
    /// True if the pattern begins with <c>!</c>, negating matches.
    /// </summary>
    public bool IsNegated { get; init; }

    /// <summary>
    /// True if the pattern applies only to directories (ends with '/').
    /// </summary>
    public bool DirectoryOnly { get; init; }

    /// <inheritdoc/>
    public bool AnchoredToRoot { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IReadOnlyList<IPatternToken>> Segments { get; init; }

    /// <inheritdoc/>
    public PatternKind PatternKind { get; init; }

    public CompiledMatchIntent Intent { get; init; }

    /// <inheritdoc/>
    public string SourceText { get; init; }

    /// <inheritdoc/>
    public int SourceIndex { get; init; }

    internal CompiledPattern(
        bool isNegated,
        bool directoryOnly,
        bool anchoredToRoot,
        IReadOnlyList<IReadOnlyList<IPatternToken>> segments,
        PatternKind patternKind,
        CompiledMatchIntent intent,
        string sourceText = "",
        int sourceIndex = -1
    )
    {
        IsNegated = isNegated;
        DirectoryOnly = directoryOnly;
        AnchoredToRoot = anchoredToRoot;
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        PatternKind = patternKind;
        Intent = intent;
        SourceText = sourceText;
        SourceIndex = sourceIndex;
    }

    /// <summary>
    /// Returns a debug string representation of this compiled pattern.
    /// </summary>
    public override string ToString() =>
        string.Join("/", Segments.Select(seg => string.Concat(seg.Select(t => t.ToString() ?? "*"))));

    private string GetDebuggerDisplay() => ToString();
}
