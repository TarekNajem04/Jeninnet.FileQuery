//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Defines an interface for any component that can determine if a path string matches
/// a defined set of patterns.
/// </summary>
/// <remarks>
/// Implementations must be stateless, side-effect free, and rely only on the provided
/// <see cref="ICompiledPatternSet"/> and <see cref="PathMatchContext"/>. This contract
/// is key to the overall system determinism.
/// </remarks>
internal interface IPathMatcher {
    /// <summary>
    /// Checks if this matcher implementation is designed to handle patterns of the specified <see cref="PatternKind"/>.
    /// </summary>
    /// <param name="patternKind">The pattern type to check.</param>
    /// <returns><c>true</c> if this matcher supports the given pattern type; otherwise, <c>false</c>.</returns>
    bool Supports(PatternKind patternKind);

    /// <summary>
    /// Checks if the specified <paramref name="context"/> path is acceptable across all patterns provided
    /// in the set. The rule applied is: the first pattern that successfully matches determines
    /// the outcome.
    /// </summary>
    /// <param name="patterns">The compiled set of patterns to evaluate.</param>
    /// <param name="context">The execution context, containing the path and metadata.</param>
    /// <returns>A <see cref="MatchOutcome"/> indicating whether to include or exclude the path.</returns>
    MatchOutcome Match(ICompiledPatternSet patterns, PathMatchContext context);

    /// <summary>
    /// Checks if the specified <paramref name="context"/> path is acceptable according to a single,
    /// focused pattern.
    /// </summary>
    /// <param name="pattern">The single compiled <see cref="ICompiledPattern"/> to evaluate.</param>
    /// <param name="context">The execution context, containing the path and metadata.</param>
    /// <returns>A <see cref="MatchOutcome"/> indicating whether to include or exclude the path.</returns>
    MatchOutcome Match(ICompiledPattern pattern, PathMatchContext context);
}
