//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Telemetry;

/// <summary>
/// Defines the stable names and instrumentation version used by the built-in
/// <see cref="System.Diagnostics.ActivitySource"/> and <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
/// <remarks>
/// <para>
/// The library ships instrumentation only: it does not depend on OpenTelemetry packages.
/// Consumers attach exporters (for example <c>OpenTelemetry.Trace.TracerProviderBuilder.AddSource</c>
/// and <c>OpenTelemetry.Metrics.MeterProviderBuilder.AddMeter</c>) using the names defined here.
/// </para>
/// <para>
/// Spans carry only low-cardinality tags. Counts are published as metrics so that span
/// cardinality remains bounded.
/// </para>
/// </remarks>
public static class FileQueryTelemetry {
    /// <summary>
    /// The name of the <see cref="System.Diagnostics.ActivitySource"/> that emits query spans.
    /// </summary>
    public const string ActivitySourceName = "Jeninnet.FileQuery";

    /// <summary>
    /// The name of the <see cref="System.Diagnostics.Metrics.Meter"/> that emits query metrics.
    /// </summary>
    public const string MeterName = "Jeninnet.FileQuery";

    /// <summary>
    /// The activity name used for synchronous query execution.
    /// </summary>
    public const string ExecuteActivityName = "FileQuery.Execute";

    /// <summary>
    /// The activity name used for asynchronous query execution.
    /// </summary>
    public const string ExecuteAsyncActivityName = "FileQuery.ExecuteAsync";

    /// <summary>
    /// The instrumentation schema version reported by the activity source and meter.
    /// </summary>
    public const string InstrumentationVersion = "1.0.0";

    /// <summary>The counter of queries that started enumeration.</summary>
    public const string QueriesStartedMetric = "filequery.queries.started";

    /// <summary>The counter of queries that completed without an exception.</summary>
    public const string QueriesCompletedMetric = "filequery.queries.completed";

    /// <summary>The counter of queries that failed with an exception.</summary>
    public const string QueriesErrorsMetric = "filequery.queries.errors";

    /// <summary>The counter of queries that were cancelled.</summary>
    public const string QueriesCancelledMetric = "filequery.queries.cancelled";

    /// <summary>The histogram of query durations, in seconds.</summary>
    public const string QueryDurationMetric = "filequery.queries.duration";

    /// <summary>The counter of filesystem entries scanned by traversal.</summary>
    public const string EntriesScannedMetric = "filequery.entries.scanned";

    /// <summary>The counter of files matched by a query.</summary>
    public const string FilesMatchedMetric = "filequery.files.matched";
}
