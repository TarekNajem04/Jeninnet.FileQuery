namespace Jeninnet.FileQuery.Patterns.Invariants.Dialects;

/// <summary>
/// Ensures that all non-root-anchored GitIgnore patterns begin with an
/// implicit recursive wildcard (<c>**</c>) segment.
/// </summary>
/// <remarks>
/// <para>
/// <strong>GitIgnore unanchored matching rule:</strong>
/// A pattern without a leading <c>/</c> must match at any depth in the
/// directory tree, not just at the root. This is implemented by prepending
/// a <c>**</c> segment so that the compiled matcher slides the pattern across
/// every depth level during traversal.
/// </para>
/// <para>
/// <strong>Why this lives in the invariant phase, not the scanner:</strong>
/// Inserting a synthetic <c>**</c> segment is a semantic transformation that
/// depends on dialect knowledge (<em>"GitIgnore unanchored patterns are implicitly
/// recursive"</em>). It is not a lexical operation. The scanner's sole
/// responsibility is tokenizing the raw pattern text; it must not apply
/// dialect-specific rewrites.
/// </para>
/// <para>
/// <strong>History:</strong>
/// This invariant replaces two previously split implementations:
/// <list type="bullet">
///   <item>
///     <c>PatternScanner.ApplyImplicitRecursiveWildcard</c> — handled the
///     general case but lived in the scanner (wrong layer).
///   </item>
///   <item>
///     <c>GitIgnoreNegationImplicitRecursiveInvariant</c> — handled only the
///     negated case and was therefore incomplete; it is now superseded by this
///     invariant.
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Conditions under which <c>**</c> is prepended:</strong>
/// <list type="bullet">
///   <item>Pattern kind is <see cref="PatternKind.GitIgnore"/>.</item>
///   <item>The pattern is <em>not</em> root-anchored (no leading <c>/</c>).</item>
///   <item>The first segment is not already <c>**</c>.</item>
///   <item>The token list is not empty (a zero-segment pattern is already
///         caught by <see cref="GitIgnorePatternInvariant"/>).</item>
/// </list>
/// </para>
/// </remarks>
internal sealed class GitIgnoreImplicitRecursiveInvariant : IPatternInvariant {
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Semantic;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => PatternKind.GitIgnore;

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context) {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Tokens);

        // Root-anchored patterns match only from the root — no implicit ** needed.
        if(context.State.IsRootAnchored) {
            return PatternInvariantResult.Success;
        }

        // Nothing to prepend to an empty token list.
        // (GitIgnorePatternInvariant will report the empty-segment error separately.)
        if(context.Tokens.Count == 0) {
            return PatternInvariantResult.Success;
        }

        // If the first segment is already a standalone **, the pattern is already
        // recursive — do not add a duplicate.
        if(IsRecursiveWildcardSegment(context.Tokens[0])) {
            return PatternInvariantResult.Success;
        }

        // Prepend the implicit ** segment.
        context.Tokens.Insert(0, [new RecursiveWildcardToken()]);

        return PatternInvariantResult.Success;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="segment"/> consists of
    /// exactly one <see cref="RecursiveWildcardToken"/>.
    /// </summary>
    private static bool IsRecursiveWildcardSegment(List<IPatternToken> segment) =>
        segment is [RecursiveWildcardToken];
}
