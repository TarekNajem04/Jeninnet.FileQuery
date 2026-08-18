//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Defines the compilation phase in which an invariant must run.
/// </summary>
internal enum PatternInvariantPhase {
    /// <summary>
    /// Raw text validation (escaping, forbidden sequences).
    /// </summary>
    Lexical = 0,

    /// <summary>
    /// Token/segment structure validation.
    /// </summary>
    Structural = 1,

    /// <summary>
    /// Semantic meaning validation (compiler-dependent).
    /// </summary>
    Semantic = 2
}
