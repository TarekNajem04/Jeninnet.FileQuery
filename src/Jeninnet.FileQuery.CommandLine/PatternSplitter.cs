//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.CommandLine;

/// <summary>
/// Provides utilities for parsing lists of pattern expressions from a single input string.
/// <para>
/// This parser is responsible only for splitting and normalizing multi-pattern input.
/// </para>
/// <para>
/// Typical usage:
/// <code>
/// var patterns = PatternSplitter.Split("*.cs;!bin/**");
/// </code>
/// </para>
/// <para>
/// Separators are configurable and default to semicolon (<c>;</c>).
/// Empty entries are removed automatically.
/// </para>
/// </summary>
public static class PatternSplitter {
    /// <summary>
    /// The default character used to separate multiple patterns in a string.
    /// </summary>
    public const char DEFAULT_PATTERN_SEPARATOR = ';';

    /// <summary>
    /// Splits a multi-pattern string into individual pattern expressions.
    /// <para>
    /// This method does not interpret escape sequences or quoting rules.
    /// It is intended for simple, configuration-style pattern lists.
    /// </para>
    /// </summary>
    /// <param name="input">The raw pattern list string (e.g., <c>"*.cs;!bin/**"</c>).</param>
    /// <param name="separator">The character used to separate patterns. Defaults to <c>';'</c>.</param>
    /// <returns>A sequence of individual pattern strings, trimmed and with empty entries removed.</returns>
    /// <exception cref="ArgumentNullException"> Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    public static IEnumerable<string> Split(
        string input,
        char separator = DEFAULT_PATTERN_SEPARATOR
    ) {
        ArgumentException.ThrowIfNullOrEmpty(input);

        return input.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(static p => p.Trim())
                    .Where(static p => p.Length > 0);
    }

    /// <summary>
    /// Splits a multi-pattern string using multiple possible separators.
    /// <para>
    /// This overload is useful when accepting user input from CLI or
    /// configuration files where separators may vary (e.g., <c>','</c>,
    /// <c>';'</c>, or whitespace).
    /// </para>
    /// </summary>
    /// <param name="input">The raw pattern list string.</param>
    /// <param name="separators">A set of characters used to separate patterns.</param>
    /// <returns>A sequence of individual pattern strings, trimmed and with empty entries removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> or <paramref name="separators"/> is <c>null</c>.</exception>
    public static IEnumerable<string> Split(
        string input,
        params char[] separators
    ) {
        ArgumentException.ThrowIfNullOrEmpty(input);
        ArgumentNullException.ThrowIfNull(separators);

        return input.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(static p => p.Trim())
                    .Where(static p => p.Length > 0);
    }
}
