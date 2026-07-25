namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// Provides a base class and common implementation structure for all specialized path matchers.
/// </summary>
/// <remarks>
/// All derived matchers must utilize the logic within <see cref="MatchCore"/> to perform the actual evaluation.
/// This abstract class enforces the standard <see cref="IPathMatcher"/> contract while providing boilerplate
/// methods for single and set pattern matching.
/// </remarks>
internal abstract class PathMatcher : IPathMatcher {
    /// <inheritdoc/>
    public abstract bool Supports(PatternKind patternKind);

    /// <inheritdoc cref="IPathMatcher.Match(ICompiledPatternSet, PathMatchContext)"/>
    public MatchOutcome Match(ICompiledPatternSet patterns, PathMatchContext context) => MatchCore(patterns, context).ToOutcome();

    /// <inheritdoc cref="IPathMatcher.Match(ICompiledPattern, PathMatchContext)"/>
    public virtual MatchOutcome Match(ICompiledPattern pattern, PathMatchContext context) => Match(new CompiledPatternSet([pattern]), context);

    /// <summary>
    /// Performs the core, concrete matching logic against the provided patterns and context.
    /// Derived classes must implement this to define their specific matching rules.
    /// </summary>
    /// <param name="patterns">The set of compiled patterns to test against.</param>
    /// <param name="context">The normalized file system context.</param>
    /// <returns>A <see cref="MatchResult"/> representing the evaluation outcome.</returns>
    protected abstract MatchResult MatchCore(ICompiledPatternSet patterns, PathMatchContext context);
}
