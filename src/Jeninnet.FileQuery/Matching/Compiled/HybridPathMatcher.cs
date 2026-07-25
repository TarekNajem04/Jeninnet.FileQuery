namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// A hybrid matcher that supports <see cref="PatternKind.GitIgnore"/>,
/// <see cref="PatternKind.Glob"/>, and <see cref="PatternKind.Regex"/> pattern semantics.
/// </summary>
/// <remarks>
/// <para>
/// This matcher orchestrates three internal engines:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="GitIgnoreInstructionMatcher"/> – directory-aware, supports <c>**</c>, anchored patterns, and negation.</description></item>
/// <item><description><see cref="GlobInstructionMatcher"/> – classic globbing applied to the full normalized path.</description></item>
/// <item><description><see cref="RegexInstructionMatcher"/> – regular expression matching applied to the full normalized path.</description></item>
/// </list>
/// <para>
/// Matching Rules:
/// </para>
/// <list type="number">
/// <item>Patterns are evaluated in order within each matcher.</item>
/// <item>Later patterns override earlier ones, matching .gitignore semantics where applicable.</item>
/// <item>Negated patterns (starting with <c>!</c>) reverse inclusion state in GitIgnore mode.</item>
/// <item>Directory-only patterns match only directories.</item>
/// </list>
/// <para>
/// This class is pure and thread-safe after construction.
/// </para>
/// </remarks>
internal sealed class HybridPathMatcher : IPathMatcher {
    private readonly MatchPrecedenceResolver _precedenceResolver = MatchPrecedenceResolver.Default;

    internal HybridPathMatcher() { }

    /// <inheritdoc/>
    public bool Supports(PatternKind patternKind) => patternKind is PatternKind.GitIgnore or PatternKind.Glob or PatternKind.Regex;

    /// <inheritdoc/>
    public MatchOutcome Match(ICompiledPatternSet patterns, PathMatchContext context) => MatchPrecedenceResolver.Resolve(patterns, context);

    /// <inheritdoc/>
    public MatchOutcome Match(ICompiledPattern pattern, PathMatchContext context) => _precedenceResolver.Resolve(pattern, context);
}
