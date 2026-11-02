//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Validation;

/// <summary>
/// Locates structural error positions in raw pattern text.
/// </summary>
/// <remarks>
/// Positions are zero-based character indexes in the original pattern string. Segment indexes are
/// zero-based slash-separated segments and are approximate for escaped separators.
/// </remarks>
internal static class PatternTextPosition {
    /// <summary>
    /// Counts the slash-separated segment that contains a character position.
    /// </summary>
    /// <param name="pattern">The raw pattern text.</param>
    /// <param name="position">The character position.</param>
    /// <returns>The zero-based segment index.</returns>
    public static int SegmentAt(ReadOnlySpan<char> pattern, int position) {
        var segment = 0;
        var limit = Math.Min(position, pattern.Length);

        for(var i = 0; i < limit; i++) {
            if(pattern[i] == '/') {
                segment++;
            }
        }

        return segment;
    }

    /// <summary>
    /// Finds the first occurrence of a literal fragment.
    /// </summary>
    /// <param name="pattern">The raw pattern text.</param>
    /// <param name="fragment">The fragment to find.</param>
    /// <returns>The zero-based position, or <see langword="null"/> when absent.</returns>
    public static int? Find(ReadOnlySpan<char> pattern, ReadOnlySpan<char> fragment) {
        var index = pattern.IndexOf(fragment, StringComparison.Ordinal);
        return index < 0 ? null : index;
    }

    /// <summary>
    /// Finds the next occurrence after a previous position.
    /// </summary>
    /// <param name="pattern">The raw pattern text.</param>
    /// <param name="fragment">The fragment to find.</param>
    /// <param name="start">The zero-based position to start searching from.</param>
    /// <returns>The zero-based position, or <see langword="null"/> when absent.</returns>
    public static int? FindAfter(ReadOnlySpan<char> pattern, ReadOnlySpan<char> fragment, int start) {
        if(start >= pattern.Length) {
            return null;
        }

        var index = pattern[start..].IndexOf(fragment, StringComparison.Ordinal);
        return index < 0 ? null : start + index;
    }
}
