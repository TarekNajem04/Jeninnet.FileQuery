# LogFiltering Sample

Stream matches as they are discovered and process each file immediately, with cooperative
cancellation.

**What it teaches:** `ExecuteAsync`, `IAsyncEnumerable<string>`, and per-result processing.

## Run

```bash
dotnet run --project samples/LogFiltering/LogFiltering.csproj
```

The sample cancels after five seconds. The demo tree is small, so all **3 `.log` files** are
usually found first; the cancellation path demonstrates how a long-running consumer stops cleanly.

## Code

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

await foreach (var path in FileQuery.From(root)
    .UsingGlob()
    .Where("**/*.log")
    .ExecuteAsync(cts.Token))
{
    Console.WriteLine(path);
    await ProcessAsync(path, cts.Token);
}
```

`ExecuteAsync` yields paths during traversal; it does not buffer the whole result set. Cancellation
is observed between entries and propagated as `OperationCanceledException`.

## Related

- [Quick start](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/getting-started/quick-start.md)
- [Observability and diagnostics](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/observability-and-diagnostics.md)
- [All samples](../README.md)
