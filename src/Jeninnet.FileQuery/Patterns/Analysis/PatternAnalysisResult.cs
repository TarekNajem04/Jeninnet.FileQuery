namespace Jeninnet.FileQuery.Patterns.Analysis;

/// <summary>
/// Represents extracted structural and semantic signals from a pattern.
/// </summary>
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
