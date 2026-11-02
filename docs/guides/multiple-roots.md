# Multiple Roots

`FileQueryBatch` runs several built queries as one ordered stream. Results from one root are
emitted completely before the next root starts, so each root keeps the same deterministic ordering
as a standalone query.

---

## Usage

```csharp
var batch = FileQueryBatch.From(
    FileQuery.From(@"C:\src\app").Where("**/*.cs"),
    FileQuery.From(@"C:\src\tools").Where("**/*.cs"));

foreach (var path in batch.Execute())
{
    Console.WriteLine(path); // absolute path
}
```

Use `Create` when the queries are already built:

```csharp
var queries = roots.Select(root => FileQuery.From(root)
    .UsingGitIgnore()
    .Where("**;!bin/")
    .Build());

var batch = FileQueryBatch.Create(queries);
```

---

## Entry results and root identity

`ExecuteEntries()` exposes `FileQueryEntry.RootPath` so identical relative paths below different
roots remain unambiguous:

```csharp
foreach (var entry in batch.ExecuteEntries())
{
    Console.WriteLine($"{entry.RootPath} | {entry.RelativePath} | {entry.Length}");
}
```

`Execute()` returns absolute paths only, which are also unambiguous. `Roots` lists the batch roots
in execution order.

---

## Ordering, cancellation, and errors

- Roots run in the order supplied; each root keeps DFS/BFS ordering.
- `CancellationToken` is checked between roots and forwarded to each query.
- Progress snapshots are forwarded to each query; counters restart at zero for every root.
- Each query keeps its own options. IO errors in a root follow that query's
  `FileQueryErrorRecoveryOptions`; a failure that aborts one query does not silently change the
  policy of the remaining queries (contact with the caller through the exception is preserved).

The asynchronous batch (`ExecuteAsync` / `ExecuteEntriesAsync`) follows the same ordering and
root semantics.
