# FileQueryRuntime

`FileQueryRuntime` is the composition root for the engine. It is the entry point for all file querying operations.

---

## Creating an Engine

```csharp
IFileQueryEngine engine = FileQueryRuntime.Create();
```

`Create()` returns a fully wired `IFileQueryEngine` backed by the default compilation pipeline, traversal executor, and pattern matchers. It is safe to call multiple times — each call returns a new engine instance that shares no mutable state with other instances.

For applications that use dependency injection, prefer `AddFileQuery()` over calling `Create()` directly. See [Dependency Injection](../guides/dependency-injection.md).

---

## IFileQueryEngine

The engine exposes path and metadata result families, each with sync/async and
cancellation/progress overloads:

```csharp
public interface IFileQueryEngine
{
    IEnumerable<string> Execute(FileQuery query);
    IEnumerable<string> Execute(FileQuery query, CancellationToken cancellationToken);
    IEnumerable<string> Execute(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ExecuteAsync(
        FileQuery query,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ExecuteAsync(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default);

    IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query);
    IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query, CancellationToken cancellationToken);
    IEnumerable<FileQueryEntry> ExecuteEntries(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
        FileQuery query,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
        FileQuery query,
        IProgress<FileQueryProgress>? progress,
        CancellationToken cancellationToken = default);
}
```

Path APIs return absolute filesystem paths using the native separator. Entry APIs return
`FileQueryEntry` with root-relative paths and enumeration metadata.

---

## Building a Query

Use `FileQuery.From(rootPath)` to start a fluent query builder:

```csharp
var query = FileQuery.From(@"C:\repo")
                     .Where("**", "!*.cs")
                     .IgnoreCase()
                     .Build();

var results = engine.Execute(query).ToList();
```

The builder validates the configuration and throws `ArgumentException` when the root path is null or
empty. `DirectoryNotFoundException` is thrown if the root directory does not exist when the query
is built.

---

## Advanced Configuration

Build the query through the fluent API; `FileQuery` itself is an immutable descriptor and its
constructor is internal. For options that do not have a fluent method, use the corresponding builder
method or the `FileQueryOptionsConfig` surface exposed by the library.

```csharp
var query = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .WithRecursion()
    .IgnoreCase()
    .IgnoreInaccessible()
    .WithErrorRecovery(FileQueryErrorRecoveryOptions.Skip)
    .Build();

var results = engine.Execute(query);
```

`FileQuery.Options` is intentionally internal. Inspect behavior through the public builder,
`FileQueryPatternSet`, diagnostics, and the Tooling SDK instead of reading option state.

---

## Thread Safety

A single `IFileQueryEngine` instance is safe to use from multiple threads simultaneously. Each call to `Execute` or `ExecuteAsync` creates its own `TraversalPlan` and `TraversalFrontier` — no shared mutable state exists between concurrent calls.

The `RegexInstructionMatcher` maintains a `ConcurrentDictionary` cache of compiled `Regex` instances; this is the only shared state, and it is thread-safe by design.

---

## Streaming Behaviour

`Execute` returns `IEnumerable<string>`. The enumeration is lazy — filesystem traversal begins when the first `MoveNext()` is called on the enumerator. Disposing the enumerator before exhausting it stops traversal immediately.

`ExecuteAsync` returns `IAsyncEnumerable<string>`. Traversal is cooperative-async: each path is yielded with an `await Task.Yield()` to allow other async operations to interleave. Pass a `CancellationToken` to stop traversal on demand.

The underlying .NET directory and attribute APIs used by the default filesystem are synchronous. `ExecuteAsync` does not create a thread-pool work item per filesystem entry; it performs synchronous enumeration with cancellation checkpoints and yields control between entries. This keeps large traversals predictable and avoids unbounded thread-pool pressure, while still fitting async consumer pipelines.

---

## Using the Fluent API Directly

`FileQueryBuilder` exposes `Execute()` and `ExecuteAsync()` shorthand methods that bypass the intermediate `FileQuery` object:

```csharp
// Equivalent to engine.Execute(FileQuery.From(root).Where(...).Build())
var results = FileQuery.From(root)
                       .Where("**", "!*.txt")
                       .Execute()
                       .ToList();
```

The shorthand creates a default engine lazily on first call.
