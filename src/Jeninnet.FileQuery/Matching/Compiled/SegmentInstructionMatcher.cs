namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// Provides static methods for matching file path segments against tokenized
/// pattern segments, supporting wildcards, character classes (including POSIX
/// named classes), and custom comparison options.
/// </summary>
/// <remarks>
/// All methods are static and thread-safe.
/// </remarks>
internal static class SegmentInstructionMatcher {
    /// <summary>
    /// Matches a single path segment (e.g., <c>"helpers.cs"</c>) against a
    /// tokenized pattern segment.
    /// </summary>
    /// <param name="segment">The path segment to match.</param>
    /// <param name="tokens">The tokenized pattern segment.</param>
    /// <param name="cmp">The string comparison mode.</param>
    /// <returns>
    /// <see langword="true"/> if the entire segment matches the token sequence;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool MatchSegment(
        ReadOnlySpan<char> segment,
        IReadOnlyList<IPatternToken> tokens,
        StringComparison cmp
    ) {
        var result = MatchSequence(tokens, 0, segment, 0, cmp);
        return result == segment.Length;
    }

    /// <summary>
    /// Matches a sequence of pattern tokens against a path segment slice.
    /// </summary>
    /// <param name="tokens">The list of tokens to match.</param>
    /// <param name="tokenIndex">The index of the current token.</param>
    /// <param name="text">The text to match against.</param>
    /// <param name="pos">The current position in the text.</param>
    /// <param name="cmp">The string comparison mode.</param>
    /// <returns>
    /// The position in <paramref name="text"/> where the match stopped,
    /// or <see langword="null"/> if no match was found.
    /// </returns>
    private static int? MatchSequence(
        IReadOnlyList<IPatternToken> tokens,
        int tokenIndex,
        ReadOnlySpan<char> text,
        int pos,
        StringComparison cmp
    ) {
        if(IsPatternComplete(tokens, tokenIndex)) {
            return MatchIfTextComplete(text, pos);
        }

        var token = tokens[tokenIndex];

        if(IsTextExhausted(text, pos)) {
            return TryMatchTrailingWildcard(tokens, tokenIndex, token, text, pos, cmp);
        }

        return token switch {
            WildcardToken => HandleWildcard(tokens, tokenIndex, text, pos, cmp),
            LiteralToken literal => MatchLiteral(tokens, tokenIndex, literal, text, pos, cmp),
            SingleCharToken => MatchSingleCharacter(tokens, tokenIndex, text, pos, cmp),
            CharacterClassToken cls => MatchCharacterClass(tokens, tokenIndex, cls, text, pos, cmp),
            _ => null,
        };
    }

    private static bool IsPatternComplete(IReadOnlyList<IPatternToken> tokens, int index) => index == tokens.Count;

    private static bool IsTextExhausted(ReadOnlySpan<char> text, int pos) => pos == text.Length;

    private static int? MatchIfTextComplete(ReadOnlySpan<char> text, int pos) => pos == text.Length ? pos : null;

    private static int? TryMatchTrailingWildcard(
        IReadOnlyList<IPatternToken> tokens,
        int tokenIndex,
        IPatternToken token,
        ReadOnlySpan<char> text,
        int pos,
        StringComparison cmp
    ) =>
        token is WildcardToken
            ? HandleWildcard(tokens, tokenIndex, text, pos, cmp)
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSegmentSeparator(char c) => c == '/' || c == Path.DirectorySeparatorChar;

    private static int? MatchLiteral(
        IReadOnlyList<IPatternToken> tokens,
        int tokenIndex,
        LiteralToken literal,
        ReadOnlySpan<char> text,
        int pos,
        StringComparison cmp
    ) {
        if(pos + literal.Text.Length > text.Length ||
            !text[pos..].StartsWith(literal.Text.AsSpan(), cmp)) {
            return null;
        }

        return MatchSequence(tokens, tokenIndex + 1, text, pos + literal.Text.Length, cmp);
    }

    private static int? MatchSingleCharacter(
        IReadOnlyList<IPatternToken> tokens,
        int tokenIndex,
        ReadOnlySpan<char> text,
        int pos,
        StringComparison cmp
    ) => MatchSequence(tokens, tokenIndex + 1, text, pos + 1, cmp);

    /// <summary>
    /// Matches one character from <paramref name="text"/> against the compiled
    /// <see cref="CharacterClassToken"/>.
    /// </summary>
    /// <param name="tokens">The list of tokens to match.</param>
    /// <param name="tokenIndex">The index of the current token.</param>
    /// <param name="cls">The character class to match against.</param>
    /// <param name="text">The text to match against.</param>
    /// <param name="pos">The current position in the text.</param>
    /// <param name="cmp">The string comparison mode.</param>
    private static int? MatchCharacterClass(
        IReadOnlyList<IPatternToken> tokens,
        int tokenIndex,
        CharacterClassToken cls,
        ReadOnlySpan<char> text,
        int pos,
        StringComparison cmp
    ) {
        var c = text[pos];

        if(IsSegmentSeparator(c)) {
            return null;
        }

        if(!CharacterClassMatches(cls.Value, c)) {
            return null;
        }

        return MatchSequence(tokens, tokenIndex + 1, text, pos + 1, cmp);
    }

    /// <summary>
    /// Returns <see langword="true"/> when character <paramref name="c"/>
    /// satisfies the <see cref="CharacterClass"/> definition.
    /// </summary>
    /// <param name="cls">The character class definition.</param>
    /// <param name="c">The character to test.</param>
    /// <remarks>
    /// <para>
    /// <strong>Allocation fix:</strong> The previous implementation used
    /// <c>cls.Elements.Any(element => MatchesElement(element, c))</c>.
    /// The lambda captures the local variable <paramref name="c"/>
    /// (a <c>char</c>), which causes the C# compiler to hoist <paramref name="c"/>
    /// into a heap-allocated display class (closure object) on every call.
    /// Replaced with a manual <c>for</c> loop that calls
    /// <see cref="MatchesElement"/> directly — no closure, no LINQ overhead,
    /// and an early <c>break</c> as soon as a matching element is found.
    /// </para>
    /// </remarks>
    private static bool CharacterClassMatches(CharacterClass cls, char c) {
        var elements = cls.Elements;
        var inSet = false;

        // MANUAL LOOP — eliminates the display-class allocation that
        // Any(element => MatchesElement(element, c)) would create per call.
        for(var i = 0; i < elements.Count; i++) {
            if(MatchesElement(elements[i], c)) {
                inSet = true;
                break; // short-circuit: no need to evaluate remaining elements
            }
        }

        return cls.IsNegated ? !inSet : inSet;
    }

    /// <summary>
    /// Pattern-matches a single <see cref="ICharacterClassElement"/> against
    /// character <paramref name="c"/>.
    /// </summary>
    /// <param name="element">The character class element.</param>
    /// <param name="c">The character to test.</param>
    private static bool MatchesElement(ICharacterClassElement element, char c) =>
        element switch {
            CharLiteral literal => literal.Value == c,
            CharRange range => c >= range.Start && c <= range.End,
            PosixClass posix => MatchesPosixClass(posix.Name, c),
            CharacterClassParseError _ => false, // sentinel; never matches at runtime
            _ => false
        };

    /// <summary>
    /// Evaluates whether <paramref name="c"/> belongs to the POSIX named class
    /// <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the POSIX class.</param>
    /// <param name="c">The character to test.</param>
    /// <remarks>
    /// Unknown POSIX class names return <see langword="false"/> (safe default).
    /// </remarks>
    private static bool MatchesPosixClass(string name, char c) =>
        name switch {
            "digit" => char.IsDigit(c),
            "alpha" => char.IsLetter(c),
            "alnum" => char.IsLetterOrDigit(c),
            "space" => char.IsWhiteSpace(c),
            "blank" => c is ' ' or '\t',
            "upper" => char.IsUpper(c),
            "lower" => char.IsLower(c),
            "print" => !char.IsControl(c),
            "graph" => !char.IsControl(c) && c != ' ',
            "punct" => char.IsPunctuation(c) || char.IsSymbol(c),
            "cntrl" => char.IsControl(c),
            "xdigit" => Uri.IsHexDigit(c),
            _ => false
        };

    /// <summary>
    /// Handles the single wildcard <c>*</c> using canonical backtracking.
    /// </summary>
    /// <param name="tokens">The list of tokens to match.</param>
    /// <param name="wildcardIndex">The index of the wildcard token.</param>
    /// <param name="text">The text to match against.</param>
    /// <param name="pos">The current position in the text.</param>
    /// <param name="cmp">The string comparison mode.</param>
    private static int? HandleWildcard(
        IReadOnlyList<IPatternToken> tokens,
        int wildcardIndex,
        ReadOnlySpan<char> text,
        int pos,
        StringComparison cmp
    ) {
        if(wildcardIndex == tokens.Count - 1) {
            return text.Length; // trailing '*' consumes everything in the segment
        }

        for(var skip = pos; skip <= text.Length; skip++) {
            var next = MatchSequence(tokens, wildcardIndex + 1, text, skip, cmp);
            if(next.HasValue) {
                return next.Value;
            }
        }

        return null;
    }
}
