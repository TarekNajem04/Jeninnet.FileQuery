//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// A no-op matcher implementing the Null Object pattern.
/// </summary>
/// <remarks>
/// Instead of using <see langword="null"/> to represent “no matcher”, this class provides
/// a concrete implementation that always returns an inclusive, non-matching result.
/// This simplifies client code by removing the need for null checks and special cases.
/// </remarks>
internal sealed class NullMatcher : PathMatcher {
    /// <summary>
    /// A shared <see cref="NullMatcher"/> instance.
    /// </summary>
    private static readonly Lazy<NullMatcher> _instance = new(static () => new NullMatcher()); // Thread-safe by default

    /// <summary>
    /// Gets the shared singleton instance of <see cref="NullMatcher"/>.
    /// </summary>
    public static NullMatcher Instance => _instance.Value;

    private NullMatcher() { }

    /// <inheritdoc/>
    public override bool Supports(PatternKind patternKind) => true;

    /// <summary>
    /// Returns a no-op match result.
    /// </summary>
    /// <param name="patterns">Ignored.</param>
    /// <param name="context">Ignored.</param>
    /// <returns>
    /// Always returns a successful result, effectively treating all paths as included.
    /// </returns>
    /// <remarks>
    /// This behavior is intentional: when no patterns are configured, the engine
    /// should not filter out any paths. Centralizing this logic here keeps the
    /// traversal code simple and free of special cases.
    /// </remarks>
    protected override MatchResult MatchCore(ICompiledPatternSet patterns, PathMatchContext context) => MatchResult.Success();
}
