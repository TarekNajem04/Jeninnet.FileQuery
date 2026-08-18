//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Specifies how pattern matches are resolved when multiple patterns apply.
/// </summary>
internal enum MatchPrecedence {
    /// <summary>
    /// The first matching pattern determines the result, and later matches are ignored.
    /// </summary>
    FirstMatchWins = 0,

    /// <summary>
    /// The last matching pattern determines the result, overriding earlier matches.
    /// </summary>
    LastMatchWins = 1,

    /// <summary>
    /// Explicit exclusion rules take precedence over inclusion rules regardless of order.
    /// </summary>
    ExcludeWins = 2
}
