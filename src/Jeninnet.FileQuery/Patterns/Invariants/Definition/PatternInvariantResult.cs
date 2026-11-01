//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Result of an invariant validation.
/// </summary>
/// <param name="IsSuccess">Whether the validation succeeded.</param>
/// <param name="Message">Optional error message if validation failed.</param>
/// <param name="Position">Optional zero-based character position of the error.</param>
/// <param name="Segment">Optional zero-based segment index of the error.</param>
internal readonly record struct PatternInvariantResult(
    bool IsSuccess,
    string? Message,
    int? Position = null,
    int? Segment = null
) {
    public static PatternInvariantResult Success => new(true, null);

    public static PatternInvariantResult Fail(string message, int? position = null, int? segment = null) =>
        new(false, message, position, segment);
}
