//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// An <see cref="IProgress{T}"/> implementation that invokes the callback synchronously.
/// </summary>
/// <remarks>
/// <see cref="Progress{T}"/> posts callbacks to a synchronization context or the thread pool,
/// which lets diagnostics arrive after the query has already completed. The tool writes
/// explanations in traversal order, so it needs an immediate sink.
/// </remarks>
/// <typeparam name="T">The reported value type.</typeparam>
/// <param name="report">The callback invoked for each reported value.</param>
internal sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T> {
    private readonly Action<T> _report = report ?? throw new ArgumentNullException(nameof(report));

    /// <inheritdoc/>
    public void Report(T value) => _report(value);
}
