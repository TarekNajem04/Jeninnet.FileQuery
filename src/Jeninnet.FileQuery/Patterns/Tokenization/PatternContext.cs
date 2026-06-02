namespace Jeninnet.FileQuery.Patterns.Tokenization;

internal readonly record struct PatternContext(
    bool IsNegated,
    bool IsRootAnchored,
    bool IsDirectoryOnly,
    int Start,
    int End
);
