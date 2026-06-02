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

The engine exposes two methods:

```csharp
public interface IFileQueryEngine
{
    IEnumerable<string> Execute(FileQuery query);

    IAsyncEnumerable<string> ExecuteAsync(
        FileQuery query,
        CancellationToken cancellationToken = default);
}
```

Both methods return absolute file-system paths as produced by the underlying `Directory` enumeration APIs. Paths use the native directory separator for the current platform.

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

The builder validates the configuration and throws `InvalidOperationException` if the root path is null or whitespace. `DirectoryNotFoundException` is thrown if the root directory does not exist when the query is built.

---

## Executing Without the Builder

Construct `FileQueryOptions` directly and pass it to the engine:

```csharp
var options = new FileQueryOptions {
    PatternInput = new(patterns: ["**", "!src/**/*.cs"]),
    RecurseSubdirectories = true,
    CaseSensitivity       = CaseSensitivity.Insensitive
};

var query   = new FileQuery(rootPath, options);  // internal constructor via From()
var results = engine.Execute(query);
```

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
