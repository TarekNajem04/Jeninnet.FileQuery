//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Controls which Git ignore files are loaded into a query.
/// </summary>
/// <remarks>
/// <para>
/// Ignore rules are appended before the query's own GitIgnore patterns, so explicit query
/// patterns keep the highest precedence. Deeper nested ignore files are appended after their
/// parents, matching Git's last-match-wins model.
/// </para>
/// <para>
/// Global excludes are optional and are treated as repository-root rules. The loader does not
/// read Git configuration; pass the resolved global exclude file explicitly.
/// </para>
/// </remarks>
public sealed record GitIgnoreOptions {
    /// <summary>
    /// Gets a value indicating whether <c>.gitignore</c> files in subdirectories are loaded.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeNestedFiles { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether <c>.git/info/exclude</c> is loaded when the query root is
    /// inside a Git working tree. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeInfoExclude { get; init; } = true;

    /// <summary>
    /// Gets the optional path of a global Git ignore file (equivalent to <c>core.excludesFile</c>).
    /// Relative paths are resolved against the query root.
    /// </summary>
    public string? GlobalExcludesFile { get; init; }
}
