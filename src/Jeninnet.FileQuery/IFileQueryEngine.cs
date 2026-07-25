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
}
