namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Dependency-injectable, phase-driven pattern compilation pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline runs in three phases:
/// <list type="number">
///   <item>
///     <strong>Lexical</strong> — validates the raw pattern text before any
///     scanning occurs. Example: control characters, invalid regex syntax.
///   </item>
///   <item>
///     <strong>Structural</strong> — validates the token stream produced by
///     <see cref="PatternScanner"/>. Example: malformed character classes,
///     parent traversal (<c>..</c>), empty segments.
///   </item>
///   <item>
///     <strong>Semantic</strong> — applies dialect-specific transforms and
///     validates cross-token meaning. Example: inserting the implicit
///     <c>**</c> segment for unanchored GitIgnore patterns.
///   </item>
/// </list>
/// </para>
/// </remarks>
internal sealed class PatternPipeline
{
    private readonly PatternInvariantRegistry _invariants;
    private readonly IPatternCompilerRegistry _compilers;

    internal PatternPipeline(
        PatternInvariantRegistry invariants,
        IPatternCompilerRegistry compilers
    )
    {
        _invariants = invariants ?? throw new ArgumentNullException(nameof(invariants));
        _compilers = compilers ?? throw new ArgumentNullException(nameof(compilers));
    }

    // ------------------------------------------------------------------
    // Compile single pattern
    // ------------------------------------------------------------------

    /// <summary>
    /// Compiles a single classified pattern through the full pipeline.
    /// </summary>
    /// <param name="pattern">The classified pattern to compile.</param>
    public ICompiledPattern Compile(ClassifiedPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        var context = new PatternCompilationContext(pattern);
        var syntax = PatternSyntaxProfile.GetProfileForPatternType(pattern.Type);

        // Phase 1 — Lexical invariants (raw text, before scanning).
        _invariants.ValidateLexical(context);

        // Phase 2 — Scan (lex + structural parse).
        PatternScanner.Scan(context, syntax);

        // Phase 3 — Structural invariants (token stream).
        _invariants.ValidateStructural(context);

        // Phase 4 — Semantic invariants (dialect transforms + cross-token validation).
        _invariants.ValidateSemantic(context);

        // Phase 5 — Compile tokens to compiled pattern.
        var compilerResult = _compilers.GetCompiler(pattern.Type);
        if(!compilerResult.IsSuccess)
        {
            throw new PatternException(compilerResult.Error!);
        }

        return compilerResult.Value!.Compile(context);
    }

    // ------------------------------------------------------------------
    // Compile pattern set
    // ------------------------------------------------------------------

    /// <summary>
    /// Compiles all patterns in <paramref name="set"/> and returns an ordered
    /// compiled set.
    /// </summary>
    /// <param name="set">The classified pattern set.</param>
    public ICompiledPatternSet Compile(ClassifiedPatternSet set)
    {
        ArgumentNullException.ThrowIfNull(set);

        if(set.Patterns.Count == 0)
        {
            return CompiledPatternSet.Empty;
        }

        var compiled = new List<ICompiledPattern>(set.Patterns.Count);

        foreach(var pattern in set.Patterns)
        {
            compiled.Add(Compile(pattern));
        }

        return new CompiledPatternSet(compiled);
    }

    // ------------------------------------------------------------------
    // Default pipeline wiring
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the default pipeline with the standard invariant and compiler sets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Invariant changes from the previous wiring:</strong>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="GitIgnoreImplicitRecursiveInvariant"/>
    ///     replaces both, handling all non-root-anchored GitIgnore patterns in
    ///     one place at the correct (Semantic) phase.
    ///   </item>
    ///   <item>
    ///     <see cref="RecursiveWildcardIsolationInvariant"/> now
    ///     inspects tokens instead of the raw pattern text.
    ///   </item>
    ///   <item>
    ///     <see cref="RecursiveWildcardInSegmentInvariant"/> is new —
    ///     catches mixed segments such as <c>**a</c> for all pattern kinds.
    ///   </item>
    ///   <item>
    ///     <see cref="RegexSyntaxInvariant"/> now strips the
    ///     <c>r:</c> prefix before compiling the expression.
    ///   </item>
    ///   <item>
    ///     <see cref="GitIgnorePatternInvariant"/> now also
    ///     rejects a bare <c>"/"</c> (root-anchored with empty body).
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    public static PatternPipeline CreateDefault()
    {
        var invariants = new PatternInvariantRegistry([

            // ---- Lexical phase ----
            new LiteralNormalizationInvariant(),
            new RegexSyntaxInvariant(),             // fixed: strips r: prefix
            new EmptyPatternInvariant(),

            // ---- Structural phase ----
            new CharacterClassRangeInvariant(),
            new CharacterClassStructureInvariant(),
            new CurrentDirectoryInvariant(),
            new ParentTraversalInvariant(),
            new RecursiveWildcardInSegmentInvariant(), // new: catches **a, a**
            new RecursiveWildcardRedundancyInvariant(),

            // ---- Semantic phase ----
            // GitIgnoreImplicitRecursiveInvariant replaces both:
            //   - the removed PatternScanner.ApplyImplicitRecursiveWildcard
            //   - the removed GitIgnoreNegationImplicitRecursiveInvariant
            new GitIgnoreImplicitRecursiveInvariant(),
            new GitIgnorePatternInvariant(),
            new GlobPatternInvariant(),

            // RecursiveWildcardIsolationInvariant fixed: token-based, not text-based
            new RecursiveWildcardIsolationInvariant(),
        ]);

        var compilers = new PatternCompilerRegistry();

        return new PatternPipeline(invariants, compilers);
    }
}
