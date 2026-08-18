//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Defines a compiler for a single <see cref="PatternKind"/>.
/// </summary>
internal interface IPatternCompiler {
    PatternKind PatternKind { get; }
    ICompiledPattern Compile(PatternCompilationContext context);
}
