//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.Enums;

namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// A complete, immutable inspection of a single pattern.
/// </summary>
/// <param name="Pattern">The inspected pattern text.</param>
/// <param name="Kind">The classified dialect, or <see cref="PatternKind.Unknown"/> when the pattern is malformed.</param>
/// <param name="IsRegex">Whether the pattern is a regular expression (the <c>r:</c> prefix).</param>
/// <param name="IsNegated">Whether the pattern starts with the GitIgnore negation marker <c>!</c>.</param>
/// <param name="IsRootAnchored">Whether the pattern is anchored to the query root.</param>
/// <param name="IsDirectoryOnly">Whether the pattern is a directory-only GitIgnore rule (trailing <c>/</c>).</param>
/// <param name="HasGitIgnoreSyntax">Whether the pattern uses GitIgnore-specific syntax.</param>
/// <param name="HasWildcard">Whether the pattern contains a wildcard.</param>
/// <param name="HasRecursiveWildcard">Whether the pattern contains the recursive wildcard <c>**</c>.</param>
/// <param name="HasCharacterClass">Whether the pattern contains a bracket expression.</param>
/// <param name="HasEscapedCharacters">Whether the pattern contains escaped characters.</param>
/// <param name="SegmentCount">The number of slash-separated segments in the pattern.</param>
/// <param name="Validation">The structural validation result.</param>
/// <param name="Segments">The tokenized segments, in source order.</param>
public sealed record PatternInspection(
    string Pattern,
    PatternKind Kind,
    bool IsRegex,
    bool IsNegated,
    bool IsRootAnchored,
    bool IsDirectoryOnly,
    bool HasGitIgnoreSyntax,
    bool HasWildcard,
    bool HasRecursiveWildcard,
    bool HasCharacterClass,
    bool HasEscapedCharacters,
    int SegmentCount,
    PatternValidation Validation,
    IReadOnlyList<PatternSegmentInfo> Segments
);
