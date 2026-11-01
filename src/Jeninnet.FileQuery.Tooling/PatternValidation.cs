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
public sealed record PatternValidation(bool IsValid, string? Error);
