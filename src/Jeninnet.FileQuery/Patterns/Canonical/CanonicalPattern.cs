//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents a normalized, explicit pattern.
/// </summary>
/// <param name="Text">The pattern text.</param>
/// <param name="ExplicitType">The optional explicit type of the pattern.</param>
public sealed record CanonicalPattern(string Text, PatternKind? ExplicitType);
