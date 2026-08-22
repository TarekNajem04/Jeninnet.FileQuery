//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Syntax;

/// <summary>
/// Defines the syntactic capabilities of a pattern dialect.
/// </summary>
/// <remarks>
/// This is a value object, not a flag set.
/// Invalid combinations are unrepresentable.
/// </remarks>
public sealed record PatternSyntaxProfile {
    /// <summary>
    /// Gets a value indicating whether the dialect supports recursive wildcards (e.g. <c>**</c>).
    /// </summary>
    public bool SupportsRecursiveWildcard { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect has implicit recursive wildcards.
    /// </summary>
    public bool ImplicitRecursiveWildcard { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect supports character classes (e.g. <c>[a-z]</c>).
    /// </summary>
    public bool SupportsCharacterClasses { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect supports single character wildcards (e.g. <c>?</c>).
    /// </summary>
    public bool SupportsSingleCharWildcard { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect supports character escaping (e.g. <c>\*</c>).
    /// </summary>
    public bool SupportsEscaping { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect supports negation patterns (e.g. starting with <c>!</c>).
    /// </summary>
    public bool SupportsNegation { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect supports root anchoring (e.g. starting with <c>/</c>).
    /// </summary>
    public bool SupportsRootAnchoring { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect supports directory-only patterns (e.g. ending with <c>/</c>).
    /// </summary>
    public bool SupportsDirectoryOnly { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialect is a regular expression.
    /// </summary>
    public bool IsRegularExpression { get; init; }

    /// <summary>
    /// Gets the syntax profile for GitIgnore patterns.
    /// </summary>
    public static PatternSyntaxProfile GitIgnore { get; } = new() {
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

    /// <summary>
    /// Gets the syntax profile for Glob patterns.
    /// </summary>
    public static PatternSyntaxProfile Glob { get; } = new() {
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

    /// <summary>
    /// Gets the syntax profile for Regex patterns.
    /// </summary>
    public static PatternSyntaxProfile Regex { get; } = new() {
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

    /// <summary>
    /// Gets the default syntax profile.
    /// </summary>
    public static PatternSyntaxProfile Default { get; } = new() {
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

    /// <summary>
    /// Resolves the appropriate syntax profile for a given <see cref="PatternKind"/>.
    /// </summary>
    /// <param name="type">The pattern type kind.</param>
    /// <returns>A syntax profile representing the dialect's capabilities.</returns>
    public static PatternSyntaxProfile GetProfileForPatternType(PatternKind type) =>
        type switch {
            PatternKind.GitIgnore => GitIgnore,
            PatternKind.Glob => Glob,
            PatternKind.Regex => Regex,
            _ => Default
        };
}
