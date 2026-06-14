namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Shared, allocation-free context flowing through the pattern compilation pipeline.
/// </summary>
/// <remarks>
/// This context is mutable by design but scoped to a single compilation.
/// It must never escape the pipeline.
/// </remarks>
internal sealed class PatternCompilationContext
{
    internal PatternCompilationContext(ClassifiedPattern pattern) => Pattern = pattern;

    public ClassifiedPattern Pattern { get; }

    /// <summary> Tokenized segments (after lexical phase). </summary>
    public List<List<IPatternToken>>? Tokens { get; set; }

    /// <summary> Canonicalized representation (optional). </summary>
    public CanonicalPattern? Canonical { get; set; }

    /// <summary> Compiler-specific scratchpad. </summary>
    public PatternContext State { get; set; }
    public ICompiledPattern? Compiled { get; set; }
}
