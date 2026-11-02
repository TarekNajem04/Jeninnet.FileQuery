//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// Identifies the kind of an atomic token produced by the pattern tokenizer.
/// </summary>
public enum PatternTokenKind {
    /// <summary>Literal text that must match exactly.</summary>
    Literal = 0,

    /// <summary>The single wildcard <c>*</c>.</summary>
    Wildcard = 1,

    /// <summary>The recursive wildcard <c>**</c>.</summary>
    RecursiveWildcard = 2,

    /// <summary>The single-character wildcard <c>?</c>.</summary>
    SingleCharacterWildcard = 3,

    /// <summary>A character class such as <c>[a-z]</c> or <c>[[:digit:]]</c>.</summary>
    CharacterClass = 4,

    /// <summary>A regular expression introduced by the <c>r:</c> prefix.</summary>
    RegularExpression = 5,

    /// <summary>
    /// An escaped character. The current tokenizer resolves escapes into literal tokens, so this
    /// value is reserved for tokenizers that preserve the distinction; use
    /// <see cref="PatternInspection.HasEscapedCharacters"/> to detect escaping.
    /// </summary>
    EscapedCharacter = 6,

    /// <summary>The token kind is not recognized by this SDK version.</summary>
    Unknown = 7
}
