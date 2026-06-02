namespace Jeninnet.FileQuery.Patterns.Compilation;

internal sealed class PatternCompilerRegistry : IPatternCompilerRegistry {
    private readonly Dictionary<PatternKind, IPatternCompiler> _registry;

    internal PatternCompilerRegistry() =>
        _registry = new Dictionary<PatternKind, IPatternCompiler> {
            [PatternKind.GitIgnore] = new GitIgnorePatternCompiler(),
            [PatternKind.Glob] = new GlobPatternCompiler(),
            [PatternKind.Regex] = new RegexPatternCompiler()
        };

    public IPatternCompiler GetCompiler(PatternKind type) {
        if(!_registry.TryGetValue(type, out var compiler)) {
            throw new PatternException($"No compiler registered for pattern type {type}.");
        }

        return compiler;
    }
}
