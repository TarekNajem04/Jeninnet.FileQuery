//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Expands brace alternation (<c>{a,b}</c>) before pattern compilation.
/// </summary>
/// <remarks>
/// <para>
/// Brace expansion follows shell-style rules: alternatives are separated by top-level commas,
/// groups can be nested, and escaped braces (<c>\{</c>, <c>\}</c>) or braces inside a character
/// class are literal. A single group creates one pattern per alternative; multiple groups form a
/// cartesian product.
/// </para>
/// <para>
/// Expansion is bounded to avoid runaway input. Invalid or unterminated braces raise
/// <see cref="PatternException"/> with a character position and segment.
/// </para>
/// </remarks>
internal static class BraceExpander {
    private const int MAX_EXPANSIONS = 256;

    /// <summary>
    /// Expands all brace groups in a pattern.
    /// </summary>
    /// <param name="pattern">The raw pattern text.</param>
    /// <returns>The expanded patterns in stable alternative order.</returns>
    /// <exception cref="PatternException">Thrown when a brace is unmatched or expansion is too large.</exception>
    public static IReadOnlyList<string> Expand(string pattern) {
        ArgumentNullException.ThrowIfNull(pattern);

        if(pattern.IndexOf('{') < 0 && pattern.IndexOf('}') < 0) {
            return [pattern];
        }

        var results = new List<string>();
        var builder = new StringBuilder(pattern.Length);
        var error = ExpandCore(pattern, 0, builder, results);

        if(error is not null) {
            throw new PatternException(
                error.Message,
                error.Position,
                PatternTextPosition.SegmentAt(pattern.AsSpan(), error.Position));
        }

        return results;
    }

    /// <summary>
    /// Recursively expands the next brace group.
    /// </summary>
    /// <param name="pattern">The text to expand.</param>
    /// <param name="start">The position to start scanning.</param>
    /// <param name="builder">The accumulated prefix.</param>
    /// <param name="results">The expansion sink.</param>
    /// <returns>An error, or <see langword="null"/> on success.</returns>
    private static BraceError? ExpandCore(
        string pattern,
        int start,
        StringBuilder builder,
        List<string> results
    ) {
        var (open, close, groupError) = FindGroup(pattern, start);

        if(groupError is not null) {
            return groupError;
        }

        if(open < 0) {
            builder.Append(pattern, start, pattern.Length - start);
            results.Add(builder.ToString());

            return results.Count > MAX_EXPANSIONS
                ? new BraceError($"Brace expansion exceeds {MAX_EXPANSIONS} alternatives.", Math.Max(0, start - 1))
                : null;
        }

        builder.Append(pattern, start, open - start);

        var suffix = pattern[(close + 1)..];
        var alternatives = SplitAlternatives(pattern, open + 1, close);

        if(alternatives.Count == 0) {
            return new BraceError("Brace alternation must contain at least one alternative.", open);
        }

        foreach(var alternative in alternatives) {
            if(alternative.Length == 0) {
                return new BraceError("Brace alternatives cannot be empty.", open);
            }

            var length = builder.Length;
            var error = ExpandCore(alternative + suffix, 0, builder, results);

            if(error is not null) {
                return error;
            }

            builder.Length = length;
        }

        return null;
    }

    /// <summary>
    /// Finds the next expandable brace group.
    /// </summary>
    /// <param name="pattern">The text to scan.</param>
    /// <param name="start">The position to start scanning.</param>
    /// <returns>The group bounds or an error.</returns>
    private static (int Open, int Close, BraceError? Error) FindGroup(string pattern, int start) {
        var inCharacterClass = false;
        var i = start;

        while(i < pattern.Length) {
            var c = pattern[i];

            if(c == '\\') {
                i += 2;
                continue;
            }

            if(c == '[') {
                inCharacterClass = true;
                i++;
                continue;
            }

            if(c == ']' && inCharacterClass) {
                inCharacterClass = false;
                i++;
                continue;
            }

            if(inCharacterClass) {
                i++;
                continue;
            }

            if(c == '}') {
                return (-1, -1, new BraceError("Closing brace '}' has no matching opening brace '{'.", i));
            }

            if(c != '{') {
                i++;
                continue;
            }

            var close = FindClosingBrace(pattern, i + 1);

            if(close < 0) {
                return (-1, -1, new BraceError("Opening brace '{' has no matching closing brace '}'.", i));
            }

            if(HasTopLevelComma(pattern, i + 1, close)) {
                return (i, close, null);
            }

            // A brace group without a comma is a literal sequence, not alternation.
            i = close + 1;
        }

        return (-1, -1, null);
    }

