//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Tokenization;

/// <summary>
/// Scans a raw pattern string and produces a tokenized representation
/// stored in <see cref="PatternCompilationContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Single responsibility:</strong> pure lexer and structural parser.
/// Semantic transformations (e.g., implicit <c>**</c> insertion for unanchored
/// GitIgnore patterns) belong exclusively to the invariant phase.
/// </para>
/// <para>
/// <strong>Performance contract:</strong>
/// <list type="bullet">
///   <item>No <see cref="ReadOnlySpan{T}"/> escapes this type.</item>
///   <item>Tokenizers are O(n) per segment.</item>
///   <item>The outer segment list is pre-sized using the count returned by
///         <see cref="SplitSegments"/> — no resizing occurs.</item>
///   <item>Inner per-segment token lists use a small initial capacity
///         (<see cref="SEGMENT_INITIAL_TOKEN_CAPACITY"/>) to eliminate the first
///         internal array resize for the common 1–3 token case.</item>
/// </list>
/// </para>
/// </remarks>
internal static class PatternScanner {
    private static readonly IWholePatternTokenizer[] _wholePatternTokenizers = [
        new RegexPatternTokenizer()
    ];

    private static readonly IPatternTokenizer[] _tokenizers = [
        new EscapeTokenizer(),
        new RecursiveWildcardTokenizer(),
        new WildcardTokenizer(),
        new SingleCharWildcardTokenizer(),
        new CharacterClassTokenizer(),
        new LiteralTokenizer()        // fallback — always succeeds
    ];

    /// <summary>
    /// Most pattern segments contain 1–3 tokens (e.g., "*.cs" → [Wildcard, Literal(".cs")]).
    /// Initial capacity 3 avoids the first internal array resize for the vast
    /// majority of segments without over-allocating for single-token segments.
    /// </summary>
    private const int SEGMENT_INITIAL_TOKEN_CAPACITY = 3;

    /// <summary>
    /// Scans the pattern in <paramref name="context"/> and populates
    /// <see cref="PatternCompilationContext.State"/> and
    /// <see cref="PatternCompilationContext.Tokens"/>.
    /// </summary>
    /// <param name="context">The compilation context. Must not be <see langword="null"/>.</param>
    /// <param name="syntax">The syntax profile that controls which token types are recognized.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="PatternCompilationContext.Tokens"/> is already populated,
    /// indicating that <see cref="Scan"/> has been called more than once on the same context.
    /// </exception>
    public static void Scan(PatternCompilationContext context, PatternSyntaxProfile syntax) {
        ArgumentNullException.ThrowIfNull(context);

        if(context.Tokens is not null) {
            throw new InvalidOperationException(
                "PatternScanner.Scan must only be called once per compilation context.");
        }

        var span = context.Pattern.Text.AsSpan().Trim();

        // 1. Whole-pattern tokenizers (e.g., "r:" regex prefix).
        foreach(var wholeTokenizer in _wholePatternTokenizers) {
            if(wholeTokenizer.TryTokenize(span, syntax, out var tokens, out var state)) {
                context.State = state;
                context.Tokens = tokens;
                return;
            }
        }

        // 2. Structural analysis: identifies !, leading /, trailing /.
        var structure = AnalyzeStructure(span, syntax);

        // 3. Segment splitting: divides on '/' within the effective range.
        var segments = SplitSegments(span, structure);

        // 4. Per-segment tokenization.
        //
        // PRE-SIZED LOOP — the segment count is known from step 3, so the outer
        // list is allocated at exactly the right capacity. The previous LINQ
        // expression [.. segments.Select(seg => TokenizeSegment(...))] created an
        // intermediate IEnumerable<List<IPatternToken>> via the Select iterator,
        // then materialized it into an array through the collection expression —
        // allocating both the iterator object and the array. The direct loop
        // produces only the final List<List<IPatternToken>>.
        var tokenSegments = new List<List<IPatternToken>>(segments.Count);

        foreach(var (start, length) in segments) {
            tokenSegments.Add(TokenizeSegment(span, start, length, syntax));
        }

        context.State = structure;
        context.Tokens = tokenSegments;
    }

    private static PatternContext AnalyzeStructure(
        ReadOnlySpan<char> span,
        PatternSyntaxProfile syntax
    ) {
        var start = 0;
        var end = span.Length;

        // Escaped leading '!' or '#' — consume the backslash.
        if(span.Length > 1 && span[0] == '\\' && span[1] is '!' or '#') {
            start++;
        }

        var negated = syntax.SupportsNegation && start < end && span[start] == '!' && ++start > 0;
        var rooted = syntax.SupportsRootAnchoring && start < end && span[start] == '/' && ++start > 0;
        var dirOnly = syntax.SupportsDirectoryOnly && end > start && span[end - 1] == '/' && --end > 0;

        return new PatternContext(
            IsNegated: negated,
            IsRootAnchored: rooted,
            IsDirectoryOnly: dirOnly,
            Start: start,
            End: end
        );
    }

    private static List<(int Start, int Length)> SplitSegments(
        ReadOnlySpan<char> span,
        PatternContext context
    ) {
        var segments = new List<(int, int)>();
        var start = context.Start;

        for(var i = context.Start; i < context.End; i++) {
            if(span[i] == '/') {
                if(i > start) {
                    segments.Add((start, i - start));
                }

                start = i + 1;
            }
        }

        if(start < context.End) {
            segments.Add((start, context.End - start));
        }

        // Bare root anchor with no body (e.g., "/") produces a zero-length sentinel.
        // GitIgnorePatternInvariant rejects this case during the structural phase.
        if(context.IsRootAnchored && context.End == context.Start) {
            segments.Add((0, 0));
        }

        return segments;
    }

    private static List<IPatternToken> TokenizeSegment(
        ReadOnlySpan<char> pattern,
        int start,
        int length,
        PatternSyntaxProfile syntax
    ) {
        var tokens = new List<IPatternToken>(SEGMENT_INITIAL_TOKEN_CAPACITY);
        var segment = pattern.Slice(start, length);
        var index = 0;

        while(index < segment.Length) {
            foreach(var tokenizer in _tokenizers) {
                if(tokenizer.TryTokenize(segment, ref index, syntax, tokens)) {
                    break;
                }
            }
        }

        return tokens;
    }
}
