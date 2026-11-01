# Error Handling and Validation

This page covers the errors a caller can observe, how to configure recovery, and how to report
pattern problems to users.

---

## Pattern validation

Malformed patterns fail during compilation, before traversal starts. `PatternException` carries the
validator's reason and, when known, the offending character position and slash-separated segment:

```csharp
try
{
    var query = FileQuery.From(root)
        .UsingGlob()
        .Where(PatternKind.Glob, ["src/[abc"])
        .Build();
}
catch(PatternException ex)
{
    Console.WriteLine(ex.Message);     // Invalid pattern 'src/[abc': Missing closing bracket ...
    Console.WriteLine(ex.Position);    // 4
    Console.WriteLine(ex.Segment);     // 1
}
```

The same checks are available without executing a query through the Tooling SDK:

```csharp
var validation = PatternInspector.Validate("src/[abc");

if(!validation.IsValid)
{
    Console.WriteLine($"{validation.Error} at {validation.Position}");
}
```

## Regex timeouts

Regex patterns are compiled with a one-second match timeout. If an expression exceeds it, the
matcher throws `PatternException` with the source pattern and the inner
`RegexMatchTimeoutException`. Keep expressions simple; catastrophic backtracking is still a
performance risk even with a timeout.

## Inaccessible entries

By default, inaccessible files and directories are skipped:

```csharp
var files = FileQuery.From(root).Execute();
```

To surface access failures instead:

```csharp
var files = FileQuery.From(root)
    .IgnoreInaccessible(false)
    .Execute();
```

Disabling `IgnoreInaccessible` selects the `Abort` recovery policy unless you call
`WithErrorRecovery(...)`.

## Error recovery policies

```csharp
var files = FileQuery.From(root)
    .WithErrorRecovery(FileQueryErrorRecoveryOptions.Retry(2))
    .Execute();
```

| Policy | Behavior |
|---|---|
| `Skip` | Log nothing and continue with the next entry/directory |
| `Retry(n)` | Re-attempt the failed directory enumeration up to `n` times |
| `Abort` | Rethrow the recoverable IO exception |

Recoverable errors are `UnauthorizedAccessException`, `IOException`, and
`DirectoryNotFoundException`.

## Cancellation

Cancellation is cooperative and checked between entries:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

await foreach (var file in FileQuery.From(root).ExecuteAsync(cts.Token))
{
    // ...
}
```

Cancellation surfaces as `OperationCanceledException`. The synchronous overloads accept the same
token.

## Argument and state errors

| Situation | Exception |
|---|---|
| Null/empty root, too-long path, invalid path characters | `ArgumentException` / `InvalidOperationException` |
| Root does not exist | `DirectoryNotFoundException` at `Build()` |
| Typed pattern conflicts with the selected matching mode | `InvalidOperationException` |
| Invalid size/date range | `ArgumentOutOfRangeException` or `ArgumentException` |
| Pattern cannot be classified or compiled | `PatternException` |

## Provider-specific failures

An `IFileProvider` may throw during directory enumeration. The adapter applies the same
`Skip`/`Retry`/`Abort` policy to provider failures that map to the recoverable exceptions above.

## Symlinks

`SymlinkPolicy.Follow` performs no cycle detection. A symlink cycle can make traversal run
indefinitely; use `FollowWithCycleDetection` for trees that may contain cycles. See
[Traversal model](../runtime/traversal-model.md).

---

## Related

- [Observability and diagnostics](observability-and-diagnostics.md)
- [Pattern validation with the Tooling SDK](tooling-sdk.md)
- [MSBuild validation and SARIF](msbuild-validation.md)
- [Traversal model](../runtime/traversal-model.md)
