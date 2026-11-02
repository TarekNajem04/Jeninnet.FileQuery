//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Git;

/// <summary>
/// The set of files that changed between a Git baseline and the working tree.
/// </summary>
/// <param name="RepositoryRoot">The absolute repository root reported by Git.</param>
/// <param name="RootPrefix">The forward-slash-terminated path from the repository root to the queried directory; empty at the repository root.</param>
/// <param name="Changes">The individual changes, including deletions.</param>
internal sealed record GitChangeSet(
    string RepositoryRoot,
    string RootPrefix,
    IReadOnlyList<GitChange> Changes
) {
    /// <summary>
    /// Gets the repository-relative paths that still exist after the change set
    /// (added, modified, renamed, copied, and type-changed files).
    /// </summary>
    public IReadOnlyList<string> ExistingPaths { get; } = [.. Changes
        .Where(static change => change.Kind is not GitChangeKind.Deleted)
        .Select(static change => change.Path)
        .Distinct(StringComparer.Ordinal)];
}
