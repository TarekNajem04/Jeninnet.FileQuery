//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Represents the public outcome of evaluating a single path against a compiled pattern set.
/// </summary>
public enum FileQueryMatchOutcome {
    /// <summary>
    /// No pattern matched the path, so the default inclusion state applies.
    /// </summary>
    NoMatch = 0,

    /// <summary>
    /// The path is included by the compiled patterns.
    /// </summary>
    Include = 1,

    /// <summary>
    /// The path is excluded by the compiled patterns.
    /// </summary>
    Exclude = 2
}
