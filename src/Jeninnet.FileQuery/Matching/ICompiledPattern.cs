namespace Jeninnet.FileQuery.Matching;

internal interface ICompiledPattern
{
    /// <summary>
    /// True if the pattern begins with <c>!</c>, negating matches.
    /// </summary>
    bool IsNegated { get; }

    /// <summary>
    /// True if the pattern applies only to directories (ends with '/').
    /// </summary>
    bool DirectoryOnly { get; }

    /// <summary>
    /// True if the pattern is anchored to the root directory (starts with '/').
    /// </summary>
    bool AnchoredToRoot { get; }

    /// <summary>
    /// The compiled pattern segments, immutable and safe for public exposure.
    /// </summary>
    IReadOnlyList<IReadOnlyList<IPatternToken>> Segments { get; }

    /// <summary>
    /// The type of pattern (Glob, GitIgnore, Regex, etc.) responsible for evaluation.
    /// </summary>
    PatternKind PatternKind { get; }

    CompiledMatchIntent Intent { get; }

    /// <summary>
    /// Gets the original source pattern text.
    /// </summary>
    string SourceText { get; }

    /// <summary>
    /// Gets the zero-based source pattern index, or -1 when unknown.
    /// </summary>
    int SourceIndex { get; }
}
