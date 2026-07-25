// ============================================================
// RecursiveWildcardInSegmentInvariant.cs
// Location: src/Jeninnet.FileQuery/Patterns/Invariants/
//
// Fix: was inspecting context.Pattern.Text (raw string) for "***".
// A raw-string check cannot catch "**a" (two stars then a literal)
// because there are only two stars. The invariant must inspect the
// compiled token stream, not the source text.
// ============================================================

namespace Jeninnet.FileQuery.Patterns.Invariants;

// ============================================================
// RecursiveWildcardInSegmentInvariant.cs
// Location: src/Jeninnet.FileQuery/Patterns/Invariants/
//
// NEW invariant — catches "**a", "a**", "a**b" in any pattern kind.
// GlobPatternInvariant catches this for Glob already; this invariant
// fills the gap for GitIgnore patterns.
// ============================================================

/// <summary>
/// Ensures that when a <c>**</c> token appears in a segment it occupies
/// the segment alone and is not mixed with other tokens.
/// </summary>
/// <remarks>
/// <para>
/// A segment such as <c>**a</c> produces the token list
/// <c>[RecursiveWildcardToken, LiteralToken("a")]</c>. This is structurally
/// ambiguous: does the recursive wildcard still mean "zero or more directories"
/// when followed by a literal in the same segment? All major glob
/// implementations reject this form.
/// </para>
/// <para>
/// <see cref="GlobPatternInvariant"/> already enforces this rule for
/// <see cref="PatternKind.Glob"/> patterns. This invariant extends the same
/// guarantee to all pattern kinds (including <see cref="PatternKind.GitIgnore"/>).
/// </para>
/// </remarks>
internal sealed class RecursiveWildcardInSegmentInvariant : IPatternInvariant {
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => null; // all dialects

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context) {
        foreach(var segment in context.Tokens!) {
            var hasRecursive = segment.Any(static t => t is RecursiveWildcardToken);

            if(hasRecursive && segment.Count > 1) {
                return PatternInvariantResult.Fail(
                    "In glob and GitIgnore patterns, '**' must appear as a " +
                    "standalone segment. Mixed segments such as '**a' or 'a**' " +
                    "are not supported.");
            }
        }

        return PatternInvariantResult.Success;
    }
}
