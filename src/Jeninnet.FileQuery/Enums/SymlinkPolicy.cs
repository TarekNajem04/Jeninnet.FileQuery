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
