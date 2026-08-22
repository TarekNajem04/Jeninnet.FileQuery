//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Compiles patterns that follow flat Glob-style semantics by delegating parsing
/// and tokenization to the centralized <see cref="PatternScanner"/>.
/// </summary>
/// <remarks>
/// <para>
/// Glob-style patterns support wildcards (*, ?, [abc]), recursive wildcards (**),
/// and character classes. This compiler ignores GitIgnore-specific semantics such as
/// directory-only patterns, negation prefixes (!), or root anchoring (/).
/// </para>
/// <para>
/// All compiled patterns are immutable and returned as <see cref="CompiledPattern"/> instances.
/// This class enforces Step 9 guarantees: raw pattern strings never escape the compilation
/// pipeline, and all invariants are applied through <see cref="PatternCompilerBase"/>.
/// </para>
/// </remarks>
internal sealed class GlobPatternCompiler : PatternCompilerBase {
    public override PatternKind PatternKind => PatternKind.Glob;

    protected override ICompiledPattern CompileCore(PatternCompilationContext context) {
        var state = context.State;
        var tokens = context.Tokens!;

        return new CompiledPattern(new CompiledPatternConfig(
            IsNegated: state.IsNegated,
            DirectoryOnly: state.IsDirectoryOnly,
            AnchoredToRoot: state.IsRootAnchored,
            Segments: tokens,
            PatternKind: PatternKind.Glob,
            Intent: CompiledMatchIntent.FromNegation(state.IsNegated),
            ConcretePathAnchor: PatternAnchorResolver.Resolve(tokens),
            SourceText: context.Pattern.Text,
            SourceIndex: context.Pattern.SourceIndex
        ));
    }
}
