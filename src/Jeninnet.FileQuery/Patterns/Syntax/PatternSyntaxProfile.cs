namespace Jeninnet.FileQuery.Patterns.Syntax;

/// <summary>
/// Defines the syntactic capabilities of a pattern dialect.
/// </summary>
/// <remarks>
/// This is a value object, not a flag set.
/// Invalid combinations are unrepresentable.
/// </remarks>
public sealed record PatternSyntaxProfile
{
    public bool SupportsRecursiveWildcard { get; init; }
    public bool ImplicitRecursiveWildcard { get; init; }
    public bool SupportsCharacterClasses { get; init; }
    public bool SupportsSingleCharWildcard { get; init; }
    public bool SupportsEscaping { get; init; }
    public bool SupportsNegation { get; init; }
    public bool SupportsRootAnchoring { get; init; }
    public bool SupportsDirectoryOnly { get; init; }

    public bool IsRegularExpression { get; init; }

    public static PatternSyntaxProfile GitIgnore { get; } = new()
    {
        SupportsRecursiveWildcard = true,
        ImplicitRecursiveWildcard = true,
        SupportsCharacterClasses = true,
        SupportsSingleCharWildcard = true,
        SupportsEscaping = true,
        SupportsNegation = true,
        SupportsRootAnchoring = true,
        SupportsDirectoryOnly = true,
        IsRegularExpression = false
    };

    public static PatternSyntaxProfile Glob { get; } = new()
    {
        SupportsRecursiveWildcard = true,
        ImplicitRecursiveWildcard = false,
        SupportsCharacterClasses = true,
        SupportsSingleCharWildcard = true,
        SupportsEscaping = true,
        SupportsNegation = false,
        SupportsRootAnchoring = false,
        SupportsDirectoryOnly = false,
        IsRegularExpression = false
    };

    public static PatternSyntaxProfile Regex { get; } = new()
    {
        SupportsRecursiveWildcard = false,
        ImplicitRecursiveWildcard = false,
        SupportsCharacterClasses = false,
        SupportsSingleCharWildcard = false,
        SupportsEscaping = false,
        SupportsNegation = false,
        SupportsRootAnchoring = false,
        SupportsDirectoryOnly = false,
        IsRegularExpression = true
    };

    public static PatternSyntaxProfile Default { get; } = new()
    {
        SupportsRecursiveWildcard = true,
        ImplicitRecursiveWildcard = true,
        SupportsCharacterClasses = true,
        SupportsSingleCharWildcard = true,
        SupportsEscaping = true,
        SupportsNegation = true,
        SupportsRootAnchoring = true,
        SupportsDirectoryOnly = true,
        IsRegularExpression = true
    };

    public static PatternSyntaxProfile GetProfileForPatternType(PatternKind type) =>
        type switch
        {
            PatternKind.GitIgnore => GitIgnore,
            PatternKind.Glob => Glob,
            PatternKind.Regex => Regex,
            _ => Default
        };
}
