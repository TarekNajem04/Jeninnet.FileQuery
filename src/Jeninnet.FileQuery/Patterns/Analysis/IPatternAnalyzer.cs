//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Analysis;

/// <summary>
/// Analyzes a file‑matching pattern and extracts structural characteristics.
/// </summary>
/// <remarks>
/// This analyzer is:
/// <list type="bullet">
/// <item><description>single‑pass</description></item>
/// <item><description>allocation‑free</description></item>
/// <item><description>side‑effect free</description></item>
/// </list>
/// It supports GitIgnore‑style syntax, wildcard detection, escaping rules,
/// and a lightweight check for regex-prefixed patterns (<c>r:</c>).
/// </remarks>
internal interface IPatternAnalyzer {
    /// <summary>
    /// Performs structural analysis of the provided pattern.
    /// </summary>
    /// <param name="pattern">The pattern to analyze.</param>
    /// <returns>A <see cref="PatternAnalysisResult"/> describing the pattern.</returns>
    PatternAnalysisResult Analyze(ReadOnlySpan<char> pattern);
}
