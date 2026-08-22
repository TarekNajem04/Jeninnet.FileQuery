//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Defines how file‑system patterns are interpreted when selecting
/// files and directories during enumeration.
/// </summary>
/// <remarks>
/// The matching engine supports multiple interpretation modes, each
/// corresponding to a different pattern dialect.
///
/// <para><strong>GitIgnore</strong></para>
/// Behaves like a <c>.gitignore</c> file:
/// <list type="bullet">
/// <item><description>Directory‑aware matching</description></item>
/// <item><description>Anchored patterns (starting with <c>/</c>)</description></item>
/// <item><description>Directory‑only patterns (ending with <c>/</c>)</description></item>
/// <item><description>Recursive wildcard segments (<c>**</c>)</description></item>
/// <item><description>Patterns evaluated relative to a base directory</description></item>
/// <item><description>“Last rule wins” semantics</description></item>
/// </list>
///
/// <para><strong>Glob</strong></para>
/// Classic globbing rules:
/// <list type="bullet">
/// <item><description>Wildcard‑based matching (<c>*</c>, <c>?</c>)</description></item>
/// <item><description>Applied to the full normalized path</description></item>
/// <item><description>No hierarchical or directory‑aware semantics</description></item>
/// <item><description>All patterns operate on a single unified string</description></item>
/// </list>
///
/// <para><strong>Regex</strong></para>
/// <list type="bullet">
/// <item><description>Full .NET regular expression matching</description></item>
/// <item><description>Applied to the full normalized path</description></item>
/// </list>
///
/// <para>
/// All modes support negation (<c>!pattern</c>) unless explicitly disabled.
/// </para>
/// </remarks>
public enum PatternMatchingMode {
    /// <summary>
    /// GitIgnore‑compatible, directory‑aware interpretation.
    /// Recommended for tooling that needs hierarchical semantics.
    /// </summary>
    GitIgnore = 0,

    /// <summary>
    /// Classic glob syntax (<c>*</c>, <c>?</c>) applied to the full normalized path.
    /// </summary>
    Glob = 1,

    /// <summary>
    /// Regular expression matching using .NET <see cref="System.Text.RegularExpressions.Regex"/>.
    /// </summary>
    Regex = 2,
}
