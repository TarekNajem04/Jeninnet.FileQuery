//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Syntax.CharacterClasses;

/// <summary>
/// Represents a parsed character class expression (<c>[...]</c>).
/// </summary>
/// <remarks>
/// <para>
/// This is a pure syntax-level AST node. No matching logic is embedded here.
/// The matcher interprets <see cref="Elements"/> at match time via a
/// <c>switch</c> expression over the <see cref="ICharacterClassElement"/> discriminated union.
/// </para>
/// <para>
/// A <see cref="CharacterClass"/> is produced by <see cref="CharacterClassParser"/> and
/// stored inside a <see cref="CharacterClassToken"/>. The parser never throws;
/// structural problems are represented as <see cref="CharacterClassParseError"/> elements
/// and reported by <see cref="CharacterClassStructureInvariant"/> during the
/// invariant phase.
/// </para>
/// </remarks>
/// <param name="IsNegated">
/// <see langword="true"/> when the class was prefixed with <c>!</c> or <c>^</c>,
/// meaning it matches characters <em>not</em> in the element set.
/// </param>
/// <param name="Elements">
/// The ordered sequence of elements inside the bracket expression.
/// May contain at most one <see cref="CharacterClassParseError"/>; when one is present
/// the class is considered malformed and no other elements are meaningful.
/// </param>
internal sealed record CharacterClass(
    bool IsNegated,
    IReadOnlyList<ICharacterClassElement> Elements
);
