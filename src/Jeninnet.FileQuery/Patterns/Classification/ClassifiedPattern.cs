//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Classification;

/// <summary>
/// Represents a pattern with fully resolved semantics.
/// </summary>
/// <param name="Text">The pattern text.</param>
/// <param name="Type">The pattern kind.</param>
/// <param name="SourceIndex">The index of the pattern in the source.</param>
internal sealed record ClassifiedPattern(string Text, PatternKind Type, int SourceIndex = -1);
