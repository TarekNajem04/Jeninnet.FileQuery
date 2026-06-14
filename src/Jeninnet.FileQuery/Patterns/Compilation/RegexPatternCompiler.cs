namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Compiles patterns that follow Flat Glob-style semantics by delegating all parsing
/// to the centralized <see cref="PatternScanner"/>.
/// </summary>
internal sealed class RegexPatternCompiler : PatternCompilerBase
{
    public override PatternKind PatternKind => PatternKind.Regex;

    protected override ICompiledPattern CompileCore(PatternCompilationContext context)
    {
        var state = context.State!;
        var tokens = context.Tokens!;

        return new CompiledPattern(
            isNegated: state.IsNegated,
            directoryOnly: state.IsDirectoryOnly,
            anchoredToRoot: state.IsRootAnchored,
            segments: tokens,
            patternKind: PatternKind.Regex,
            CompiledMatchIntent.FromNegation(state.IsNegated),
            context.Pattern.Text,
            context.Pattern.SourceIndex
        );
    }
}
