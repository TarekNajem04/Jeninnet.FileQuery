//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Git;

/// <summary>
/// Describes one file change reported by Git.
/// </summary>
/// <param name="Kind">The kind of change.</param>
/// <param name="Path">The repository-relative path after the change (forward slashes).</param>
/// <param name="PreviousPath">The previous path for a rename or copy; otherwise <see langword="null"/>.</param>
internal sealed record GitChange(
    GitChangeKind Kind,
    string Path,
    string? PreviousPath = null
);
