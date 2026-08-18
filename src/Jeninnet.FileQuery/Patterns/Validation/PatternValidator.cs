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
    public static (bool IsMalformed, string? Error) Validate(ReadOnlySpan<char> pattern) {
        if(pattern.IsEmpty) {
            return (false, null);
        }

        if(EndsWithEscape(pattern)) {
            return (true, "Pattern ends with an unescaped backslash.");
        }

        if(!pattern.Contains('[')) {
            return (false, null);
        }

        if(!pattern.Contains(']')) {
            return (true, "Missing closing bracket ']' for an opening bracket '['.");
        }

        if(ContainsEmptyBracket(pattern)) {
            return (true, "Empty bracket expression '[]' is not allowed.");
        }

        var str = pattern.ToString();

        if(DetectNestedBrackets().IsMatch(str)) {
            return (true, "Nested brackets are not supported (e.g. '[[a-z]]').");
        }

        if(InvalidRange().IsMatch(str)) {
            return (true, "Invalid range syntax detected (e.g. '[a-]').");
        }

        if(InvalidRangeWithMissingLeftOperand().IsMatch(str)) {
            return (true, "Range missing left operand (e.g. '[-z]').");
        }

        if(DetectNonsenseLike().IsMatch(str)) {
            return (true, "Nonsensical double-dash sequence detected (e.g. '[--]').");
        }

        return (false, null);
    }

    public static bool IsMalformed(ReadOnlySpan<char> pattern) => Validate(pattern).IsMalformed;

    /// <summary>
    /// Returns <see langword="true"/> when a closing bracket appears in the pattern
    /// without a corresponding opening bracket.
    /// </summary>
    /// <param name="pattern">The pattern span to check.</param>
    public static bool HasStrayClosingBracket(ReadOnlySpan<char> pattern) => pattern.Contains(']') && !pattern.Contains('[');

    // ------------------------------------------------------------------
    // Span-based helpers (zero allocation)
    // ------------------------------------------------------------------

    private static bool EndsWithEscape(ReadOnlySpan<char> pattern) => pattern[^1] == '\\';

    private static bool ContainsEmptyBracket(ReadOnlySpan<char> pattern) => pattern.Contains("[]", StringComparison.Ordinal);

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
    /// and a <em>"No compiler registered for pattern type Unknown"</em>
    /// <see cref="PatternException"/> to be thrown at compile time.
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
