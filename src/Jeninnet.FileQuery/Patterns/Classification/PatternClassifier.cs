//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Classification;

/// <summary>
/// <para>
/// Provides an advanced syntactic classifier for file-matching patterns.
/// This classifier understands:
/// - GitIgnore syntax (negation, root anchoring, directory markers)
/// - POSIX glob syntax (*, **, ?, [ranges])
/// - Escaping rules (\!, \#, \*, \?, \[, \])
/// - Malformed glob/range patterns
/// - Windows-style paths
/// </para>
/// <para>
/// GitIgnore is treated as a superset of glob.
/// Any pattern valid in both → GitIgnore.
/// </para>
/// </summary>
internal static partial class PatternClassifier {
    private static readonly PatternAnalyzer _analyzer = new();

    private static PatternKind ResolveType(CanonicalPattern pattern, PatternInterpretationMode mode) {
        if(pattern.ExplicitType is not null) {
            return pattern.ExplicitType.Value;
        }

        if(mode == PatternInterpretationMode.Specific) {
            throw new PatternException($"Pattern '{pattern.Text}' requires an explicit PatternKind when interpretation mode is set to 'Specific'.");
        }

        return Classify(pattern.Text);
    }

    public static ClassifiedPatternSet Classify(CanonicalPatternSet input, PatternInterpretationMode mode) {
        ArgumentNullException.ThrowIfNull(input);

        var result = new List<ClassifiedPattern>();

        foreach(var pattern in input.Patterns) {
            var type = ResolveType(pattern, mode);

            result.Add(new ClassifiedPattern(pattern.Text, type));
        }

        return new ClassifiedPatternSet { Patterns = result };
    }

    /// <summary>
    /// Classifies a file pattern as GitIgnore, Glob, Regex, or Unknown.
    /// </summary>
    /// <param name="pattern">The pattern string to classify.</param>
    /// <returns>The detected <see cref="PatternKind"/>.</returns>
    public static PatternKind Classify(string pattern) {
        pattern = pattern.Trim();

        var span = pattern.AsSpan();

        var (isMalformed, _) = PatternValidator.Validate(span);
        if(isMalformed) {
            // Store the specific error reason in a way the compiler can access.
            // For now, we mark as Unknown and the compiler can throw based on this.
            // To fully implement 'error index', we would need to pass the error string/index through ClassifiedPattern.
            return PatternKind.Unknown;
        }

        var analysis = _analyzer.Analyze(span);

        // ---------------------------------------------------------
        // 1. Empty
        // ---------------------------------------------------------
        if(analysis.IsEmpty) {
            return PatternKind.GitIgnore;
        }

        // ---------------------------------------------------------
        // 2. Regex (terminal)
        // ---------------------------------------------------------
        if(analysis.IsRegex) {
            return PatternKind.Regex;
        }

        // ---------------------------------------------------------
        // 3. Stray bracket → Glob
        // ---------------------------------------------------------
        if(PatternValidator.HasStrayClosingBracket(span)) {
            return PatternKind.Glob;
        }

        // ---------------------------------------------------------
        // 4. WINDOWS PATH RULE (HIGHEST PRIORITY)
        // ---------------------------------------------------------
        // escaped-only case must NOT be treated as path
        if(analysis.HasBackslash && !analysis.HasEscapedCharacters && !analysis.IsNegated) {
            return PatternKind.Glob;
        }

        // ---------------------------------------------------------
        // 5. Escaped → GitIgnore
        // ---------------------------------------------------------
        if(analysis.HasEscapedCharacters) {
            return PatternKind.GitIgnore;
        }

        // ---------------------------------------------------------
        // 6. GitIgnore syntax
        // ---------------------------------------------------------
        if(analysis.HasGitIgnoreSyntax) {
            return PatternKind.GitIgnore;
        }

        // ---------------------------------------------------------
        // 7. Wildcards default to GitIgnore
        // ---------------------------------------------------------
        if(analysis.HasWildcard || analysis.HasBracket) {
            return PatternKind.GitIgnore;
        }

        // ---------------------------------------------------------
        // 8. Fallback
        // ---------------------------------------------------------
        return PatternKind.GitIgnore;
    }

    // -------------------------------------------------------------------------
    // Regex helpers
    // -------------------------------------------------------------------------
    [GeneratedRegex(@"\\[!\#\*\?\[\]]")]
    internal static partial Regex DetectEscapedCharacters();
}
