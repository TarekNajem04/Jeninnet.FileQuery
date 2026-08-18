//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Base class for all pattern compilers.
/// </summary>
internal abstract class PatternCompilerBase : IPatternCompiler {
    public abstract PatternKind PatternKind { get; }

    public ICompiledPattern Compile(PatternCompilationContext context) {
        ArgumentNullException.ThrowIfNull(context);

        if(context.Pattern.Type != PatternKind) {
            throw new PatternException(
                $"Compiler {GetType().Name} cannot compile {context.Pattern.Type}");
        }

        return CompileCore(context);
    }

    protected abstract ICompiledPattern CompileCore(PatternCompilationContext context);
}
