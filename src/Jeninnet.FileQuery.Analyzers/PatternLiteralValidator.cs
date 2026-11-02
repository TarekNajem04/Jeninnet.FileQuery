//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System;
using System.Text.RegularExpressions;

namespace Jeninnet.FileQuery.Analyzers;

/// <summary>
/// Structural validation rules shared by the analyzer and the code fix.
/// </summary>
/// <remarks>
/// The rules mirror <c>PatternInspector.Validate</c> from the Tooling SDK. They are copied
/// into the analyzer because the analyzer targets <c>netstandard2.0</c> and cannot load a
/// <c>net10.0</c> library in the compiler process.
/// </remarks>
internal static class PatternLiteralValidator {
    /// <summary>The malformation kind for a trailing unescaped backslash.</summary>
    public const string TRAILING_BACKSLASH = "TrailingBackslash";

    /// <summary>The malformation kind for an opening bracket without a closing bracket.</summary>
    public const string MISSING_CLOSING_BRACKET = "MissingClosingBracket";

    /// <summary>The malformation kind for an empty bracket expression.</summary>
    public const string EMPTY_BRACKET = "EmptyBracket";

    /// <summary>The malformation kind for nested brackets.</summary>
    public const string NESTED_BRACKETS = "NestedBrackets";

    /// <summary>The malformation kind for invalid range syntax.</summary>
    public const string INVALID_RANGE = "InvalidRange";

    /// <summary>The malformation kind for a range without a left operand.</summary>
    public const string MISSING_RANGE_OPERAND = "MissingRangeOperand";

    /// <summary>The malformation kind for a nonsensical double-dash sequence.</summary>
    public const string NONSENSE_DASH = "NonsenseDash";

    private static readonly Regex _nestedBrackets = new(
        @"\[[^\]]*\[(?!:)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex _invalidRange = new(
        @"\[[^\]]*-\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex _missingRangeOperand = new(
        @"\[\-[A-Za-z]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex _nonsenseDash = new(
        @"\[\-\-",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Validates a pattern literal.
    /// </summary>
    /// <param name="pattern">The pattern value.</param>
    /// <returns>The issue, or <see langword="null"/> when the pattern is structurally valid.</returns>
    public static PatternIssue? Validate(string pattern) {
        if(pattern.Length == 0) {
            return null;
        }

        if(pattern[pattern.Length - 1] == '\\') {
            return new PatternIssue(
                TRAILING_BACKSLASH,
                "the pattern ends with an unescaped backslash.");
        }

        if(pattern.IndexOf('[') < 0) {
            return null;
        }

        if(pattern.IndexOf(']') < 0) {
            return new PatternIssue(
                MISSING_CLOSING_BRACKET,
                "missing closing bracket ']' for an opening bracket '['.");
        }

        if(pattern.IndexOf("[]", System.StringComparison.Ordinal) >= 0) {
            return new PatternIssue(
                EMPTY_BRACKET,
                "empty bracket expression '[]' is not allowed.");
        }

        if(_nestedBrackets.IsMatch(pattern)) {
            return new PatternIssue(
                NESTED_BRACKETS,
                "nested brackets are not supported (e.g. '[[a-z]]').");
        }

        if(_invalidRange.IsMatch(pattern)) {
            return new PatternIssue(
                INVALID_RANGE,
                "invalid range syntax detected (e.g. '[a-]').");
        }

        if(_missingRangeOperand.IsMatch(pattern)) {
            return new PatternIssue(
                MISSING_RANGE_OPERAND,
                "range missing left operand (e.g. '[-z]').");
        }

        if(_nonsenseDash.IsMatch(pattern)) {
            return new PatternIssue(
                NONSENSE_DASH,
                "nonsensical double-dash sequence detected (e.g. '[--]').");
        }

        return null;
    }
}

/// <summary>
/// Describes a structural pattern issue.
/// </summary>
/// <param name="kind">The machine-readable malformation kind.</param>
/// <param name="message">The human-readable explanation.</param>
internal sealed class PatternIssue(string kind, string message) {

    /// <summary>
    /// Gets the machine-readable malformation kind.
    /// </summary>
    public string Kind { get; } = kind;

    /// <summary>
    /// Gets the human-readable explanation.
    /// </summary>
    public string Message { get; } = message;
}
