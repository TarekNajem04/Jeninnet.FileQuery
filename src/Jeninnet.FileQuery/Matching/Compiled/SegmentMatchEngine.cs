namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// Allocation-free engine for segment-based path matching.
/// </summary>
/// <remarks>
/// This engine is policy-free and shared by Glob and GitIgnore matchers.
/// It relies exclusively on value-type enumerator snapshots for backtracking.
/// </remarks>
internal abstract class SegmentMatchEngine : PathMatcher {
    protected static bool MatchExact(
        IReadOnlyList<IReadOnlyList<IPatternToken>> patternSegments,
        int patternIndex,
        StringComparison comparison,
        ref PathSegmentEnumerator pathSegmentEnumerator
    ) {
        while(true) {
            if(patternIndex == patternSegments.Count) {
                return !pathSegmentEnumerator.MoveNext();
            }

            var tokens = patternSegments[patternIndex];

            if(IsDoubleStar(tokens)) {
                return MatchAfterDoubleStar(
                    patternSegments,
                    patternIndex,
                    comparison,
                    ref pathSegmentEnumerator
                );
            }

            if(!pathSegmentEnumerator.MoveNext()) {
                return false;
            }

            if(!SegmentInstructionMatcher.MatchSegment(pathSegmentEnumerator.Current, tokens, comparison)) {
                return false;
            }

            patternIndex++;
        }
    }

    protected static bool MatchUnanchored(
        IReadOnlyList<IReadOnlyList<IPatternToken>> patternSegments,
        StringComparison comparison,
        PathSegmentEnumerator pathSegmentEnumerator
    ) {
        do {
            var snapshot = pathSegmentEnumerator;
            if(MatchExact(patternSegments, 0, comparison, ref snapshot)) {
                return true;
            }
        }
        while(pathSegmentEnumerator.MoveNext());

        return false;
    }

    private static bool MatchAfterDoubleStar(
        IReadOnlyList<IReadOnlyList<IPatternToken>> patternSegments,
        int patternIndex,
        StringComparison comparison,
        ref PathSegmentEnumerator pathSegmentEnumerator
    ) {
        // Trailing "**" matches everything
        if(patternIndex == patternSegments.Count - 1) {
            return true;
        }

        var snapshot = pathSegmentEnumerator;

        do {
            var attempt = snapshot;
            if(MatchExact(
                patternSegments,
                patternIndex + 1,
                comparison,
                ref attempt)) {
                return true;
            }
        }
        while(snapshot.MoveNext());

        return false;
    }

    /// <summary>
    /// Determines whether the given pattern token sequence represents a recursive wildcard segment (<c>**</c>).
    /// </summary>
    /// <param name="tokens">The tokens for a single pattern segment.</param>
    /// <returns>
    /// <see langword="true"/> if the segment consists of a single <see cref="RecursiveWildcardToken"/> token;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This helper centralizes the definition of a “double star” segment, ensuring consistent semantics
    /// across all matchers that support recursive directory matching.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static bool IsDoubleStar(IReadOnlyList<IPatternToken> tokens) =>
        tokens.Count == 1 &&
        tokens[0] is RecursiveWildcardToken;
}
