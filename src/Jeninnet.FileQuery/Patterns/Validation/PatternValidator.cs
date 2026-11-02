//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Validation;

/// <summary>
/// Provides structural validation for file-matching patterns.
/// </summary>
/// <remarks>
/// This component is responsible <em>only</em> for detecting patterns that are
/// structurally malformed at the raw-text level. It does not classify or interpret
/// semantics, and it never throws — it returns a <see langword="bool"/> so that
/// the classifier can route the pattern to <see cref="PatternKind.Unknown"/>
/// without propagating an exception.
/// </remarks>
internal static partial class PatternValidator {
    /// <summary>
    /// Determines whether a pattern is structurally invalid.
    /// </summary>
    /// <param name="pattern">The raw pattern string.</param>
    /// <returns>
    /// <see langword="true"/> if the pattern is malformed; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Checks applied in order:
    /// <list type="number">
    ///   <item>Trailing unescaped backslash.</item>
    ///   <item>Opening <c>'['</c> without a matching closing <c>']'</c>.</item>
    ///   <item>Empty bracket expression <c>"[]"</c> (excluding the first-element
    ///         literal case, which the parser handles internally).</item>
    ///   <item>Genuinely nested brackets such as <c>"[[a-z]]"</c> — but
    ///         <em>not</em> POSIX class syntax <c>"[[:digit:]]"</c>
    ///         (see <see cref="DetectNestedBrackets"/>).</item>
    ///   <item>Invalid range syntax <c>"[a-]"</c>.</item>
    ///   <item>Range with a missing left operand <c>"[-z]"</c>.</item>
    ///   <item>Nonsense double-dash <c>"[--"</c>.</item>
    /// </list>
    /// </remarks>
    public static (bool IsMalformed, string? Error, int? Position, int? Segment) Validate(ReadOnlySpan<char> pattern) {
        if(pattern.IsEmpty) {
            return (false, null, null, null);
        }

        // Regex syntax is validated by RegexSyntaxInvariant. Brace and bracket
        // pairing rules are glob/GitIgnore grammar and do not apply to expressions
        // such as r:^a}$ or r:a{2, where '}' and '{' are literal regex characters.
        if(pattern.StartsWith("r:", StringComparison.Ordinal)) {
            return (false, null, null, null);
        }

        if(EndsWithUnescapedBackslash(pattern)) {
            return (true, "Pattern ends with an unescaped backslash.", pattern.Length - 1, PatternTextPosition.SegmentAt(pattern, pattern.Length - 1));
        }

        var braceError = ValidateBraces(pattern);
        if(braceError is not null) {
            var position = braceError.Value.Position;
            return (true, braceError.Value.Message, position, PatternTextPosition.SegmentAt(pattern, position));
        }

        var str = MaskEscapedCharacters(pattern);
        var openingBracket = str.IndexOf('[');

        if(openingBracket < 0) {
            return (false, null, null, null);
        }

        if(str.IndexOf(']') < 0) {
            return (true, "Missing closing bracket ']' for an opening bracket '['.", openingBracket, PatternTextPosition.SegmentAt(pattern, openingBracket));
        }

        if(str.Contains("[]", StringComparison.Ordinal)) {
            var emptyBracket = str.IndexOf("[]", StringComparison.Ordinal);
            return (true, "Empty bracket expression '[]' is not allowed.", emptyBracket, PatternTextPosition.SegmentAt(pattern, emptyBracket));
        }

        var nested = DetectNestedBrackets().Match(str);
        if(nested.Success) {
            return (true, "Nested brackets are not supported (e.g. '[[a-z]]').", nested.Index, PatternTextPosition.SegmentAt(pattern, nested.Index));
        }

        var invalidRange = InvalidRange().Match(str);
        if(invalidRange.Success) {
            return (true, "Invalid range syntax detected (e.g. '[a-]').", invalidRange.Index, PatternTextPosition.SegmentAt(pattern, invalidRange.Index));
        }

        var missingLeft = InvalidRangeWithMissingLeftOperand().Match(str);
        if(missingLeft.Success) {
            return (true, "Range missing left operand (e.g. '[-z]').", missingLeft.Index, PatternTextPosition.SegmentAt(pattern, missingLeft.Index));
        }

        var nonsense = DetectNonsenseLike().Match(str);
        if(nonsense.Success) {
            return (true, "Nonsensical double-dash sequence detected (e.g. '[--]').", nonsense.Index, PatternTextPosition.SegmentAt(pattern, nonsense.Index));
        }

        return (false, null, null, null);
    }

    public static bool IsMalformed(ReadOnlySpan<char> pattern) => Validate(pattern).IsMalformed;

    /// <summary>
    /// Returns <see langword="true"/> when a closing bracket appears in the pattern
    /// without a corresponding opening bracket.
    /// </summary>
    /// <param name="pattern">The pattern span to check.</param>
    public static bool HasStrayClosingBracket(ReadOnlySpan<char> pattern) =>
        IndexOfUnescaped(pattern, ']') >= 0 && IndexOfUnescaped(pattern, '[') < 0;

