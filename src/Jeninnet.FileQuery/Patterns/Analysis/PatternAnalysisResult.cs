//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Analysis;

/// <summary>
/// Represents extracted structural and semantic signals from a pattern.
/// </summary>
/// <param name="IsEmpty">Whether the pattern is empty.</param>
/// <param name="IsRegex">Whether the pattern is a regex.</param>
/// <param name="IsNegated">Whether the pattern is negated.</param>
/// <param name="HasBackslash">Whether the pattern contains a backslash.</param>
/// <param name="HasForwardSlash">Whether the pattern contains a forward slash.</param>
/// <param name="HasWildcard">Whether the pattern contains a wildcard.</param>
/// <param name="HasSingleCharWildcard">Whether the pattern contains a single char wildcard.</param>
/// <param name="HasBracket">Whether the pattern contains a bracket.</param>
/// <param name="HasRecursiveWildcard">Whether the pattern contains a recursive wildcard.</param>
/// <param name="HasEscapedCharacters">Whether the pattern contains escaped characters.</param>
/// <param name="HasGitIgnoreSyntax">Whether the pattern contains gitignore syntax.</param>
/// <param name="SegmentCount">The number of segments.</param>
internal readonly record struct PatternAnalysisResult(
    bool IsEmpty,
    bool IsRegex,
    bool IsNegated,
    bool HasBackslash,
    bool HasForwardSlash,
    bool HasWildcard,
    bool HasSingleCharWildcard,
    bool HasBracket,
    bool HasRecursiveWildcard,
    bool HasEscapedCharacters,
    bool HasGitIgnoreSyntax,
    int SegmentCount
) {
    public static PatternAnalysisResult Empty() => new(
        IsEmpty: false,
        IsRegex: false,
        IsNegated: false,
        HasBackslash: false,
        HasForwardSlash: false,
        HasWildcard: false,
        HasSingleCharWildcard: false,
        HasBracket: false,
        HasRecursiveWildcard: false,
        HasEscapedCharacters: false,
        HasGitIgnoreSyntax: false,
        SegmentCount: 0
    );

    public static PatternAnalysisResult Regex() => new(
        IsEmpty: false,
        IsRegex: true,
        IsNegated: false,
        HasBackslash: false,
        HasForwardSlash: false,
        HasWildcard: false,
        HasSingleCharWildcard: false,
        HasBracket: false,
        HasRecursiveWildcard: false,
        HasEscapedCharacters: false,
        HasGitIgnoreSyntax: false,
        SegmentCount: 0
    );
}
