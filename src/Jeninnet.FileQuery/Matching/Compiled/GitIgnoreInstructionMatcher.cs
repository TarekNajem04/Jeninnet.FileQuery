//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// GitIgnore-compatible path matcher.
/// </summary>
/// <remarks>
/// <para>
/// Implements GitIgnore-style semantics using a segment-driven algorithm:
/// </para>
/// <list type="bullet">
///   <item>Leading <c>/</c> anchors a pattern to the logical root.</item>
///   <item>Trailing <c>/</c> marks directory-only patterns.</item>
///   <item><c>!</c> negation follows last-rule-wins semantics.</item>
///   <item><c>*</c> and <c>?</c> operate within a single segment;
///         <c>**</c> matches whole segments across directories.</item>
/// </list>
/// <para>
/// <strong>Performance contract — zero hot-path allocations:</strong>
/// Matching must not allocate. All traversal is performed using
/// <see cref="PathView"/> and <see cref="PathSegmentEnumerator"/> (both
/// <c>ref struct</c>). Pattern iteration uses index-based <c>for</c> loops
/// rather than <c>foreach</c> over the <see cref="ICompiledPatternSet"/>
/// interface, which would box a heap-allocated <see cref="IEnumerator{T}"/>
/// on every call (~40 B per enumerator, confirmed by benchmarks).
/// </para>
/// </remarks>
internal sealed class GitIgnoreInstructionMatcher : SegmentMatchEngine {
    /// <summary>
    /// Initializes a new instance of the <see cref="GitIgnoreInstructionMatcher"/>.
    /// </summary>
    /// <remarks>
    /// <c>internal</c> by design. Matchers must only be created by
    /// <see cref="PathMatcherFactory"/>.
    /// </remarks>
    internal GitIgnoreInstructionMatcher() { }

    /// <inheritdoc/>
    public override bool Supports(PatternKind patternKind) => patternKind is PatternKind.GitIgnore;

    internal bool AppliesToPattern(ICompiledPattern pattern, PathMatchContext context) {
        if(context.Path.IsEmpty) {
            return false;
        }

        var pathView = new PathView(context.Path, context.PathKind == PathKind.Directory);
        var comparison = context.CaseSensitivity.GetStringComparison();

        if(!MatchPathAgainstCompiledPattern(pattern, pathView, comparison)) {
            return false;
        }

        return !pattern.DirectoryOnly ||
            pathView.IsDirectory ||
            pathView.SegmentCount != pattern.Segments.Count - 1;
    }

    /// <inheritdoc/>
    protected override MatchResult MatchCore(
        ICompiledPatternSet patterns,
        PathMatchContext context
    ) {
        if(context.Path.IsEmpty) {
            return MatchResult.Fail();
        }

        if(patterns.Count == 0) {
            return MatchResult.Success();
        }

        var pathView = new PathView(context.Path, context.PathKind == PathKind.Directory);
        var comparison = context.CaseSensitivity.GetStringComparison();
        var result = MatchResult.Included();

        for(var i = 0; i < patterns.Count; i++) {
            var pattern = patterns[i];

            // If the pattern does not match, skip WITHOUT resetting IsMatched.
            // Resetting it (the original bug) turned a prior Exclude into NoMatch
            // when a non-matching pattern followed, allowing traversal into pruned
            // directories like .xx/ after .*/  was followed by *.xxx.
            if(!MatchPathAgainstCompiledPattern(pattern, pathView, comparison)) {
                continue;
            }

            // Dir-only depth exception for files: "xxx/" must not affect a *file*
            // named "xxx" at the same nesting level — it targets the directory only.
            if(pattern.DirectoryOnly &&
                !pathView.IsDirectory &&
                pathView.SegmentCount == pattern.Segments.Count - 1) {
                continue;
            }

            // Pattern genuinely applies — record the match and update inclusion state.
            result.Match();
            if(pattern.IsNegated) {
                result.Include();
            } else {
                result.Exclude();
            }
        }

        _ = TryApplyDirectoryInclusionOverride(ref result, patterns, pathView, comparison);
        return result;
    }

