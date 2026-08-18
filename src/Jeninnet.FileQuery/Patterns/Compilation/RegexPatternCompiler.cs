//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Compiles patterns that follow Flat Glob-style semantics by delegating all parsing
/// to the centralized <see cref="PatternScanner"/>.
/// </summary>
internal sealed class RegexPatternCompiler : PatternCompilerBase {
    public override PatternKind PatternKind => PatternKind.Regex;

    protected override ICompiledPattern CompileCore(PatternCompilationContext context) {
        var state = context.State;
        var tokens = context.Tokens!;

        var regexText = tokens[0][0] is RegularExpressionToken ret ? ret.Pattern : null;

        return new CompiledPattern(new CompiledPatternConfig(
            IsNegated: state.IsNegated,
            DirectoryOnly: state.IsDirectoryOnly,
            AnchoredToRoot: state.IsRootAnchored,
            Segments: tokens,
            PatternKind: PatternKind.Regex,
            Intent: CompiledMatchIntent.FromNegation(state.IsNegated),
            ConcretePathAnchor: PatternAnchorResolver.Resolve(tokens),
            SourceText: context.Pattern.Text,
            SourceIndex: context.Pattern.SourceIndex,
            RegexText: regexText
        ));
    }
}
