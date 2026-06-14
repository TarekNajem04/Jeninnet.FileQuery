namespace Jeninnet.FileQuery;

/// <summary>
/// Represents a point-in-time progress snapshot reported during file query traversal.
/// </summary>
/// <param name="DirectoriesVisited">The number of directories opened for enumeration.</param>
/// <param name="EntriesScanned">The number of filesystem entries inspected.</param>
/// <param name="FilesMatched">The number of files yielded by the query.</param>
/// <param name="CurrentPath">The most recent filesystem path inspected.</param>
public readonly record struct FileQueryProgress(
    long DirectoriesVisited,
    long EntriesScanned,
    long FilesMatched,
    string CurrentPath
);
