//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Maps a <see cref="PatternKind"/> to its corresponding compiler.
/// </summary>
internal interface IPatternCompilerRegistry {
    /// <summary>
    /// Retrieves the registered pattern compiler for the specified pattern kind.
    /// </summary>
    /// <param name="type">The pattern kind for which to retrieve a compiler.</param>
    /// <returns>The result containing the compiler or an error message.</returns>
    PatternResult<IPatternCompiler> GetCompiler(PatternKind type);
}
