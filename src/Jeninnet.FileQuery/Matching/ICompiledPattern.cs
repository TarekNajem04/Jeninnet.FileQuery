namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Represents a compiled, immutable pattern used for matching file paths.
/// </summary>
internal interface ICompiledPattern {
    /// <summary>
    /// Gets a value indicating whether the pattern begins with <c>!</c>, negating matches.
    /// </summary>
    bool IsNegated { get; }

    /// <summary>
    /// Gets a value indicating whether the pattern applies only to directories (ends with '/').
    /// </summary>
    bool DirectoryOnly { get; }

    /// <summary>
    /// Gets a value indicating whether the pattern is anchored to the root directory (starts with '/').
    /// </summary>
    bool AnchoredToRoot { get; }

    /// <summary>
    /// Gets the compiled pattern segments, immutable and safe for public exposure.
    /// </summary>
    IReadOnlyList<IReadOnlyList<IPatternToken>> Segments { get; }

    /// <summary>
    /// Gets the type of pattern (Glob, GitIgnore, Regex, etc.) responsible for evaluation.
    /// </summary>
    PatternKind PatternKind { get; }

    /// <summary>
    /// Gets the compiled match intent.
    /// </summary>
    CompiledMatchIntent Intent { get; }

    /// <summary>
    /// Gets the original source pattern text.
    /// </summary>
    string SourceText { get; }

    /// <summary>
    /// Gets the zero-based source pattern index, or -1 when unknown.
    /// </summary>
    int SourceIndex { get; }

    /// <summary>
    /// Gets the pre-calculated concrete path anchor (literal prefix) for this pattern.
    /// Used during directory traversal decisions.
    /// </summary>
    string ConcretePathAnchor { get; }

    /// <summary>
    /// Gets the trailing literal suffix of the last pattern segment, when one exists.
    /// Every path that the pattern can match must end with this suffix; matchers use
    /// it as a zero-allocation rejection fast path before entering the recursive
    /// segment matcher. Empty when no fixed suffix exists or when the suffix is
    /// unsafe to apply (e.g. directory-only patterns).
    /// </summary>
    string LiteralSuffix { get; }

    /// <summary>
    /// Gets the raw regex string if the pattern is a Regex kind.
    /// </summary>
    string? RegexText { get; }
}
