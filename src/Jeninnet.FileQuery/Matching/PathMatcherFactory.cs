namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Centralized factory responsible for creating and providing
/// <see cref="IPathMatcher"/> instances appropriate for a given query configuration.
/// </summary>
/// <remarks>
/// <para>
/// This factory is the <strong>exclusive construction authority</strong> for all
/// <see cref="IPathMatcher"/> implementations. Matchers intentionally do not expose
/// public constructors. This factory enforces architectural invariants by ensuring that
/// matchers are created only in valid configurations.
/// </para>
/// <para>
/// Architecture tests rely on this contract. Do not bypass or duplicate matcher
/// construction elsewhere in the codebase.
/// </para>
/// </remarks>
internal static class PathMatcherFactory {
    /// <summary>
    /// A shared <see cref="HybridPathMatcher"/> instance, reused across queries
    /// since matchers carry no per-query mutable state.
    /// </summary>
    private static readonly HybridPathMatcher _hybridPathMatcher = new();

    /// <summary>
    /// A shared registry of single-dialect matchers, keyed by
    /// <see cref="PatternMatchingMode"/>.
    /// </summary>
    private static readonly Dictionary<PatternMatchingMode, IPathMatcher> _matchers = new() {
        [PatternMatchingMode.GitIgnore] = new GitIgnoreInstructionMatcher(),
        [PatternMatchingMode.Glob] = new GlobInstructionMatcher(),
        [PatternMatchingMode.Regex] = new RegexInstructionMatcher()
    };

    /// <summary>
    /// Creates an appropriate <see cref="IPathMatcher"/> based on the provided
    /// query options.
    /// </summary>
    /// <param name="options">
    /// The query options containing pattern configuration and matching behavior.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     <see cref="NullMatcher.Instance"/> when no patterns are configured —
    ///     the null-object pattern eliminates the need for null checks in the traversal loop.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     A shared <see cref="HybridPathMatcher"/> when
    ///     <see cref="PatternInterpretationMode.Hybrid"/> is selected.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     A dialect-specific matcher (GitIgnore, Glob, or Regex) when
    ///     <see cref="PatternInterpretationMode.Specific"/> is selected.
    ///     </description>
    ///   </item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public static IPathMatcher Create(FileQueryOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        // When no patterns are configured, use the null-object matcher.
        // This avoids a conditional in the hot traversal loop.
        // Note: PatternInput.TypedPatterns is never null (guaranteed by the PatternInput constructor).
        if(
            options.PatternInput.Patterns.Count == 0 &&
            options.PatternInput.TypedPatterns.Count == 0
        ) {
            return NullMatcher.Instance;
        }

        return options.PatternInput.InterpretationMode is PatternInterpretationMode.Hybrid
            ? _hybridPathMatcher
            : _matchers[options.PatternMatchingMode];
    }
}
