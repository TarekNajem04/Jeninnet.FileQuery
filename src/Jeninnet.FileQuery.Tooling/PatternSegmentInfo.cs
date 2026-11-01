//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// Represents one slash-separated segment of a tokenized pattern.
/// </summary>
/// <param name="Tokens">The ordered tokens that make up the segment.</param>
public sealed record PatternSegmentInfo(IReadOnlyList<PatternTokenInfo> Tokens);