    // ------------------------------------------------------------------
    // Span-based helpers (zero allocation)
    // ------------------------------------------------------------------

    private static bool EndsWithUnescapedBackslash(ReadOnlySpan<char> pattern) {
        var count = 0;

        for(var i = pattern.Length - 1; i >= 0 && pattern[i] == '\\'; i--) {
            count++;
        }

        return count % 2 != 0;
    }

    /// <summary>
    /// Replaces escaped characters with spaces so structural bracket checks ignore them.
    /// </summary>
    private static string MaskEscapedCharacters(ReadOnlySpan<char> pattern) {
        var chars = pattern.ToArray();
        var i = 0;

        while(i < chars.Length) {
            if(chars[i] != '\\') {
                i++;
                continue;
            }

            chars[i] = ' ';

            if(i + 1 < chars.Length) {
                chars[i + 1] = ' ';
                i += 2;
            } else {
                i++;
            }
        }

        return new string(chars);
    }

    private static int IndexOfUnescaped(ReadOnlySpan<char> pattern, char value) {
        var i = 0;

        while(i < pattern.Length) {
            if(pattern[i] == '\\') {
                i += 2;
                continue;
            }

            if(pattern[i] == value) {
                return i;
            }

            i++;
        }

        return -1;
    }

    /// <summary>
    /// Validates brace pairing outside character classes and escapes.
    /// </summary>
    /// <param name="pattern">The raw pattern text.</param>
    /// <returns>An error message and position, or <see langword="null"/> when braces are balanced.</returns>
    private static (string Message, int Position)? ValidateBraces(ReadOnlySpan<char> pattern) {
        var depth = 0;
        var firstOpen = -1;
        var inCharacterClass = false;
        var i = 0;

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
                if(depth == 0) {
                    firstOpen = i;
                }

                depth++;
            } else if(c == '}') {
                if(depth == 0) {
                    return ("Closing brace '}' has no matching opening brace '{'.", i);
                }

                depth--;
            }

            i++;
        }

        if(depth > 0) {
            return ("Opening brace '{' has no matching closing brace '}'.", firstOpen);
        }

        return null;
    }

    // ------------------------------------------------------------------
    // Source-generated Regex helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Detects genuinely nested bracket expressions such as <c>"[[a-z]]"</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pattern: <c>\[[^\]]*\[(?!:)</c>
    /// </para>
    /// <para>
    /// Breakdown:
    /// <list type="bullet">
    ///   <item><c>\[</c> — an opening bracket</item>
    ///   <item><c>[^\]]*</c> — zero or more characters that are not <c>']'</c></item>
    ///   <item><c>\[</c> — a second opening bracket</item>
    ///   <item><c>(?!:)</c> — <strong>negative lookahead</strong>: the second
    ///         <c>'['</c> must <em>not</em> be immediately followed by <c>':'</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Why the lookahead is required:</strong> POSIX class syntax embeds a
    /// <c>[:</c> prefix inside an outer bracket expression, e.g.
    /// <c>[[:digit:]]</c> or <c>[a-z[:upper:]]</c>. Without the lookahead, the
    /// regex matches <c>[[</c> at the start of any POSIX class and incorrectly
    /// classifies the pattern as malformed, causing
    /// <see cref="PatternKind.Unknown"/> to be returned by the classifier
    /// and a descriptive <see cref="PatternException"/> to be thrown at compile time.
    /// </para>
    /// <para>
    /// <strong>Examples:</strong>
    /// <list type="table">
    ///   <listheader><term>Input</term><description>Matches (malformed)?</description></listheader>
    ///   <item><term><c>[[a-z]]</c></term><description>Yes — genuine nested bracket</description></item>
    ///   <item><term><c>[[:digit:]]</c></term><description>No — POSIX class, excluded by lookahead</description></item>
    ///   <item><term><c>[a[:upper:]]</c></term><description>No — POSIX class mixed with literal</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [GeneratedRegex(@"\[[^\]]*\[(?!:)")]
    private static partial Regex DetectNestedBrackets();

    /// <summary>Detects invalid range syntax such as <c>"[a-]"</c>.</summary>
    [GeneratedRegex(@"\[[^\]]*-\]")]
    private static partial Regex InvalidRange();

    /// <summary>Detects a range missing its left operand, such as <c>"[-z]"</c>.</summary>
    [GeneratedRegex(@"\[\-[A-Za-z]")]
    private static partial Regex InvalidRangeWithMissingLeftOperand();

    /// <summary>Detects nonsensical double-dash sequences such as <c>"[--"</c>.</summary>
    [GeneratedRegex(@"\[\-\-")]
    private static partial Regex DetectNonsenseLike();
}
