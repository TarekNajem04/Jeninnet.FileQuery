//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Enums;

// https://en.wikipedia.org/wiki/NTFS_reparse_point
/// <summary>
/// Defines how symbolic links and reparse points are handled.
/// </summary>
public enum SymlinkPolicy : byte {
    /// <summary>
    /// Do not follow symbolic links.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Follow symbolic links.
    /// </summary>
    Follow = 1,

    /// <summary>
    /// Follow symbolic links, but prevent cycles.
    /// </summary>
    FollowWithCycleDetection = 2
}
