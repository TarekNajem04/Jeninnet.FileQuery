namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Parses a character class expression (<c>[...]</c>) into a
/// <see cref="CharacterClass"/> AST node.
/// </summary>
/// <remarks>
/// <para>
/// <strong>No-throw contract:</strong> this parser never throws.
/// Structural problems (unterminated bracket, invalid POSIX syntax,
/// incomplete escape) are recorded as a
/// <see cref="CharacterClassParseError"/> sentinel element appended to
/// the <see cref="CharacterClass.Elements"/> list. The invariant system
/// (<see cref="CharacterClassStructureInvariant"/>) detects these
/// sentinels during the structural phase and converts them into
/// <see cref="PatternException"/> failures.
/// This preserves the architectural contract that
/// <see cref="PatternScanner"/> must never throw a
/// <see cref="PatternException"/>.
/// </para>
///
/// <para>
/// <strong>Supported syntax:</strong>
/// <list type="bullet">
///   <item><c>[abc]</c> — literal set</item>
///   <item><c>[a-z]</c> — inclusive character range</item>
///   <item><c>[!abc]</c> or <c>[^abc]</c> — negated class</item>
///   <item><c>[[:digit:]]</c> — POSIX named class</item>
///   <item><c>[\*]</c> — escaped literal inside class</item>
///   <item><c>[]abc]</c> — <c>']'</c> as a literal when it is the first element</item>
///   <item><c>[-abc]</c> — <c>'-'</c> as a literal when it is the first element</item>
/// </list>
/// </para>
///
/// <para>
/// <strong>Calling convention:</strong>
/// The caller passes a <c>ref int</c> pointing at the opening <c>'['</c>.
/// On return, the index is positioned at the first character
/// after the closing <c>']'</c>, or at <c>pattern.Length</c> if the
/// class was unterminated.
/// </para>
///
/// </remarks>
internal static class CharacterClassParser {
    /// <summary>
    /// Parses the character class starting at <paramref name="index"/>.
    /// </summary>
    /// <param name="pattern">The segment being tokenized. Must not be empty.</param>
    /// <param name="index">
    /// On entry: the position of the opening <c>'['</c>.
    /// On return: the position immediately after the closing <c>']'</c>,
    /// or <c>pattern.Length</c> if the class was unterminated.
    /// </param>
    /// <returns>
    /// A <see cref="CharacterClass"/> AST node. When parsing fails, the
    /// <see cref="CharacterClass.Elements"/> list contains exactly one
    /// <see cref="CharacterClassParseError"/> sentinel.
    /// </returns>
    public static CharacterClass Parse(ReadOnlySpan<char> pattern, ref int index) {
        Debug.Assert(
            index < pattern.Length && pattern[index] == '[',
            "Caller must ensure index points to the opening '['.");

        index++; // consume '['

        var isNegated = ParseNegation(pattern, ref index);
        var elements = new List<ICharacterClassElement>();
        var first = true;
        var closed = false;

        while(index < pattern.Length) {
            if(IsClassEnd(pattern, index, first)) {
                index++; // consume ']'
                closed = true;
                break;
            }

            var element = ParseElement(pattern, ref index, first);
            elements.Add(element);

            // Abort accumulation on the first parse error.
            // The sentinel is enough information for the invariant system.
            if(element is CharacterClassParseError) {
                break;
            }

            first = false;
        }

        // Avoid adding a second sentinel if ParseElement already added one.
        if(!closed && elements is not [.., CharacterClassParseError]) {
            elements.Add(new CharacterClassParseError("Unterminated character class."));
        }

        return new CharacterClass(isNegated, elements.AsReadOnly());
    }

