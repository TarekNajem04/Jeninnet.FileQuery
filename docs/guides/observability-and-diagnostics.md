# Observability and Diagnostics

Jeninnet.FileQuery provides deep insights into the enumeration and matching process through asynchronous progress reporting, detailed diagnostic audits, and configurable IO error recovery.

## 1. Progress Reporting

For long-running filesystem scans, you can monitor progress using `IProgress<FileQueryProgress>`.

```csharp
var progress = new Progress<FileQueryProgress>(p => {
    Console.WriteLine($"Processed: {p.EntriesProcessed}, Current: {p.CurrentDirectory}");
});

var engine = FileQueryRuntime.Create();
await foreach (var file in engine.ExecuteAsync(query, progress, cancellationToken))
{
    // ...
}
```

## 2. Match Diagnostics (Audit Mode)

To understand *why* a file was included or excluded, enable Audit mode in `FileQueryOptions`.

```csharp
var options = new FileQueryOptions(
    patternInput: myPatterns,
    auditMatches: true // Enable diagnostics
);

var engine = FileQueryRuntime.Create();
await foreach (var result in engine.ExecuteAsync(new(root, options)))
{
    // Access diagnostics for this match
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"{result.Path}: {diagnostic.Message} (Pattern: {diagnostic.Pattern})");
    }
}
```

## 3. Error Recovery

Configure how the engine handles IO errors (e.g., access denied, file locked) using `FileQueryErrorRecoveryOptions`.

```csharp
var options = new FileQueryOptions(
    patternInput: myPatterns,
    errorRecovery: new FileQueryErrorRecoveryOptions(
        Action: FileQueryErrorAction.Skip, // Default: Skip, Retry, Abort
        MaxRetries: 3
    )
);
```
