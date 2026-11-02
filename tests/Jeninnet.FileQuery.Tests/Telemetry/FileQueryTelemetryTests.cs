//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Jeninnet.FileQuery.Telemetry;

namespace Jeninnet.FileQuery.Tests.Telemetry;

/// <summary>
/// Tests for the built-in OpenTelemetry-compatible instrumentation.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class FileQueryTelemetryTests {
    /// <summary>
    /// Synchronous execution emits a span with the stable name and low-cardinality tags.
    /// </summary>
    [TestMethod]
    public void Execute_EmitsActivity_WithLowCardinalityTags() {
        using var environment = CreateEnvironment();
        var activities = new List<Activity>();
        using var listener = CreateActivityListener(activities);

        var engine = FileQueryRuntime.Create();
        var query = FileQuery.From(environment.Root)
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.txt"])
            .Build();

        _ = engine.Execute(query, TestContext.CancellationToken).ToArray();

        activities.Should().HaveCount(1);

        var activity = activities[0];
        activity.OperationName.Should().Be(FileQueryTelemetry.ExecuteActivityName);
        (activity.GetTagItem("filequery.matching_mode")?.ToString()).Should().Be("Glob");
        (activity.GetTagItem("filequery.interpretation")?.ToString()).Should().Be("Specific");
        (activity.GetTagItem("filequery.outcome")?.ToString()).Should().Be("completed");
    }

    /// <summary>
    /// Asynchronous execution emits the asynchronous span name.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_EmitsAsyncActivityAsync() {
        using var environment = CreateEnvironment();
        var activities = new List<Activity>();
        using var listener = CreateActivityListener(activities);

        var engine = FileQueryRuntime.Create();
        var query = FileQuery.From(environment.Root)
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.txt"])
            .Build();

        var results = new List<string>();

        await foreach(var path in engine.ExecuteAsync(query, TestContext.CancellationToken)) {
            results.Add(path);
        }

        results.Should().HaveCount(3);
        activities.Should().HaveCount(1);
        activities[0].OperationName.Should().Be(FileQueryTelemetry.ExecuteAsyncActivityName);
    }

    /// <summary>
    /// Execution publishes query, traversal, and duration metrics.
    /// </summary>
    [TestMethod]
    public void Execute_EmitsMetrics() {
        using var environment = CreateEnvironment();
        var counters = new ConcurrentDictionary<string, long>(StringComparer.Ordinal);
        var histograms = new ConcurrentDictionary<string, double>(StringComparer.Ordinal);

        using var listener = new MeterListener {
            InstrumentPublished = static (instrument, meterListener) => {
                if(string.Equals(instrument.Meter.Name, FileQueryTelemetry.MeterName, StringComparison.Ordinal)) {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            counters.AddOrUpdate(instrument.Name, measurement, (_, current) => current + measurement));
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
            histograms.AddOrUpdate(instrument.Name, measurement, (_, current) => current + measurement));
        listener.Start();

        var engine = FileQueryRuntime.Create();
        var query = FileQuery.From(environment.Root)
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.txt"])
            .Build();

        _ = engine.Execute(query, TestContext.CancellationToken).ToArray();

        counters[FileQueryTelemetry.QueriesStartedMetric].Should().BeGreaterThanOrEqualTo(1);
        counters[FileQueryTelemetry.QueriesCompletedMetric].Should().BeGreaterThanOrEqualTo(1);
        counters[FileQueryTelemetry.EntriesScannedMetric].Should().BeGreaterThanOrEqualTo(1);
        counters[FileQueryTelemetry.FilesMatchedMetric].Should().Be(3);
        histograms[FileQueryTelemetry.QueryDurationMetric].Should().BeGreaterThan(0);
    }

    /// <summary>
    /// A traversal failure increments the error counter.
    /// </summary>
    [TestMethod]
    public void Execute_RecordsErrorMetric_WhenTraversalFails() {
        using var environment = CreateEnvironment();
        var engine = new Engine.FileQueryEngine(new ThrowingTraversalExecutor(), new NullPlanBuilder());
        var query = FileQuery.From(environment.Root).Build();
        var results = engine.Execute(query, TestContext.CancellationToken);

        var counters = new ConcurrentDictionary<string, long>(StringComparer.Ordinal);
        using var listener = CreateLongCounterListener(counters);

        FluentActions.Invoking(() => { _ = results.ToList(); }).Should().ThrowExactly<IOException>();
        counters[FileQueryTelemetry.QueriesErrorsMetric].Should().BeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Instrumentation performs no allocation when no listener is attached.
    /// </summary>
    [TestMethod]
    public void Instrumentation_DoesNotAllocate_WhenNoListenerAttached() {
        var query = FileQuery.From(Path.GetTempPath()).Build();

        // Warm up lazily initialized static state before measuring.
        FileQueryInstrumentation.StartActivity(FileQueryTelemetry.ExecuteActivityName, query.Options);
        FileQueryInstrumentation.RecordTraversal(0, 0);

        FileQueryInstrumentation.HasActivityListeners.Should().BeFalse();

        var startTimestamp = FileQueryInstrumentation.RecordStarted();
        var before = GC.GetAllocatedBytesForCurrentThread();

        FileQueryInstrumentation.RecordTraversal(10, 2);
        FileQueryInstrumentation.RecordCompleted(startTimestamp);

        var after = GC.GetAllocatedBytesForCurrentThread();

        after.Should().Be(before);
    }

    /// <summary>
    /// Creates a test tree with two matching files.
    /// </summary>
    private static TestEnvironment CreateEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFiles(
            "a.txt",
            "b.txt",
            "c.log",
            "sub/d.txt");
        return environment;
    }

    /// <summary>
    /// Registers a meter listener that accumulates long counters by instrument name.
    /// </summary>
    /// <param name="counters">The counter sink.</param>
    private static MeterListener CreateLongCounterListener(ConcurrentDictionary<string, long> counters) {
        var listener = new MeterListener {
            InstrumentPublished = static (instrument, meterListener) => {
                if(string.Equals(instrument.Meter.Name, FileQueryTelemetry.MeterName, StringComparison.Ordinal)) {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            counters.AddOrUpdate(instrument.Name, measurement, (_, current) => current + measurement));
        listener.SetMeasurementEventCallback<double>(static (_, _, _, _) => { });
        listener.Start();

        return listener;
    }

    /// <summary>
    /// Registers an activity listener that records stopped activities.
    /// </summary>
    /// <param name="activities">The activity sink.</param>
    private static ActivityListener CreateActivityListener(List<Activity> activities) {
        var listener = new ActivityListener {
            ShouldListenTo = static source => string.Equals(
                source.Name,
                FileQueryTelemetry.ActivitySourceName,
                StringComparison.Ordinal),
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => {
                lock(activities) {
                    activities.Add(activity);
                }
            }
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    /// <summary>
    /// A traversal executor that yields one result and then fails.
    /// </summary>
    private sealed class ThrowingTraversalExecutor : ITraversalExecutor {
        public IEnumerable<string> Execute(TraversalPlan plan, CancellationToken cancellationToken = default) {
            yield return "first";
            throw new IOException("simulated traversal failure");
        }
        public IAsyncEnumerable<string> ExecuteAsync(TraversalPlan plan, CancellationToken cancellationToken) => throw new NotSupportedException();
        public IEnumerable<FileQueryEntry> ExecuteEntries(TraversalPlan plan, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(TraversalPlan plan, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    /// <summary>
    /// A plan builder that returns a placeholder plan for the fake traversal.
    /// </summary>
    private sealed class NullPlanBuilder : ITraversalPlanBuilder {
        public TraversalPlan Build(FileQuery query, IProgress<FileQueryProgress>? progress = null) => null!;
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