    /// <summary>
    /// Consumes an optional negation prefix (<c>!</c> or <c>^</c>).
    /// </summary>
    private static bool ParseNegation(ReadOnlySpan<char> pattern, ref int index) {
        if(index < pattern.Length && pattern[index] is '!' or '^') {
            index++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the character at <paramref name="pos"/> is
    /// <c>']'</c> and it is not the very first element position.
    /// </summary>
    /// <remarks>
    /// POSIX and most glob engines allow <c>']'</c> as a literal character when it
    /// appears as the first element (or the first element of a negated class).
    /// The <paramref name="first"/> flag tracks whether any element has been consumed yet.
    /// </remarks>
    private static bool IsClassEnd(ReadOnlySpan<char> pattern, int pos, bool first) =>
        pattern[pos] == ']' && !first;

    /// <summary>
    /// Parses the next element at <paramref name="index"/> and advances the index.
    /// </summary>
    private static ICharacterClassElement ParseElement(
        ReadOnlySpan<char> pattern,
        ref int index,
        bool first
    ) {
        // POSIX class: [: ... :]
        if(IsPosixStart(pattern, index)) {
            return ParsePosix(pattern, ref index);
        }

        // Escape sequence: \x
        if(pattern[index] == '\\') {
            return ParseEscape(pattern, ref index);
        }

        // Literal or range
        return ParseRangeOrLiteral(pattern, ref index, first);
    }

    /// <summary>
    /// Parses an escape sequence <c>\x</c> inside a character class.
    /// </summary>
    private static ICharacterClassElement ParseEscape(
        ReadOnlySpan<char> pattern,
        ref int index
    ) {
        index++; // skip '\'

        if(index >= pattern.Length) {
            return new CharacterClassParseError(
                "Incomplete escape sequence: '\\' at end of character class.");
        }

        return new CharLiteral(pattern[index++]);
    }

    /// <summary>
    /// Parses either a character range (<c>x-y</c>) or a single literal character.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><c>'-'</c> as the first element → <see cref="CharLiteral"/></item>
    ///   <item><c>']'</c> as the first element → <see cref="CharLiteral"/></item>
    ///   <item><c>x-y</c> where <c>y</c> is not <c>']'</c> → <see cref="CharRange"/></item>
    ///   <item>anything else → <see cref="CharLiteral"/></item>
    /// </list>
    /// </remarks>
    private static ICharacterClassElement ParseRangeOrLiteral(
        ReadOnlySpan<char> pattern,
        ref int index,
        bool first
    ) {
        var c = pattern[index];

        // '-' as the first element is a literal, not a range delimiter.
        if(c == '-' && first) {
            index++;
            return new CharLiteral('-');
        }

        // ']' as the first element is a literal, not the closing bracket.
        if(c == ']' && first) {
            index++;
            return new CharLiteral(']');
        }

        // Range detection: requires three characters "x-y" where y != ']'.
        if(IsValidRange(pattern, index)) {
            var range = new CharRange(c, pattern[index + 2]);
            index += 3;
            return range;
        }

        // Simple literal.
        index++;
        return new CharLiteral(c);
    }

    /// <summary>
    /// Returns <see langword="true"/> when positions <c>[index]</c>, <c>[index+1]</c>,
    /// and <c>[index+2]</c> form a syntactically valid range expression <c>x-y</c>
    /// where <c>y</c> is not <c>']'</c>.
    /// </summary>
    /// <remarks>
    /// Inverted ranges (<c>z-a</c>) are syntactically valid here and are detected
    /// later by <see cref="CharacterClassRangeInvariant"/>.
    /// </remarks>
    private static bool IsValidRange(ReadOnlySpan<char> pattern, int index) {
        // Need at least three characters: start, '-', end.
        if(index + 2 >= pattern.Length) {
            return false;
        }

        if(pattern[index + 1] != '-') {
            return false;
        }

        // "a-]" is not a range; '-' before the closing bracket is a literal.
        return pattern[index + 2] != ']';
    }

    /// <summary>
    /// Returns <see langword="true"/> when the position starts a POSIX class
    /// prefix <c>[:</c>.
    /// </summary>
    private static bool IsPosixStart(ReadOnlySpan<char> pattern, int index) =>
        index + 1 < pattern.Length &&
        pattern[index] == '[' &&
        pattern[index + 1] == ':';

    /// <summary>
    /// Parses a POSIX named class <c>[: name :]</c>.
    /// </summary>
    /// <remarks>
    /// On success advances <paramref name="index"/> past the closing <c>:]</c>.
    /// On failure returns a <see cref="CharacterClassParseError"/> sentinel.
    /// </remarks>
    private static ICharacterClassElement ParsePosix(
        ReadOnlySpan<char> pattern,
        ref int index
    ) {
        index += 2; // skip "[:"

        var nameStart = index;

        // Scan until ':' or end of input.
        while(index < pattern.Length && pattern[index] != ':') {
            index++;
        }

        if(index >= pattern.Length) {
            return new CharacterClassParseError(
                "Unterminated POSIX class: missing closing ':'.");
        }

        var name = pattern[nameStart..index].ToString();
        index++; // consume ':'

        if(index >= pattern.Length || pattern[index] != ']') {
            return new CharacterClassParseError(
                $"Invalid POSIX class syntax for ':{name}': expected ']' after ':'.");
        }

        index++; // consume ']'
        return new PosixClass(name);
    }
}
