namespace Jeninnet.FileQuery.Patterns.Syntax;

/// <summary>
/// Represents the smallest atomic unit of a parsed file-system pattern.
/// </summary>
/// <remarks>
/// A pattern is broken into segments (directory parts), and each segment
/// consists of an ordered sequence of tokens. Matchers evaluate tokens in
/// sequence without allocating new objects, ensuring high performance.
/// All <see cref="PatternToken"/> subclasses are immutable sealed records.
/// Pattern compilers produce tokens once; matchers reuse them forever,
/// enabling fully allocation-free match pipelines.
/// </remarks>
internal abstract record PatternToken : IPatternToken;

// ------------------------------------------------------------------------
//  Literal
// ------------------------------------------------------------------------

/// <summary>
/// A literal string of characters that must match exactly.
/// </summary>
/// <remarks>
/// Literal tokens represent sequences such as <c>"foo"</c>, <c>"bar.cs"</c>,
/// or any text that contains no wildcard symbols. They are already normalized
/// and escape-resolved by the time they reach the matcher.
/// </remarks>
/// <param name="Text">The literal text. Compared using the active <see cref="StringComparison"/> mode.</param>
internal sealed record LiteralToken(string Text) : PatternToken
{
    /// <inheritdoc/>
    public override string ToString() => $"Literal({Text})";
}

// ------------------------------------------------------------------------
//  "*"
// ------------------------------------------------------------------------

/// <summary>
/// Represents the single wildcard <c>*</c>.
/// </summary>
/// <remarks>
/// Matches any sequence of characters <strong>excluding</strong> the
/// directory separator <c>'/'</c>. Does not match zero characters when
/// bounded by other tokens (standard backtracking applies).
/// </remarks>
internal sealed record WildcardToken : PatternToken
{
    /// <inheritdoc/>
    public override string ToString() => "*";
}

// ------------------------------------------------------------------------
//  "**"
// ------------------------------------------------------------------------

/// <summary>
/// Represents the recursive wildcard <c>**</c>.
/// </summary>
/// <remarks>
/// Matches across directory boundaries, including zero or more complete
/// path segments. Must occupy an entire segment on its own; mixed segments
/// such as <c>foo**</c> are rejected by
/// <see cref="GlobPatternInvariant"/>.
/// </remarks>
internal sealed record RecursiveWildcardToken : PatternToken
{
    /// <inheritdoc/>
    public override string ToString() => "**";
}

// ------------------------------------------------------------------------
//  "?"
// ------------------------------------------------------------------------

/// <summary>
/// Represents the single-character wildcard <c>?</c>.
/// </summary>
/// <remarks>
/// Matches exactly one character that is <strong>not</strong> the directory
/// separator <c>'/'</c>.
/// </remarks>
internal sealed record SingleCharToken : PatternToken
{
    /// <inheritdoc/>
    public override string ToString() => "?";
}

// ------------------------------------------------------------------------
//  Character Class "[...]"
// ------------------------------------------------------------------------

/// <summary>
/// Represents a character class expression (<c>[...]</c>).
/// </summary>
/// <remarks>
/// <para>
/// The class is stored as a <see cref="CharacterClass"/> AST node
/// produced by <see cref="CharacterClassParser"/>. The matcher
/// pattern-matches on the concrete <see cref="ICharacterClassElement"/>
/// types at evaluation time.
/// </para>
/// <para>
/// Supported element kinds:
/// <list type="bullet">
///   <item><see cref="CharLiteral"/> — literal character</item>
///   <item><see cref="CharRange"/> — inclusive range (e.g. <c>a-z</c>)</item>
///   <item><see cref="PosixClass"/> — POSIX class (e.g. <c>[:digit:]</c>)</item>
///   <item><see cref="CharacterClassParseError"/> — parse error sentinel (never matches)</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Value">The parsed character class AST.</param>
internal sealed record CharacterClassToken(CharacterClass Value) : PatternToken
{
    /// <inheritdoc/>
    public override string ToString()
    {
        var neg = Value.IsNegated ? "!" : "";
        return $"[{neg}…]";
    }
}

// ------------------------------------------------------------------------
//  Regular Expression "r:..."
// ------------------------------------------------------------------------

/// <summary>
/// Represents a regular expression pattern (prefixed with <c>r:</c>).
/// </summary>
/// <param name="Pattern">The raw regular expression string without the <c>r:</c> prefix.</param>
internal sealed record RegularExpressionToken(string Pattern) : PatternToken
{
    /// <inheritdoc/>
    public override string ToString() => Pattern;
}

// ------------------------------------------------------------------------
//  Escape
// ------------------------------------------------------------------------

/// <summary>
/// Represents a single escaped character in a pattern expression.
/// </summary>
/// <remarks>
/// Used for characters that would otherwise carry special meaning:
/// <c>'*'</c>, <c>'?'</c>, <c>'!'</c>, <c>'#'</c>, <c>'['</c>, <c>']'</c>, <c>'\'</c>.
/// </remarks>
/// <param name="Escaped">The character whose special meaning has been suppressed.</param>
internal sealed record EscapeToken(char Escaped) : PatternToken
{
    /// <inheritdoc/>
    public override string ToString() => Escaped.ToString();
}
