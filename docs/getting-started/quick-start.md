# Quick Start

This page shows the most common usage patterns. Five minutes is enough to get from installation to a working query.

---

## 1 — Enumerate All Files

The simplest query: enumerate every file under a root directory.

```csharp
using Jeninnet.FileQuery;

var engine = FileQueryRuntime.Create();
var query  = FileQuery.From(@"C:\repo").Build();

foreach (var file in engine.Execute(query))
{
    Console.WriteLine(file);
}
```

`Build()` validates the root directory immediately. If `C:\repo` does not exist,
it throws `DirectoryNotFoundException` before execution starts. Create or verify
the root path before building reusable queries.

---

## 2 — Include Only Matching Files

Use the fluent API to specify patterns. The `Where(...)` method accepts any combination of GitIgnore, Glob, and Regex patterns.

```csharp
var query = FileQuery.From(@"C:\repo")
                     .Where(
                         "**",           // exclude everything by default
                         "!src/**/*.cs"  // include only .cs files under src/
                     )
                     .Build();

var results = engine.Execute(query).ToList();
```

---

## 3 — Exclude Directories

Directory-only patterns end with `/`. They prevent the traversal engine from descending into matched directories.

```csharp
var query = FileQuery.From(@"C:\repo")
                     .Where(
                         "bin/",         // skip bin/ directories entirely
                         "obj/",         // skip obj/ directories entirely
                         "!**/*.cs"      // include all .cs files elsewhere
                     )
                     .Build();
```

---

## 4 — Case-Insensitive Matching

```csharp
var query = FileQuery.From(@"C:\repo")
                     .Where("**", "!**/*.TXT")
                     .IgnoreCase()
                     .Build();
```

`IgnoreCase()` makes the pattern `!**/*.TXT` match `file.txt`, `File.TXT`, and `FILE.Txt` equally.

---

## 5 — Limit Recursion Depth

```csharp
var options = new FileQueryOptions {
    PatternInput      = new(patterns: ["**", "!*.cs"]),
    MaxRecursionDepth = 2   // root + two levels deep
};

var results = engine.Execute(new(rootPath, options)).ToList();
```

`MaxRecursionDepth = 0` returns only files in the root directory.
`MaxRecursionDepth = -1` (the default) allows unlimited depth.

---

## 6 — Async Enumeration

All queries support `IAsyncEnumerable<string>` for use in async pipelines:

```csharp
await foreach (var file in engine.ExecuteAsync(query, cancellationToken))
{
    await ProcessAsync(file);
}
```

---

## 7 — Mix Pattern Languages

Combine GitIgnore, Glob, and Regex patterns in one rule set:

```csharp
var query = FileQuery.From(@"C:\repo")
                     .UsingHybrid()
                     .Where(
                         "**",                    // exclude everything
                         "!src/**/*.cs",          // include source files (GitIgnore)
                         "r:^src/.*Engine.*\\.cs$" // include Engine classes (Regex)
                     )
                     .Build();
```

---

## 8 — Using the Options Record Directly

For advanced configuration, construct `FileQueryOptions` directly:

```csharp
var options = new FileQueryOptions {
    PatternInput = new(
        patterns: ["**", "!**/*.cs", "!**/*.md"]
    ),
    RecurseSubdirectories = true,
    IgnoreInaccessible    = true,
    CaseSensitivity       = CaseSensitivity.Insensitive,
    Traversal = new TraversalOptions(
        Strategy:      TraversalStrategy.BreadthFirst,
        SymlinkPolicy: SymlinkPolicy.Ignore
    )
};

var results = engine.Execute(new(rootPath, options)).Order().ToList();
```

---

## Next Steps

- [Basic Patterns](basic-patterns.md) — a detailed walkthrough of the pattern language
- [Pattern Language Reference](../specification/pattern-language.md) — complete syntax specification
- [Architecture Overview](../architecture/engine-architecture.md) — how the engine works internally
