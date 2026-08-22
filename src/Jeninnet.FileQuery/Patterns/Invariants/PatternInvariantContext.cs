//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

internal sealed record PatternInvariantContext {
    public string? Text { get; init; }
    public IReadOnlyList<IReadOnlyList<IPatternToken>>? Segments { get; init; }
    public ClassifiedPattern? Classified { get; init; }
    public ICompiledPattern? Compiled { get; init; }
}
