//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Git;

/// <summary>
/// Classifies how a file changed relative to the Git baseline.
/// </summary>
internal enum GitChangeKind {
    /// <summary>The file is new.</summary>
    Added,

    /// <summary>The file content or metadata changed.</summary>
    Modified,

    /// <summary>The file was removed.</summary>
    Deleted,

    /// <summary>The file was renamed; the previous path is recorded separately.</summary>
    Renamed,

    /// <summary>The file was copied from another path.</summary>
    Copied,

    /// <summary>The file type changed (for example, a regular file became a symbolic link).</summary>
    TypeChanged,

    /// <summary>The status reported by Git is not recognized.</summary>
    Unknown
}
