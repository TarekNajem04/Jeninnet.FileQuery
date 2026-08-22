//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Resolves the trailing literal suffix of a pattern's last segment.
/// </summary>
/// <remarks>
/// <para>
/// Any path segment that matches a tokenized pattern segment must end with the
/// segment's trailing run of literal/escaped characters (the matcher consumes
/// those characters positionally). GitIgnore matchers therefore use the resolved
/// suffix as a zero-allocation rejection fast path: when the candidate path's
/// last segment does not end with the suffix, the pattern cannot match and the
/// expensive recursive wildcard matcher is never entered.
/// </para>
/// <para>
/// The suffix is a necessary condition only — callers must still run the full
/// matcher when the check passes. An empty result means no fixed suffix exists
/// (wildcard, single-character, or character-class token terminates the segment)
/// and the fast path must be skipped.
/// </para>
/// </remarks>
internal static class LiteralSuffixResolver {
    /// <summary>
    /// Returns the trailing literal run of the last pattern segment, or an empty
    /// string when no fixed suffix exists.
    /// </summary>
    /// <param name="segments">The tokenized pattern segments.</param>
    public static string Resolve(IReadOnlyList<IReadOnlyList<IPatternToken>> segments) {
        if(segments.Count == 0) {
            return string.Empty;
        }

        var lastSegment = segments[segments.Count - 1];
        var runStart = lastSegment.Count;

        for(var i = lastSegment.Count - 1; i >= 0; i--) {
            if(lastSegment[i] is LiteralToken or EscapeToken) {
                runStart = i;
            } else {
                break;
            }
        }

        if(runStart == lastSegment.Count) {
            return string.Empty;
        }

        var builder = new StringBuilder();
        for(var i = runStart; i < lastSegment.Count; i++) {
            if(lastSegment[i] is LiteralToken literal) {
                builder.Append(literal.Text);
            } else if(lastSegment[i] is EscapeToken escape) {
                builder.Append(escape.Escaped);
            }
        }

        // A suffix containing '/' cannot be checked against the last path segment
        // alone (escaped separators never appear in valid patterns, but guard anyway).
        var suffix = builder.ToString();
        return suffix.Length > 0 && suffix.IndexOf('/') < 0 ? suffix : string.Empty;
    }
}
