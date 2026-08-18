//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns;

internal static class CompiledPatternSetFactory {
    /// <summary>
    /// Creates a compiled pattern set from the given matching configuration.
    /// </summary>
    /// <param name="configuration">The matching configuration.</param>
    /// <returns>The compiled pattern set.</returns>
    public static ICompiledPatternSet Create(MatchingConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);
        return CompiledPatternFactory.CompileSet(configuration);
    }
}
