namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Maps a <see cref="PatternKind"/> to its corresponding compiler.
/// </summary>
internal interface IPatternCompilerRegistry
{
    IPatternCompiler GetCompiler(PatternKind type);
}
