//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
// Requires C# 14 — uses the 'extension' member syntax introduced in C# 14.
namespace Jeninnet.FileQuery.Extensions;

/// <summary>
/// Provides extension methods for <see cref="MatchOutcome"/> to simplify rule evaluation.
/// </summary>
internal static class MatchOutcomeExtensions {
    extension(MatchOutcome result) {
        /// <summary>
        /// Determines if the <see cref="MatchOutcome"/> represents a successful match that should be included.
        /// </summary>
        /// <returns><see langword="true"/> if matched and included; otherwise, <see langword="false"/>.</returns>
        public bool IsSuccess() => result is MatchOutcome.Include;

        /// <summary>
        /// Determines if the <see cref="MatchOutcome"/>represents a match that is explicitly included.
        /// </summary>
        /// <returns><see langword="true"/> if matched and included; otherwise, <see langword="false"/>.</returns>
        public bool IsIncluded() => result is MatchOutcome.Include;

        /// <summary>
        /// Determines if the <see cref="MatchOutcome"/> represents a match that is explicitly excluded.
        /// </summary>
        /// <returns><see langword="true"/> if matched but excluded; otherwise, <see langword="false"/>.</returns>
        public bool IsExcluded() => result is MatchOutcome.Exclude;

        /// <summary>
        /// Determines if the <see cref="MatchOutcome"/> did not match any rule.
        /// </summary>
        /// <returns><see langword="true"/> if unmatched; otherwise, <see langword="false"/>.</returns>
        public bool IsUnmatched() => result is MatchOutcome.NoMatch;
    }
}
