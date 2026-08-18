//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Represents a single pattern invariant that can validate a pattern.
/// </summary>
internal interface IPatternInvariant {
    /// <summary>
    /// The execution phase of this invariant.
    /// </summary>
    PatternInvariantPhase Phase { get; }

    /// <summary>
    /// Optional filter — invariant applies only to specific pattern types.
    /// </summary>
    PatternKind? AppliesTo { get; }

    /// <summary>
    /// Validates the given pattern.
    /// </summary>
    /// <param name="context">The compilation context containing the pattern to validate.</param>
    PatternInvariantResult Validate(PatternCompilationContext context);
}
