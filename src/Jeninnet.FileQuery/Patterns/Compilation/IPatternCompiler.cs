namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Defines a compiler for a single <see cref="PatternKind"/>.
/// </summary>
internal interface IPatternCompiler
{
    PatternKind PatternKind { get; }
    ICompiledPattern Compile(PatternCompilationContext context);
}
