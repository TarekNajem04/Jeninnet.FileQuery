//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Collects files from a directory based on include/exclude patterns.
/// </summary>
public interface IFileQueryEngine {
    /// <summary>
    /// Enumerates files from <paramref name="query"/>.RootPath according to the provided <paramref name="query"/>.Options.
    /// Paths returned are absolute file-system paths (as returned by <see cref="Directory"/> APIs).
    /// </summary>
    /// <param name="query">The query descriptor defining the search parameters.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> of matching file paths.</returns>
    IEnumerable<string> Execute(FileQuery query);

    /// <summary>
    /// Asynchronous variant of <see cref="Execute(FileQuery)"/>.
    /// </summary>
    /// <param name="query">The query descriptor defining the search parameters.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An async enumerable of matching file paths.</returns>
    IAsyncEnumerable<string> ExecuteAsync(FileQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronous variant of <see cref="Execute(FileQuery)"/> with progress reporting.
    /// </summary>
    /// <param name="query">The query descriptor defining the search parameters.</param>
    /// <param name="progress">The progress sink that receives traversal snapshots.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An async enumerable of matching file paths.</returns>
    IAsyncEnumerable<string> ExecuteAsync(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates matching entries together with metadata captured from the filesystem enumeration.
    /// </summary>
    /// <param name="query">The query descriptor defining the search parameters.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of matching entries.</returns>
    /// <remarks>
    /// Metadata comes from the same entries used for matching, so this API does not perform
    /// additional file-system calls. Use <see cref="Execute(FileQuery)"/> when only paths are needed.
    /// </remarks>
    IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query);

    /// <summary>
    /// Asynchronous variant of <see cref="ExecuteEntries(FileQuery)"/>.
    /// </summary>
    /// <param name="query">The query descriptor defining the search parameters.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An async enumerable of matching entries.</returns>
    IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(FileQuery query, CancellationToken cancellationToken = default);
}
