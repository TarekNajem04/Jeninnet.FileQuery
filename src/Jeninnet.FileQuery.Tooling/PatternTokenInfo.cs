//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// Describes one atomic token of a pattern.
/// </summary>
/// <param name="Kind">The token kind.</param>
/// <param name="Value">
/// The token text. Literals and escapes carry their source text; wildcards carry their
/// symbol; regular expressions carry the expression without the <c>r:</c> prefix; character
/// classes are rendered back from the parsed elements.
/// </param>
public sealed record PatternTokenInfo(PatternTokenKind Kind, string Value);
