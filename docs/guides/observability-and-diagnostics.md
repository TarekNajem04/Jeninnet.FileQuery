# Observability and Diagnostics

Jeninnet.FileQuery provides deep insights into the enumeration and matching process through progress reporting, detailed diagnostic audits, and configurable IO error recovery.

## 1. Progress Reporting

For long-running filesystem scans, monitor progress with `IProgress<FileQueryProgress>` on either the sync or async path. Both report the same snapshot shape and final counts.

```csharp
var progress = new Progress<FileQueryProgress>(p => {
    Console.WriteLine($"Scanned: {p.EntriesScanned}, Matched: {p.FilesMatched}, Current: {p.CurrentPath}");
});

var engine = FileQueryRuntime.Create();

// Asynchronous
await foreach (var file in engine.ExecuteAsync(query, progress, cancellationToken))
{
    // ...
}

// Synchronous (equivalent snapshot shape)
foreach (var file in engine.Execute(query, progress, cancellationToken))
{
    // ...
}
```

## 2. Match Diagnostics (Audit Mode)

To understand *why* a file was included or excluded, attach a diagnostics sink with
`WithDiagnostics`. The engine invokes it for every evaluated entry while traversal runs.

```csharp
var diagnostics = new Progress<FileQueryDiagnostic>(d =>
{
    Console.WriteLine($"{d.RelativePath}: {d.Outcome} — {d.Reason}");
    Console.WriteLine($"  pattern: {d.Pattern ?? "(none)"} ({d.PatternKind?.ToString() ?? "n/a"})");
});

var query = FileQuery.From(root)
    .UsingHybrid()
    .Where("**", "!**/*.cs")
    .WithDiagnostics(diagnostics)
    .Build();

foreach (var file in FileQueryRuntime.Create().Execute(query))
{
    Console.WriteLine(file);
}
```

`FileQueryDiagnostic` also exposes `Path`, `EntryKind`, `PatternIndex`, and optional metadata
(`Length`, timestamps, attributes, `IsDirectory`). Audit mode adds per-entry overhead; enable it for
diagnosis, not for hot production scans.

## 3. OpenTelemetry Instrumentation

The library ships built-in `ActivitySource` and `Meter` instrumentation. No OpenTelemetry package is
required by `Jeninnet.FileQuery`; consumers attach exporters.

| Item | Value |
|---|---|
| Activity source | `Jeninnet.FileQuery` (`FileQueryTelemetry.ActivitySourceName`) |
| Meter | `Jeninnet.FileQuery` (`FileQueryTelemetry.MeterName`) |
| Sync span | `FileQuery.Execute` |
| Async span | `FileQuery.ExecuteAsync` |

Span tags use low-cardinality values only: `filequery.interpretation`, `filequery.matching_mode`,
`filequery.case_sensitivity`, `filequery.recurse`, `filequery.include_directories`,
`filequery.strategy`, `filequery.symlink_policy`, `filequery.pattern_count`, and
`filequery.outcome`.

Metrics:

| Metric | Type | Unit | Description |
|---|---|---|---|
| `filequery.queries.started` | Counter | `{query}` | Queries that started enumeration. |
| `filequery.queries.completed` | Counter | `{query}` | Queries that completed without an exception. |
| `filequery.queries.errors` | Counter | `{query}` | Queries that failed with an exception. |
| `filequery.queries.cancelled` | Counter | `{query}` | Queries that were cancelled. |
| `filequery.queries.duration` | Histogram | `s` | Query enumeration duration. |
| `filequery.entries.scanned` | Counter | `{entry}` | Filesystem entries scanned. |
| `filequery.files.matched` | Counter | `{file}` | Files matched. |

### OpenTelemetry wiring

```csharp
using Jeninnet.FileQuery.Telemetry;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(FileQueryTelemetry.ActivitySourceName)
    .AddConsoleExporter()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(FileQueryTelemetry.MeterName)
    .AddConsoleExporter()
    .Build();

var engine = FileQueryRuntime.Create();
await foreach (var file in engine.ExecuteAsync(query))
{
    Console.WriteLine(file);
}
```

Instrumentation performs no allocation when no listener is attached: activities and tags are only
created when the source has listeners, and metric instruments are no-ops without a meter listener.

## 4. Error Recovery


Configure how the engine handles IO errors (e.g., access denied, file locked) using `FileQueryErrorRecoveryOptions`.

```csharp
var options = new FileQueryOptions(
    new FileQueryOptionsConfig(
        PatternInput: myPatterns,
        ErrorRecovery: new FileQueryErrorRecoveryOptions(
            Action: FileQueryErrorAction.Skip, // Default: Skip, Retry, Abort
            MaxRetryAttempts: 3
        )
    )
);
```
