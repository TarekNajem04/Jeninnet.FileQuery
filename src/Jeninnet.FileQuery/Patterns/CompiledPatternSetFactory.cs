namespace Jeninnet.FileQuery.Patterns;

internal static class CompiledPatternSetFactory {
    public static ICompiledPatternSet Create(MatchingConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);
        return CompiledPatternFactory.CompileSet(configuration);
    }
}
