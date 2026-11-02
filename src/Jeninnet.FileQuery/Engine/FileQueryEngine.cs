//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Diagnostics;
using Jeninnet.FileQuery.Telemetry;

namespace Jeninnet.FileQuery.Engine;

/// <summary>
/// Collects files from a directory tree using a pattern-based matcher (GitIgnore/Glob/Flat hybrid).
/// </summary>
/// <param name="traversal">The traversal executor.</param>
/// <param name="planBuilder">The traversal plan builder.</param>
internal sealed class FileQueryEngine(
    ITraversalExecutor traversal,
    ITraversalPlanBuilder planBuilder
) : IFileQueryEngine {
    public IEnumerable<string> Execute(FileQuery query) => Execute(query, progress: null, CancellationToken.None);

    public IEnumerable<string> Execute(FileQuery query, CancellationToken cancellationToken) =>
        Execute(query, progress: null, cancellationToken);

    public IEnumerable<string> Execute(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default
    ) {
        var plan = planBuilder.Build(query, progress);
        return EnumeratePlan(query.Options, traversal.Execute(plan, cancellationToken).GetEnumerator());
    }

    public async IAsyncEnumerable<string> ExecuteAsync(FileQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        await foreach(var result in ExecuteAsync(query, progress: null, cancellationToken)) {
            yield return result;
        }
    }

    public async IAsyncEnumerable<string> ExecuteAsync(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        var plan = planBuilder.Build(query, progress);
        var enumerator = traversal.ExecuteAsync(plan, cancellationToken).GetAsyncEnumerator(cancellationToken);

        await foreach(var result in EnumeratePlanAsync(
            query.Options,
            enumerator,
            FileQueryTelemetry.ExecuteAsyncActivityName).ConfigureAwait(false)) {
            yield return result;
        }
    }

    public IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query) => ExecuteEntries(query, progress: null, CancellationToken.None);

    public IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query, CancellationToken cancellationToken) =>
        ExecuteEntries(query, progress: null, cancellationToken);

    public IEnumerable<FileQueryEntry> ExecuteEntries(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default
    ) {
        var plan = planBuilder.Build(query, progress);
        return EnumeratePlan(query.Options, traversal.ExecuteEntries(plan, cancellationToken).GetEnumerator());
    }

    public async IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
        FileQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        var plan = planBuilder.Build(query);
        var enumerator = traversal.ExecuteEntriesAsync(plan, cancellationToken).GetAsyncEnumerator(cancellationToken);

        await foreach(var result in EnumeratePlanAsync(
            query.Options,
            enumerator,
            FileQueryTelemetry.ExecuteAsyncActivityName).ConfigureAwait(false)) {
            yield return result;
        }
    }

    /// <summary>
    /// Enumerates a prepared synchronous plan while emitting telemetry.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="options">The query options used for telemetry tags.</param>
    /// <param name="enumerator">The traversal enumerator.</param>
    private static IEnumerable<T> EnumeratePlan<T>(
        FileQueryOptions options,
        IEnumerator<T> enumerator
    ) {
        var startTimestamp = FileQueryInstrumentation.RecordStarted();
        using var activity = FileQueryInstrumentation.StartActivity(FileQueryTelemetry.ExecuteActivityName, options);
        using var ownedEnumerator = enumerator;

        while(true) {
            T current;

            try {
                if(!ownedEnumerator.MoveNext()) {
                    CompletePlan(activity, startTimestamp);
                    yield break;
                }

                current = ownedEnumerator.Current;
            }
            catch(Exception exception) {
                FailPlan(activity, exception, startTimestamp);
                throw;
            }

            yield return current;
        }
    }

    /// <summary>
    /// Enumerates a prepared asynchronous plan while emitting telemetry.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="options">The query options used for telemetry tags.</param>
    /// <param name="enumerator">The traversal enumerator.</param>
    /// <param name="activityName">The activity name for the operation.</param>
    private static async IAsyncEnumerable<T> EnumeratePlanAsync<T>(
        FileQueryOptions options,
        IAsyncEnumerator<T> enumerator,
        string activityName
    ) {
        var startTimestamp = FileQueryInstrumentation.RecordStarted();
        using var activity = FileQueryInstrumentation.StartActivity(activityName, options);
        await using var ownedEnumerator = enumerator;

        while(true) {
            T current;

            try {
                if(!await ownedEnumerator.MoveNextAsync().ConfigureAwait(false)) {
                    CompletePlan(activity, startTimestamp);
                    yield break;
                }

                current = ownedEnumerator.Current;
            }
            catch(Exception exception) {
                FailPlan(activity, exception, startTimestamp);
                throw;
            }

            yield return current;
        }
    }

    /// <summary>
    /// Records a plan enumeration that reached its end.
    /// </summary>
    /// <param name="activity">The current activity, if any.</param>
    /// <param name="startTimestamp">The enumeration start timestamp.</param>
    private static void CompletePlan(Activity? activity, long startTimestamp) {
        activity?.SetTag("filequery.outcome", "completed");
        FileQueryInstrumentation.RecordCompleted(startTimestamp);
    }

    /// <summary>
    /// Records a plan enumeration that ended with an exception.
    /// </summary>
    /// <param name="activity">The current activity, if any.</param>
    /// <param name="exception">The failure that ended the enumeration.</param>
    /// <param name="startTimestamp">The enumeration start timestamp.</param>
    private static void FailPlan(Activity? activity, Exception exception, long startTimestamp) {
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

        if(exception is OperationCanceledException) {
            activity?.SetTag("filequery.outcome", "cancelled");
            FileQueryInstrumentation.RecordCancelled(startTimestamp);

            return;
        }

        activity?.SetTag("filequery.outcome", "error");
        FileQueryInstrumentation.RecordError(startTimestamp);
    }
}
