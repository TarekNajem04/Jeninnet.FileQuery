namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Identifies the pattern syntax used by a compiled pattern.
/// </summary>
/// <remarks>
/// This describes the <em>dialect</em> of an individual pattern, not the overall matching mode.
/// A <see cref="PatternMatchingMode"/> determines how patterns are interpreted during evaluation.
/// </remarks>
public enum PatternKind {
    /// <summary>
    /// GitIgnore‑style pattern syntax (anchoring, directory‑only, <c>**</c>).
    /// </summary>
    GitIgnore,

    /// <summary>
    /// Classic glob syntax (<c>*</c>, <c>?</c>) applied to the full path.
    /// </summary>
    Glob,

    /// <summary>
    /// Regular expression pattern applied to the full normalized path.
    /// </summary>
    Regex,

    /// <summary>
    /// Indicates that the pattern could not be parsed or recognized.
    /// </summary>
    Unknown
}
