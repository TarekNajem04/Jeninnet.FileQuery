namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Compiles patterns that follow GitIgnore-style semantics by delegating parsing
/// and tokenization to the centralized <see cref="PatternScanner"/>.
/// </summary>
/// <remarks>
/// <para>
/// GitIgnore-style patterns support recursive wildcards (**), single-character wildcards (?),
/// character classes ([abc]), escaping (\), as well as GitIgnore-specific semantics:
/// directory-only patterns (ending with '/'), negation (prefix '!'), and root anchoring ('/').
///</para>
/// <para>
/// All compiled patterns are immutable <see cref="CompiledPattern"/> instances,
/// and raw pattern strings never escape the compilation pipeline.
/// Invariants are enforced via <see cref="PatternCompilerBase"/>.
/// </para>
/// </remarks>
internal sealed class GitIgnorePatternCompiler : PatternCompilerBase
{
    public override PatternKind PatternKind => PatternKind.GitIgnore;

    protected override ICompiledPattern CompileCore(PatternCompilationContext context)
    {
        var state = context.State;
        var tokens = context.Tokens!;

        return new CompiledPattern(new CompiledPatternConfig(
            IsNegated: state.IsNegated,
            DirectoryOnly: state.IsDirectoryOnly,
            AnchoredToRoot: state.IsRootAnchored,
            Segments: tokens,
            PatternKind: PatternKind.GitIgnore,
            Intent: CompiledMatchIntent.FromNegation(state.IsNegated),
            ConcretePathAnchor: PatternAnchorResolver.Resolve(tokens),
            SourceText: context.Pattern.Text,
            SourceIndex: context.Pattern.SourceIndex
        ));
    }
}
