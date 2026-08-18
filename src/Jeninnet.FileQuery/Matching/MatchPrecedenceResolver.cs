//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Resolves the final <see cref="MatchOutcome"/> for a path by applying
/// precedence rules over compiled pattern instructions.
/// </summary>
/// <remarks>
/// Policy:
/// <list type="bullet">
/// <item><description>Instructions are evaluated in order: GitIgnore → Glob → Regex.</description></item>
/// <item><description>The first sub-set that returns <see cref="MatchOutcome.Include"/> wins.</description></item>
/// <item><description>If nothing matches, the outcome defaults to <see cref="MatchOutcome.Include"/>.</description></item>
/// </list>
/// </remarks>
internal sealed class MatchPrecedenceResolver {
    public static readonly GitIgnoreInstructionMatcher GitIgnoreMatcher = new();
    public static readonly GlobInstructionMatcher GlobMatcher = new();
    public static readonly RegexInstructionMatcher RegexMatcher = new();

    public static readonly MatchPrecedenceResolver Default = new([
        GitIgnoreMatcher,
        GlobMatcher,
        RegexMatcher
    ]);

    private readonly IReadOnlyList<IPathMatcher> _matchers;

    internal MatchPrecedenceResolver(IReadOnlyList<IPathMatcher> matchers)
        => _matchers = matchers;

    /// <summary>
    /// Determines whether the given path is included or excluded by evaluating the provided compiled pattern set.
    /// </summary>
    /// <remarks>
    /// If the path is empty, returns Exclude. If the pattern set is empty, returns Include.
    /// Subsets/ are evaluated in order: GitIgnoreSubSet, then GlobSubSet, then RegexSubSet.
    /// If a GitIgnore or Glob match yields Include, evaluation stops and Include is returned;
    /// otherwise the result from the Regex subset is returned if present.
    /// </remarks>
    /// <param name="instructions">The compiled pattern set containing GitIgnoreSubSet, GlobSubSet, and RegexSubSet to evaluate.</param>
    /// <param name="context">The PathMatchContext containing the path and matching state used for evaluation.</param>
    /// <returns>A MatchOutcome indicating whether the path is included or excluded after evaluation.</returns>
    public static MatchOutcome Resolve(ICompiledPatternSet instructions, PathMatchContext context) {
        if(context.Path.IsEmpty) {
            return MatchOutcome.Exclude;
        }

        if(instructions.Count == 0) {
            return MatchOutcome.Include;
        }

        var result = MatchOutcome.Include;

        if(instructions.GitIgnoreSubSet is not null) {
            result = GitIgnoreMatcher.Match(instructions.GitIgnoreSubSet, context);
            if(result is MatchOutcome.Include) {
                return result;
            }
        }

        if(instructions.GlobSubSet is not null) {
            result = GlobMatcher.Match(instructions.GlobSubSet, context);
            if(result is MatchOutcome.Include) {
                return result;
            }
        }

        if(instructions.RegexSubSet is not null) {
            result = RegexMatcher.Match(instructions.RegexSubSet, context);
        }

        return result;
    }

    /// <summary>
    /// Resolves a single compiled pattern. Zero-allocation: index loop instead of LINQ.
    /// </summary>
    /// <param name="instruction">The compiled pattern instruction to resolve.</param>
    /// <param name="context">The match context containing path and matching state.</param>
    public MatchOutcome Resolve(ICompiledPattern instruction, PathMatchContext context) {
        for(var i = 0; i < _matchers.Count; i++) {
            if(_matchers[i].Supports(instruction.PatternKind)) {
                return _matchers[i].Match(instruction, context);
            }
        }

        return MatchOutcome.Include;
    }
}
