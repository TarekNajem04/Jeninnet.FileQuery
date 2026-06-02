namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Ensures that <c>**</c> tokens are not redundantly repeated within the
/// same segment or in adjacent segments.
/// </summary>
/// <remarks>
/// <para>
/// <strong>What changed (v1.0 fix):</strong> The previous implementation
/// inspected the raw pattern text (<c>context.Pattern.Text</c>) for the
/// substring <c>"***"</c>. This approach had two problems:
/// <list type="bullet">
///   <item>
///     It missed mixed segments such as <c>**a</c>, which tokenize to
///     <c>[RecursiveWildcardToken, LiteralToken("a")]</c> — structurally
///     invalid but undetected by a raw three-star search.
///   </item>
///   <item>
///     It duplicated token-level knowledge at the text level, making the
///     check fragile in the presence of escaping or whitespace.
///   </item>
/// </list>
/// This invariant now operates on <see cref="PatternCompilationContext.Tokens"/>
/// exclusively, consistent with the principle that invariants validate the
/// intermediate representation, not the source text.
/// </para>
/// <para>
/// Adjacent <c>**</c> segments (<c>**/**</c>) are caught by
/// <see cref="RecursiveWildcardRedundancyInvariant"/>.
/// Mixed segments (<c>**a</c>, <c>a**</c>) are caught by
/// <see cref="RecursiveWildcardInSegmentInvariant"/>.
/// Multiple <c>**</c> within one segment token list are caught here.
/// </para>
/// </remarks>
internal sealed class RecursiveWildcardIsolationInvariant : IPatternInvariant {
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Semantic;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => null; // applies to all pattern kinds

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context) {
        foreach(var segment in context.Tokens!) {
            var recursiveCount = segment.Count(static t => t is RecursiveWildcardToken);

            if(recursiveCount > 1) {
                return PatternInvariantResult.Fail(
                    "A single pattern segment must not contain more than one " +
                    "recursive wildcard ('**').");
            }
        }

        return PatternInvariantResult.Success;
    }
}