    /// <summary>
    /// Attempts to override an excluded file result by checking whether any
    /// directory-only inclusion rule applies to the file's parent directory.
    /// </summary>
    /// <param name="result">The match result to be updated.</param>
    /// <param name="patterns">The compiled patterns to evaluate.</param>
    /// <param name="pathView">The path view being evaluated.</param>
    /// <param name="comparison">The string comparison to use.</param>
    /// <returns>
    /// <see langword="true"/> if the result was overridden to
    /// <see cref="MatchResult.Include()"/>; otherwise <see langword="false"/>.
    /// </returns>
    private bool TryApplyDirectoryInclusionOverride(
        ref MatchResult result,
        IReadOnlyList<ICompiledPattern> patterns,
        PathView pathView,
        StringComparison comparison
    ) {
        if(pathView.IsDirectory || result.IsIncluded) {
            return false;
        }

        if(pathView.SegmentCount <= 1) {
            return false;
        }

        // INDEX-BASED LOOP — same rationale as in MatchCore.
        // When the file is excluded (result.IsIncluded = false),
        // this loop runs searching for a negated directory-only rule.
        // Avoiding a second enumerator here eliminates another 40 B allocation,
        // confirmed by the HybridMatcher benchmark showing 120 B (80 B from the
        // two GitIgnore loops + 40 B from the Regex loop).
        for(var i = 0; i < patterns.Count; i++) {
            var pattern = patterns[i];

            // Only directory-only inclusion rules (!dir/) are relevant.
            if(!pattern.IsNegated || !pattern.DirectoryOnly) {
                continue;
            }

            if(MatchParentDirectory(pattern, pathView, comparison)) {
                result.Include();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Matches a pattern against the parent directory of the current path.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="pathView">The view of the path segments.</param>
    /// <param name="comparison">The string comparison rules.</param>
    /// <returns>True if the pattern matches; otherwise, false.</returns>
    private bool MatchParentDirectory(
        ICompiledPattern pattern,
        PathView pathView,
        StringComparison comparison
    ) {
        var enumerator = pathView.EnumerateSegments();
        var remaining = pathView.SegmentCount - 1;

        return MatchRecursiveSegments(
            pattern,
            patternIndex: 0,
            comparison,
            enumerator,
            remaining,
            isDirectory: true
        );
    }

    /// <summary>
    /// Matches a path against a compiled GitIgnore pattern.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="pathView">The view of the path segments.</param>
    /// <param name="comparison">The string comparison rules.</param>
    /// <returns>True if the path matches the pattern; otherwise, false.</returns>
    private bool MatchPathAgainstCompiledPattern(
        ICompiledPattern pattern,
        PathView pathView,
        StringComparison comparison
    ) {
        if(pathView.SegmentCount == 0) {
            return false;
        }

        if(pattern.DirectoryOnly &&
            !pathView.IsDirectory &&
            !pattern.AnchoredToRoot) {
            return false;
        }

        if(RejectsByLiteralSuffix(pattern, pathView, comparison)) {
            return false;
        }

        var enumerator = pathView.EnumerateSegments();

        var hasLeadingDoubleStar =
            pattern.Segments.Count > 0 &&
            IsDoubleStar(pattern.Segments[0]);

        if(pattern.AnchoredToRoot || hasLeadingDoubleStar) {
            return MatchRecursiveSegments(
                pattern,
                patternIndex: 0,
                comparison,
                enumerator,
                pathView.SegmentCount,
                pathView.IsDirectory
            );
        }

        return MatchUnanchored(
            pattern,
            pathView,
            comparison,
            enumerator
        );
    }

    /// <summary>
    /// Suffix rejection fast path. When the pattern's last segment carries a
    /// fixed literal run, any path the pattern can possibly match must end with
    /// it: non-directory-only patterns always map their last segment onto the
    /// path's last segment (<see cref="MatchRecursiveSegments"/> only reports a
    /// match once the path is fully consumed), so a missing suffix proves no
    /// match without entering the recursive matcher. Directory-only patterns are
    /// exempt: they may legitimately match with leftover path segments
    /// (e.g. <c>/a/</c> matching the file <c>a/b</c>), so their suffix is never
    /// resolved (empty).
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="pathView">The view of the path segments.</param>
    /// <param name="comparison">The string comparison rules.</param>
    private static bool RejectsByLiteralSuffix(
        ICompiledPattern pattern,
        PathView pathView,
        StringComparison comparison
    ) {
        if(pattern.DirectoryOnly || pattern.LiteralSuffix.Length == 0) {
            return false;
        }

        var path = pathView.Path;

        // Directory paths carry a trailing '/' (see PathUtilities.BuildRelativePath).
        if(path.Length > 0 && path[^1] == '/') {
            path = path[..^1];
        }

        return !path.EndsWith(pattern.LiteralSuffix, comparison);
    }

    /// <summary>
    /// Unanchored match: slides the pattern across path segments until a match
    /// is found or the path is exhausted.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="pathView">The view of the path segments.</param>
    /// <param name="comparison">The string comparison rules.</param>
    /// <param name="enumerator">The enumerator for path segments.</param>
    private bool MatchUnanchored(
        ICompiledPattern pattern,
        PathView pathView,
        StringComparison comparison,
        PathSegmentEnumerator enumerator
    ) {
        var skip = 0;
        while(true) {
            var fork = enumerator; // value-type copy for speculation

            if(MatchRecursiveSegments(
                    pattern,
                    patternIndex: 0,
                    comparison,
                    fork,
                    pathView.SegmentCount - skip,
                    pathView.IsDirectory)) {
                return true;
            }

            if(!enumerator.MoveNext()) {
                break;
            }

            skip++;
        }

        return false;
    }

    /// <summary>
    /// Core recursive segment matcher implementing GitIgnore semantics.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="patternIndex">The index of the pattern segment being matched.</param>
    /// <param name="comparison">The string comparison rules.</param>
    /// <param name="path">The enumerator for path segments.</param>
    /// <param name="remainingSegments">The number of segments left in the path.</param>
    /// <param name="isDirectory">Whether the path represents a directory.</param>
    private bool MatchRecursiveSegments(
        ICompiledPattern pattern,
        int patternIndex,
        StringComparison comparison,
        PathSegmentEnumerator path,
        int remainingSegments,
        bool isDirectory
    ) {
        if(patternIndex == pattern.Segments.Count) {
            return remainingSegments == 0
                ? (!pattern.DirectoryOnly || isDirectory)
                : pattern.DirectoryOnly;
        }

        if(remainingSegments == 0) {
            return CanRemainingPatternMatchEmpty(pattern.Segments, patternIndex);
        }

        var currentPattern = pattern.Segments[patternIndex];

        if(IsDoubleStar(currentPattern)) {
            return MatchRecursiveWildcard(
                pattern,
                patternIndex,
                comparison,
                path,
                remainingSegments,
                isDirectory
            );
        }

        if(!path.MoveNext()) {
            return false;
        }

        if(!SegmentInstructionMatcher.MatchSegment(
                path.Current,
                currentPattern,
                comparison)) {
            return false;
        }

        return MatchRecursiveSegments(
            pattern,
            patternIndex + 1,
            comparison,
            path,
            remainingSegments - 1,
            isDirectory
        );
    }

    /// <summary>
    /// Handles recursive wildcard (<c>**</c>) matching with backtracking.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="patternIndex">The index of the pattern segment being matched.</param>
    /// <param name="comparison">The string comparison rules.</param>
    /// <param name="path">The enumerator for path segments.</param>
    /// <param name="remainingSegments">The number of segments left in the path.</param>
    /// <param name="isDirectory">Whether the path represents a directory.</param>
    private bool MatchRecursiveWildcard(
        ICompiledPattern pattern,
        int patternIndex,
        StringComparison comparison,
        PathSegmentEnumerator path,
        int remainingSegments,
        bool isDirectory
    ) {
        // Match zero segments.
        if(MatchRecursiveSegments(
                pattern,
                patternIndex + 1,
                comparison,
                path,
                remainingSegments,
                isDirectory)) {
            return true;
        }

        // Match one or more segments.
        while(remainingSegments > 0) {
            if(!path.MoveNext()) {
                break;
            }

            remainingSegments--;

            var fork = path; // struct copy for backtracking

            if(MatchRecursiveSegments(
                    pattern,
                    patternIndex + 1,
                    comparison,
                    fork,
                    remainingSegments,
                    isDirectory)
            ) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether all remaining pattern segments can match
    /// an empty (zero-segment) path.
    /// </summary>
    /// <param name="patternSegments">The collection of segments.</param>
    /// <param name="startIndex">The starting index of the remaining segments.</param>
    private static bool CanRemainingPatternMatchEmpty(
        IReadOnlyList<IReadOnlyList<IPatternToken>> patternSegments,
        int startIndex
    ) {
        for(var i = startIndex; i < patternSegments.Count; i++) {
            if(!IsDoubleStar(patternSegments[i])) {
                return false;
            }
        }

        return true;
    }
}
