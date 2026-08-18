//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Analysis;

/// <inheritdoc/>
internal sealed class PatternAnalyzer : IPatternAnalyzer {
    /// <inheritdoc/>
    public PatternAnalysisResult Analyze(ReadOnlySpan<char> pattern) {
        if(pattern.IsEmpty) {
            return PatternAnalysisResult.Empty();
        }

        // Fast-path: regex prefix
        if(pattern.StartsWith("r:".AsSpan(), StringComparison.Ordinal)) {
            return PatternAnalysisResult.Regex();
        }

        var state = new AnalysisState();

        // Negation (only first char)
        var index = HandleNegation(pattern, state);

        // Main scan
        ScanPattern(pattern, state, index);

        // Trailing slash → GitIgnore directory rule
        if(pattern[^1] == '/') {
            state.HasGitIgnoreSyntax = true;
        }

        return state.ToResult();
    }

    /// <summary>
    /// Detects and handles a leading negation marker (<c>!</c>).
    /// </summary>
    /// <remarks>
    /// GitIgnore negation is only valid as the very first character.
    /// </remarks>
    /// <param name="pattern">The pattern string being analyzed.</param>
    /// <param name="state">Mutable analysis state.</param>
    /// <returns>The index at which normal scanning should begin.</returns>
    private static int HandleNegation(ReadOnlySpan<char> pattern, AnalysisState state) {
        if(pattern[0] == '!') {
            state.IsNegated = true;
            state.HasGitIgnoreSyntax = true;
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Scans the pattern for structural features such as wildcards,
    /// escapes, brackets, and GitIgnore‑specific syntax.
    /// </summary>
    /// <param name="pattern">The pattern being analyzed.</param>
    /// <param name="state">Mutable analysis state.</param>
    /// <param name="start">Starting index after prefix handling.</param>
    private static void ScanPattern(ReadOnlySpan<char> pattern, AnalysisState state, int start) {
        var skipNext = false;

        for(var i = start; i < pattern.Length; i++) {
            if(skipNext) {
                skipNext = false;
                continue;
            }

            var c = pattern[i];

            if(HandleEscape(pattern, state, i, out skipNext)) {
                continue;
            }

            if(HandleSeparator(c, state)) {
                continue;
            }

            if(HandleWildcard(pattern, state, i, out skipNext)) {
                continue;
            }

            if(HandleBracket(c, state)) {
                continue;
            }

            _ = HandleComment(c, i, state);
        }
    }

    /// <summary>
    /// Handles backslash escape sequences.
    /// </summary>
    /// <remarks>
    /// Only a leading escape (position 0) contributes to GitIgnore semantics.
    /// Other backslashes are simply recorded.
    /// </remarks>
    /// <param name="pattern">The pattern string being analyzed.</param>
    /// <param name="state">Mutable analysis state.</param>
    /// <param name="i">The current index in the pattern.</param>
    /// <param name="skipNext">Output indicating whether the next character should be skipped.</param>
    private static bool HandleEscape(ReadOnlySpan<char> pattern, AnalysisState state, int i, out bool skipNext) {
        skipNext = false;

        if(pattern[i] != '\\') {
            return false;
        }

        if(i + 1 < pattern.Length) {
            var next = pattern[i + 1];

            if(i == 0 && IsEscapable(next)) {
                state.HasEscapedCharacters = true;
                state.HasGitIgnoreSyntax = true;
                skipNext = true;
                return true;
            }
        }

        state.HasBackslash = true;
        return true;
    }

    /// <summary>
    /// Handles directory separators (<c>/</c>) and increments segment count.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="state">Mutable analysis state.</param>
    private static bool HandleSeparator(char c, AnalysisState state) {
        if(c != '/') {
            return false;
        }

        state.HasForwardSlash = true;
        state.SegmentCount++;
        return true;
    }

    /// <summary>
    /// Handles wildcard characters (<c>*</c>, <c>**</c>, <c>?</c>).
    /// </summary>
    /// <remarks>
    /// Recursive wildcards (<c>**</c>) imply GitIgnore semantics.
    /// </remarks>
    /// <param name="pattern">The pattern span to analyze.</param>
    /// <param name="state">The current analysis state.</param>
    /// <param name="i">The index of the wildcard character.</param>
    /// <param name="skipNext">Whether to skip the next character in analysis.</param>
    private static bool HandleWildcard(ReadOnlySpan<char> pattern, AnalysisState state, int i, out bool skipNext) {
        skipNext = false;

        if(pattern[i] == '*') {
            state.HasWildcard = true;

            if(i + 1 < pattern.Length && pattern[i + 1] == '*') {
                state.HasRecursiveWildcard = true;
                state.HasGitIgnoreSyntax = true;
                skipNext = true;
            }

            return true;
        }

        if(pattern[i] == '?') {
            state.HasWildcard = true;
            state.HasSingleCharWildcard = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles bracket characters (<c>[</c> and <c>]</c>).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="state">Mutable analysis state.</param>
    private static bool HandleBracket(char c, AnalysisState state) {
        if(c is '[' or ']') {
            state.HasBracket = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles GitIgnore comment syntax (<c>#</c> at the start of the pattern).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="index">The current index.</param>
    /// <param name="state">Mutable analysis state.</param>
    private static bool HandleComment(char c, int index, AnalysisState state) {
        if(c == '#' && index == 0) {
            state.HasGitIgnoreSyntax = true;
            return true;
        }

        return false;
    }

    private static bool IsEscapable(char c) => c is '*' or '?' or '!' or '#' or '[' or ']' or '\\';

    // -------------------------------------------------
    // Internal state container
    // -------------------------------------------------
    /// <summary>
    /// Mutable container for pattern analysis flags.
    /// </summary>
    /// <remarks>
    /// This is used internally during scanning and later converted
    /// into an immutable <see cref="PatternAnalysisResult"/>.
    /// </remarks>
    private sealed class AnalysisState {
        /// <summary>True if the pattern begins with a negation marker (<c>!</c>).</summary>
        public bool IsNegated;

        /// <summary>True if any backslash was encountered.</summary>
        public bool HasBackslash;

        /// <summary>True if any forward slash was encountered.</summary>
        public bool HasForwardSlash;

        /// <summary>True if any wildcard (<c>*</c> or <c>?</c>) was encountered.</summary>
        public bool HasWildcard;

        /// <summary>True if a single‑character wildcard (<c>?</c>) was encountered.</summary>
        public bool HasSingleCharWildcard;

        /// <summary>True if any bracket (<c>[</c> or <c>]</c>) was encountered.</summary>
        public bool HasBracket;

        /// <summary>True if a recursive wildcard (<c>**</c>) was encountered.</summary>
        public bool HasRecursiveWildcard;

        /// <summary>True if a leading escape sequence was used.</summary>
        public bool HasEscapedCharacters;

        /// <summary>True if any GitIgnore‑specific syntax was detected.</summary>
        public bool HasGitIgnoreSyntax;

        /// <summary>Number of path segments (counted via <c>/</c>).</summary>
        public int SegmentCount = 1;

        /// <summary>
        /// Converts the accumulated state into a <see cref="PatternAnalysisResult"/>.
        /// </summary>
        public PatternAnalysisResult ToResult() =>
            new(
                IsEmpty: false,
                IsRegex: false,
                IsNegated: IsNegated,
                HasBackslash: HasBackslash,
                HasForwardSlash: HasForwardSlash,
                HasWildcard: HasWildcard,
                HasSingleCharWildcard: HasSingleCharWildcard,
                HasBracket: HasBracket,
                HasRecursiveWildcard: HasRecursiveWildcard,
                HasEscapedCharacters: HasEscapedCharacters,
                HasGitIgnoreSyntax: HasGitIgnoreSyntax,
                SegmentCount: SegmentCount
            );
    }
}
