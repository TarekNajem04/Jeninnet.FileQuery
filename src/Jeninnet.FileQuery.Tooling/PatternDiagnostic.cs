//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// Describes a pattern validation diagnostic for SARIF export.
/// </summary>
/// <param name="Pattern">The source pattern text.</param>
/// <param name="Message">The actionable validation message.</param>
/// <param name="Position">The zero-based character position, when known.</param>
/// <param name="Segment">The zero-based segment index, when known.</param>
/// <param name="FilePath">The source file that declared the pattern, when known.</param>
/// <param name="Line">The one-based source line, when known.</param>
/// <param name="Column">The one-based source column, when known.</param>
public sealed record PatternDiagnostic(
    string Pattern,
    string Message,
    int? Position = null,
    int? Segment = null,
    string? FilePath = null,
    int? Line = null,
    int? Column = null
);
