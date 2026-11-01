//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Explains why a path was included or excluded by a compiled pattern set.
/// </summary>
/// <param name="Path">The normalized path that was evaluated.</param>
/// <param name="Outcome">The final outcome produced by the pattern set.</param>
/// <param name="IsMatch">A convenience value indicating whether <paramref name="Outcome"/> is <see cref="Enums.FileQueryMatchOutcome.Include"/>.</param>
/// <param name="Reason">A human-readable explanation of the outcome.</param>
/// <param name="Pattern">The source pattern that decided the outcome, when one exists.</param>
/// <param name="PatternKind">The dialect of the deciding pattern, when one exists.</param>
/// <param name="PatternIndex">The zero-based source index of the deciding pattern, when known.</param>
public sealed record FileQueryPatternExplanation(
    string Path,
    FileQueryMatchOutcome Outcome,
    bool IsMatch,
    string Reason,
    string? Pattern,
    PatternKind? PatternKind,
    int? PatternIndex
);
