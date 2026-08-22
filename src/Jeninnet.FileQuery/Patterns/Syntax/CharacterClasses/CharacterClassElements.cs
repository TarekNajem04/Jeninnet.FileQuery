//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Syntax.CharacterClasses;

/// <summary>
/// Represents a single literal character inside a character class.
/// </summary>
/// <remarks>
/// A literal matches exactly the character <see cref="Value"/>.
/// <c>'-'</c> and <c>']'</c> are represented as <see cref="CharLiteral"/> when they
/// appear at positions where they cannot form a range delimiter or closing bracket
/// (i.e., as the first element of the class).
/// </remarks>
/// <param name="Value">The literal character to match.</param>
internal sealed record CharLiteral(char Value) : ICharacterClassElement;

/// <summary>
/// Represents an inclusive character range inside a character class (e.g., <c>a-z</c>).
/// </summary>
/// <remarks>
/// A valid range requires <c><see cref="Start"/> &lt;= <see cref="End"/></c>.
/// Inverted ranges (where <see cref="Start"/> &gt; <see cref="End"/>) are not rejected
/// by the parser; they are detected by <see cref="CharacterClassRangeInvariant"/>
/// during the structural invariant phase.
/// </remarks>
/// <param name="Start">The first character of the range (inclusive).</param>
/// <param name="End">The last character of the range (inclusive).</param>
internal sealed record CharRange(char Start, char End) : ICharacterClassElement;

/// <summary>
/// Represents a POSIX named character class inside a bracket expression
/// (e.g., <c>[:digit:]</c>, <c>[:alpha:]</c>).
/// </summary>
/// <remarks>
/// <para>
/// Supported POSIX class names and their .NET equivalents:
/// </para>
/// <list type="table">
///   <listheader><term>Name</term><description>Characters matched</description></listheader>
///   <item><term><c>digit</c></term><description><see cref="char.IsDigit(char)"/></description></item>
///   <item><term><c>alpha</c></term><description><see cref="char.IsLetter(char)"/></description></item>
///   <item><term><c>alnum</c></term><description><see cref="char.IsLetterOrDigit(char)"/></description></item>
///   <item><term><c>space</c></term><description><see cref="char.IsWhiteSpace(char)"/></description></item>
///   <item><term><c>blank</c></term><description>Space or horizontal tab</description></item>
///   <item><term><c>upper</c></term><description><see cref="char.IsUpper(char)"/></description></item>
///   <item><term><c>lower</c></term><description><see cref="char.IsLower(char)"/></description></item>
///   <item><term><c>print</c></term><description>Non-control characters</description></item>
///   <item><term><c>graph</c></term><description>Non-control, non-space characters</description></item>
///   <item><term><c>punct</c></term><description>Punctuation and symbol characters</description></item>
///   <item><term><c>cntrl</c></term><description><see cref="char.IsControl(char)"/></description></item>
///   <item><term><c>xdigit</c></term><description>Hexadecimal digits (0-9, a-f, A-F)</description></item>
/// </list>
/// <para>
/// Unknown names silently match no characters (safe default).
/// </para>
/// </remarks>
/// <param name="Name">
/// The POSIX class name without delimiters, e.g., <c>"digit"</c> for <c>[:digit:]</c>.
/// </param>
internal sealed record PosixClass(string Name) : ICharacterClassElement;

/// <summary>
/// A compile-time sentinel element that records a structural parse error
/// inside a character class.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CharacterClassParser"/> never throws. When it encounters a
/// structural problem (unterminated bracket, invalid POSIX syntax, incomplete
/// escape sequence), it appends a <see cref="CharacterClassParseError"/> to the
/// <see cref="CharacterClass.Elements"/> list and halts further element parsing.
/// </para>
/// <para>
/// <see cref="CharacterClassStructureInvariant"/> inspects compiled
/// tokens for this sentinel and converts it into a
/// <see cref="PatternInvariantResult"/> failure, which is
/// how the error surfaces to the caller as a
/// <see cref="PatternException"/>.
/// </para>
/// <para>
/// A <see cref="CharacterClassParseError"/> element <strong>never matches any
/// character</strong> at runtime — it is a compile-time artifact only.
/// </para>
/// </remarks>
/// <param name="Message">Human-readable description of the parse error.</param>
internal sealed record CharacterClassParseError(string Message) : ICharacterClassElement;
