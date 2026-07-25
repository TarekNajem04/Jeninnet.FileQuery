namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <inheritdoc/>
internal sealed class PatternCompilerRegistry : IPatternCompilerRegistry {
    private readonly Dictionary<PatternKind, IPatternCompiler> _registry;

    internal PatternCompilerRegistry() =>
        _registry = new Dictionary<PatternKind, IPatternCompiler> {
            [PatternKind.GitIgnore] = new GitIgnorePatternCompiler(),
            [PatternKind.Glob] = new GlobPatternCompiler(),
            [PatternKind.Regex] = new RegexPatternCompiler()
        };

    /// <inheritdoc/>
    public PatternResult<IPatternCompiler> GetCompiler(PatternKind type) {
        if(!_registry.TryGetValue(type, out var compiler)) {
            return PatternResult<IPatternCompiler>.Fail($"No compiler registered for pattern type '{type}'. Ensure the pattern kind is valid and supported by the current runtime configuration.");
        }

        return PatternResult<IPatternCompiler>.Success(compiler);
    }
}