    /// <summary>
    /// Finds the closing brace of a group, respecting nesting, escapes, and character classes.
    /// </summary>
    /// <param name="pattern">The text to scan.</param>
    /// <param name="start">The position after the opening brace.</param>
    /// <returns>The closing position, or <c>-1</c> when absent.</returns>
    private static int FindClosingBrace(string pattern, int start) {
        var depth = 1;
        var inCharacterClass = false;
        var i = start;

        while(i < pattern.Length) {
            var c = pattern[i];

            if(c == '\\') {
                i += 2;
                continue;
            }

            if(c == '[') {
                inCharacterClass = true;
                i++;
                continue;
            }

            if(c == ']' && inCharacterClass) {
                inCharacterClass = false;
                i++;
                continue;
            }

            if(inCharacterClass) {
                i++;
                continue;
            }

            if(c == '{') {
                depth++;
            } else if(c == '}' && --depth == 0) {
                return i;
            }

            i++;
        }

        return -1;
    }

    /// <summary>
    /// Determines whether a group has a comma at its own nesting level.
    /// </summary>
    /// <param name="pattern">The text to scan.</param>
    /// <param name="start">The position after the opening brace.</param>
    /// <param name="end">The closing brace position.</param>
    private static bool HasTopLevelComma(string pattern, int start, int end) {
        var depth = 0;
        var inCharacterClass = false;
        var i = start;

        while(i < end) {
            var c = pattern[i];

            if(c == '\\') {
                i += 2;
                continue;
            }

            if(c == '[') {
                inCharacterClass = true;
                i++;
                continue;
            }

            if(c == ']' && inCharacterClass) {
                inCharacterClass = false;
                i++;
                continue;
            }

            if(inCharacterClass) {
                i++;
                continue;
            }

            if(c == '{') {
                depth++;
            } else if(c == '}') {
                depth--;
            } else if(c == ',' && depth == 0) {
                return true;
            }

            i++;
        }

        return false;
    }

    /// <summary>
    /// Splits the content of a group on top-level commas.
    /// </summary>
    /// <param name="pattern">The text to scan.</param>
    /// <param name="start">The position after the opening brace.</param>
    /// <param name="end">The closing brace position.</param>
    private static List<string> SplitAlternatives(string pattern, int start, int end) {
        var alternatives = new List<string>();
        var depth = 0;
        var inCharacterClass = false;
        var partStart = start;
        var i = start;

        while(i < end) {
            var c = pattern[i];

            if(c == '\\') {
                i += 2;
                continue;
            }

            if(c == '[') {
                inCharacterClass = true;
                i++;
                continue;
            }

            if(c == ']' && inCharacterClass) {
                inCharacterClass = false;
                i++;
                continue;
            }

            if(inCharacterClass) {
                i++;
                continue;
            }

            if(c == '{') {
                depth++;
            } else if(c == '}') {
                depth--;
            } else if(c == ',' && depth == 0) {
                alternatives.Add(pattern[partStart..i]);
                partStart = i + 1;
            }

            i++;
        }

        alternatives.Add(pattern[partStart..end]);
        return alternatives;
    }

    /// <summary>
    /// Describes an expansion failure.
    /// </summary>
    /// <param name="Message">The actionable failure message.</param>
    /// <param name="Position">The character position of the failure.</param>
    private sealed record BraceError(string Message, int Position);
}
