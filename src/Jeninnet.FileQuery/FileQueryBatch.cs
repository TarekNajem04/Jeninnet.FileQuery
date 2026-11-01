//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Executes several built queries as one ordered batch.
/// </summary>
/// <remarks>
/// <para>
/// Queries are executed in the order supplied. Results from one root are emitted completely
/// before the next root starts, so per-root ordering is deterministic and the same as a
/// standalone query. Absolute paths are unambiguous; entry results additionally expose
/// <see cref="FileQueryEntry.RootPath"/>.
/// </para>
/// <para>
/// Each query keeps its own options, including error recovery, so a failure in one root follows
/// that query's configured policy and does not change the policies of the other roots.
/// </para>
/// </remarks>
public sealed class FileQueryBatch {
    private readonly IReadOnlyList<FileQuery> _queries;

    /// <summary>
    /// Initializes a batch from already built queries.
    /// </summary>
    /// <param name="queries">The queries in execution order.</param>
    private FileQueryBatch(IReadOnlyList<FileQuery> queries) => _queries = queries;

    /// <summary>
    /// Gets the root directory of each query in execution order.
    /// </summary>
    public IReadOnlyList<string> Roots => [.. _queries.Select(static query => query.RootPath)];

    /// <summary>
    /// Builds and groups several fluent queries into one batch.
    /// </summary>
    /// <param name="builders">The builders to build, in execution order.</param>
    /// <returns>The batch.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builders"/> is <see langword="null"/>.</exception>
    /// <remarks>Each builder is built immediately, so later mutations do not affect the batch.</remarks>
    public static FileQueryBatch From(params FileQueryBuilder[] builders) {
        ArgumentNullException.ThrowIfNull(builders);
        return Create(builders.Select(static builder => {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Build();
        }));
    }

    /// <summary>
    /// Creates a batch from already built queries.
    /// </summary>
    /// <param name="queries">The queries in execution order.</param>
    /// <returns>The batch.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="queries"/> or one of its items is <see langword="null"/>.</exception>
    public static FileQueryBatch Create(IEnumerable<FileQuery> queries) {
        ArgumentNullException.ThrowIfNull(queries);

        var materialized = queries.ToArray();

        foreach(var query in materialized) {
            ArgumentNullException.ThrowIfNull(query);
        }

        return new FileQueryBatch(materialized);
    }

    /// <summary>
    /// Executes every query in order and yields absolute paths.
    /// </summary>
    /// <param name="progress">Optional progress sink; counters restart for each root.</param>
    /// <param name="cancellationToken">A token to cancel the batch.</param>
    /// <returns>A lazy enumerable of matching absolute paths.</returns>
    public IEnumerable<string> Execute(
        IProgress<FileQueryProgress>? progress = null,
        CancellationToken cancellationToken = default
    ) {
        var engine = FileQueryRuntime.Create();

        foreach(var query in _queries) {
            cancellationToken.ThrowIfCancellationRequested();

            foreach(var path in engine.Execute(query, progress, cancellationToken)) {
                yield return path;
            }
        }
    }

    /// <summary>
    /// Executes every query in order and yields entries with metadata and root information.
    /// </summary>
    /// <param name="progress">Optional progress sink; counters restart for each root.</param>
    /// <param name="cancellationToken">A token to cancel the batch.</param>
    /// <returns>A lazy enumerable of matching entries.</returns>
    public IEnumerable<FileQueryEntry> ExecuteEntries(
        IProgress<FileQueryProgress>? progress = null,
        CancellationToken cancellationToken = default
    ) {
        var engine = FileQueryRuntime.Create();

        foreach(var query in _queries) {
            cancellationToken.ThrowIfCancellationRequested();

            foreach(var entry in engine.ExecuteEntries(query, progress, cancellationToken)) {
                yield return entry;
            }
        }
    }

    /// <summary>
    /// Executes every query in order asynchronously and yields absolute paths.
    /// </summary>
    /// <param name="progress">Optional progress sink; counters restart for each root.</param>
    /// <param name="cancellationToken">A token to cancel the batch.</param>
    /// <returns>An async enumerable of matching absolute paths.</returns>
    public async IAsyncEnumerable<string> ExecuteAsync(
        IProgress<FileQueryProgress>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        var engine = FileQueryRuntime.Create();

        foreach(var query in _queries) {
            cancellationToken.ThrowIfCancellationRequested();

            await foreach(var path in engine.ExecuteAsync(query, progress, cancellationToken).ConfigureAwait(false)) {
                yield return path;
            }
        }
    }

    /// <summary>
    /// Executes every query in order asynchronously and yields entries with metadata.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the batch.</param>
    /// <returns>An async enumerable of matching entries.</returns>
    public async IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        var engine = FileQueryRuntime.Create();

        foreach(var query in _queries) {
            cancellationToken.ThrowIfCancellationRequested();

            await foreach(var entry in engine.ExecuteEntriesAsync(query, cancellationToken).ConfigureAwait(false)) {
                yield return entry;
            }
        }
    }
}
