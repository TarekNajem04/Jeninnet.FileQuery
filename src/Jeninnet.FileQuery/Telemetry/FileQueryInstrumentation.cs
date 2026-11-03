//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Jeninnet.FileQuery.Telemetry;

/// <summary>
/// Centralizes the <see cref="ActivitySource"/> and <see cref="Meter"/> used by the engine.
/// </summary>
/// <remarks>
/// <para>
/// Instrument creation happens once per process. Activity objects and tags are only allocated
/// when a listener is attached; metric instruments are no-ops without listeners, so a query
/// performed without telemetry does not allocate instrumentation state on the hot path.
/// </para>
/// </remarks>
internal static class FileQueryInstrumentation {
    private const string QUERY_UNIT = "{query}";

    private static readonly ActivitySource _activitySource = new(
        FileQueryTelemetry.ActivitySourceName,
        FileQueryTelemetry.InstrumentationVersion);

    private static readonly Meter _meter = new(
        FileQueryTelemetry.MeterName,
        FileQueryTelemetry.InstrumentationVersion);

    private static readonly Counter<long> _queriesStarted = _meter.CreateCounter<long>(
        FileQueryTelemetry.QueriesStartedMetric,
        unit: QUERY_UNIT,
        description: "Number of file queries that started enumeration.");

    private static readonly Counter<long> _queriesCompleted = _meter.CreateCounter<long>(
        FileQueryTelemetry.QueriesCompletedMetric,
        unit: QUERY_UNIT,
        description: "Number of file queries that completed without an exception.");

    private static readonly Counter<long> _queriesErrors = _meter.CreateCounter<long>(
        FileQueryTelemetry.QueriesErrorsMetric,
        unit: QUERY_UNIT,
        description: "Number of file queries that failed with an exception.");

    private static readonly Counter<long> _queriesCancelled = _meter.CreateCounter<long>(
        FileQueryTelemetry.QueriesCancelledMetric,
        unit: QUERY_UNIT,
        description: "Number of file queries that were cancelled.");

    private static readonly Histogram<double> _queryDuration = _meter.CreateHistogram<double>(
        FileQueryTelemetry.QueryDurationMetric,
        unit: "s",
        description: "Duration of file query enumeration.");

    private static readonly Counter<long> _entriesScanned = _meter.CreateCounter<long>(
        FileQueryTelemetry.EntriesScannedMetric,
        unit: "{entry}",
        description: "Number of filesystem entries scanned during traversal.");

    private static readonly Counter<long> _filesMatched = _meter.CreateCounter<long>(
        FileQueryTelemetry.FilesMatchedMetric,
        unit: "{file}",
        description: "Number of files matched by a query.");

    /// <summary>
    /// Gets a value indicating whether any trace listener is attached to the activity source.
    /// </summary>
    public static bool HasActivityListeners => _activitySource.HasListeners();

    /// <summary>
    /// Starts a query activity when at least one listener is attached.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="options">The query options used to derive low-cardinality tags.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is attached.</returns>
    public static Activity? StartActivity(string name, FileQueryOptions options) {
        if(!_activitySource.HasListeners()) {
            return null;
        }

        var activity = _activitySource.StartActivity(name, ActivityKind.Internal);

        if(activity is null) {
            return null;
        }

        activity
            .SetTag("filequery.interpretation", options.PatternInput.InterpretationMode.ToString())
            .SetTag("filequery.matching_mode", options.PatternMatchingMode.ToString())
            .SetTag("filequery.case_sensitivity", options.CaseSensitivity.ToString())
            .SetTag("filequery.recurse", options.RecurseSubdirectories)
            .SetTag("filequery.include_directories", options.IncludeDirectories)
            .SetTag("filequery.strategy", options.Traversal.Strategy.ToString())
            .SetTag("filequery.symlink_policy", options.Traversal.SymlinkPolicy.ToString())
            .SetTag("filequery.pattern_count", CountPatterns(options.PatternInput));

        return activity;
    }

    /// <summary>
    /// Records the start of a query.
    /// </summary>
    /// <returns>The timestamp captured at the start, used by <see cref="RecordCompleted"/>.</returns>
    public static long RecordStarted() {
        _queriesStarted.Add(1);
        return Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Records the successful completion of a query.
    /// </summary>
    /// <param name="startTimestamp">The timestamp returned by <see cref="RecordStarted"/>.</param>
    public static void RecordCompleted(long startTimestamp) {
        _queriesCompleted.Add(1);

        if(_queryDuration.Enabled) {
            _queryDuration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
        }
    }

    /// <summary>
    /// Records a query failure.
    /// </summary>
    /// <param name="startTimestamp">The timestamp returned by <see cref="RecordStarted"/>.</param>
    public static void RecordError(long startTimestamp) {
        _queriesErrors.Add(1);

        if(_queryDuration.Enabled) {
            _queryDuration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
        }
    }

    /// <summary>
    /// Records a cancelled query.
    /// </summary>
    /// <param name="startTimestamp">The timestamp returned by <see cref="RecordStarted"/>.</param>
    public static void RecordCancelled(long startTimestamp) {
        _queriesCancelled.Add(1);

        if(_queryDuration.Enabled) {
            _queryDuration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
        }
    }

    /// <summary>
    /// Records traversal counters.
    /// </summary>
    /// <param name="entriesScanned">The number of entries scanned.</param>
    /// <param name="filesMatched">The number of files matched.</param>
    public static void RecordTraversal(long entriesScanned, long filesMatched) {
        if(_entriesScanned.Enabled) {
            _entriesScanned.Add(entriesScanned);
        }

        if(_filesMatched.Enabled) {
            _filesMatched.Add(filesMatched);
        }
    }

    /// <summary>
    /// Counts the configured untyped and typed patterns without enumerating them twice.
    /// </summary>
    /// <param name="input">The pattern input.</param>
    /// <returns>The total number of configured patterns.</returns>
    private static int CountPatterns(PatternInput input) {
        var count = input.Patterns.Count;

        foreach(var patterns in input.TypedPatterns.Values) {
            count += patterns.Count;
        }

        return count;
    }
}
