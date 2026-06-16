namespace Jeninnet.FileQuery.Patterns;

internal static class CompiledPatternSetFactory
{
    /// <summary>
    /// Creates a compiled pattern set from the given matching configuration.
    /// </summary>
    /// <param name="configuration">The matching configuration.</param>
    /// <returns>The compiled pattern set.</returns>
    public static ICompiledPatternSet Create(MatchingConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return CompiledPatternFactory.CompileSet(configuration);
    }
}
