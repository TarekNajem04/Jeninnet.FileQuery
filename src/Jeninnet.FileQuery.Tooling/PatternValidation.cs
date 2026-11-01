//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// The result of structural validation for a raw pattern.
/// </summary>
/// <param name="IsValid">Whether the pattern passed structural validation.</param>
/// <param name="Error">The validation message when <paramref name="IsValid"/> is <see langword="false"/>.</param>
/// <param name="Position">The zero-based character position of the error, when known.</param>
/// <param name="Segment">The zero-based segment index of the error, when known.</param>
public sealed record PatternValidation(
    bool IsValid,
    string? Error,
    int? Position = null,
    int? Segment = null
);
