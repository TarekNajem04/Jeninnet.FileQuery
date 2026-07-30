namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Specifies how untyped patterns should be classified and interpreted by the engine.
/// </summary>
public enum PatternInterpretationMode {
    /// <summary>
    /// Use the unified hybrid engine that supports
    /// GitIgnore + Glob + recursive semantics.
    /// </summary>
    Hybrid = 0,

    /// <summary>
    /// All patterns follow the explicit <see cref="PatternMatchingMode"/> is used withe <see cref="FileQueryOptions.PatternMatchingMode"/>.
    /// </summary>
    Specific = 1,
}
