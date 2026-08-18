//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Syntax.CharacterClasses;

/// <summary>
/// Marker interface for elements that appear inside a character class
/// (<c>[...]</c>) expression.
/// </summary>
/// <remarks>
/// <para>
/// Implementations form a closed discriminated union. Pattern-match on the
/// concrete type using a <c>switch</c> expression:
/// </para>
/// <code>
/// bool matches = element switch {
///     CharLiteral lit  => lit.Value == c,
///     CharRange   rng  => c >= rng.Start &amp;&amp; c &lt;= rng.End,
///     PosixClass  pos  => MatchesPosix(pos.Name, c),
///     CharacterClassParseError _ => false,
///     _ => false
/// };
/// </code>
/// <para>
/// <strong>Closed set:</strong>
/// <list type="bullet">
///   <item><see cref="CharLiteral"/> — a single literal character</item>
///   <item><see cref="CharRange"/> — an inclusive range such as <c>a-z</c></item>
///   <item><see cref="PosixClass"/> — a POSIX named class such as <c>[:digit:]</c></item>
///   <item><see cref="CharacterClassParseError"/> — compile-time parse error sentinel</item>
/// </list>
/// </para>
/// </remarks>
internal interface ICharacterClassElement;
