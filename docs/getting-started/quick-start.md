# Quick Start

Five minutes from installation to a working query. Every example uses the current v2.0.0 API.

Most snippets assume:

```csharp
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;
```

Add `using Jeninnet.FileQuery.Traversal;` for `TraversalOptions`, and
`using Microsoft.Extensions.FileProviders;` for the provider example.

---

## 1 — Install and run your first query

```bash
dotnet add package Jeninnet.FileQuery
```

```csharp
using Jeninnet.FileQuery;

var files = FileQuery.From(@"C:\repo").Execute();

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

`FileQuery.From(...).Execute()` is the shortest path: it builds and executes with the default
engine. Use `Build()` + `FileQueryRuntime.Create()` when you want to reuse an engine or compose it
through dependency injection:

```csharp
var query = FileQuery.From(@"C:\repo").Build();
var engine = FileQueryRuntime.Create();

foreach (var file in engine.Execute(query))
{
    Console.WriteLine(file);
}
```

`Build()` validates the root immediately and throws `DirectoryNotFoundException` when it does not
exist.

---

## 2 — Include only matching files

Patterns are ordered GitIgnore-style rules: `**` excludes everything, `!` re-includes.

```csharp
var query = FileQuery.From(@"C:\repo")
    .Where("**", "!**/*.cs")
    .Build();
```

For Glob-only or Regex-only queries, select the dialect explicitly:

```csharp
var glob = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .Execute();

var regex = FileQuery.From(root)
    .UsingRegex()
    .Where("r:^src/.*\\.cs$")
    .Execute();
```

Hybrid mode (the default) classifies each pattern automatically:

```csharp
var mixed = FileQuery.From(root)
    .UsingHybrid()
    .Where(
        "**",                       // exclude everything
        "!src/**/*.cs",             // re-include source files
        "r:.*\\.(png|jpg)$")        // re-include images by regex
    .Execute();
```

---

## 3 — Exclude directories

A trailing `/` marks a directory-only rule. It prunes the subtree during traversal.

```csharp
var query = FileQuery.From(root)
    .Where("bin/", "obj/", "!**/*.cs")
    .Build();
```

---

## 4 — Read metadata and filter in the same pass

`ExecuteEntries()` returns `FileQueryEntry` values populated from the enumeration, so no extra
`FileInfo` calls are made.

```csharp
var entries = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.log"])
    .MinSize(1_024)
    .ModifiedAfter(DateTimeOffset.UtcNow.AddDays(-7))
    .ExecuteEntries();

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.RelativePath}  {entry.Length} bytes");
}
```

Available filters: `MinSize`, `MaxSize`, `ModifiedAfter`, `ModifiedBefore`, `AttributesToSkip`.

---

## 5 — Include directories in the results

```csharp
var results = FileQuery.From(root)
    .IncludeDirectories()
    .ExecuteEntries();

foreach (var entry in results.Where(e => e.IsDirectory))
{
    Console.WriteLine(entry.RelativePath);
}
```

Directories come from the same walk; directory-only rules and negation still apply.

---

## 6 — Respect `.gitignore`

```csharp
var query = FileQuery.From(repositoryRoot)
    .UseGitIgnoreFiles()
    .Build();
```

This loads the root and nested `.gitignore` files, `.git/info/exclude`, and an optional global
excludes file with Git-compatible scoping and negation. No Git executable is required.

---

## 7 — Async streaming, cancellation, and progress

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var progress = new Progress<FileQueryProgress>(p =>
    Console.WriteLine($"{p.EntriesScanned} scanned, {p.FilesMatched} matched"));

await foreach (var file in FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .ExecuteAsync(progress, cts.Token))
{
    await ProcessAsync(file, cts.Token);
}
```

Synchronous `Execute`/`ExecuteEntries` accept the same `IProgress<FileQueryProgress>` and
`CancellationToken` arguments.

---

## 8 — Query several roots

```csharp
var batch = FileQueryBatch.From(
    FileQuery.From(firstRoot).UsingGlob().Where(PatternKind.Glob, ["**/*.cs"]),
    FileQuery.From(secondRoot).UsingGlob().Where(PatternKind.Glob, ["**/*.cs"]));

foreach (var entry in batch.ExecuteEntries())
{
    Console.WriteLine($"{entry.RootPath}: {entry.RelativePath}");
}
```

Each entry exposes `RootPath`, so identical relative paths remain unambiguous.

---

## 9 — Match a single path without traversal

```csharp
var patternSet = FileQueryPatternSet.Compile(
    new PatternInput(
        TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>>
        {
            [PatternKind.GitIgnore] = ["**", "!**/*.cs", "!bin/"]
        },
        InterpretationMode: PatternInterpretationMode.Specific),
    PatternMatchingMode.GitIgnore);

bool isSource = patternSet.IsMatch("src/Program.cs");
var explanation = patternSet.Explain("src/Program.cs");

Console.WriteLine($"{isSource}: {explanation.Reason}");
```

`IsMatch(ReadOnlySpan<char>)` is allocation-free and intended for linters, IDE actions, and path
classifiers.

---

## 10 — Query an ASP.NET Core `IFileProvider`

```bash
dotnet add package Jeninnet.FileQuery.FileProviders
```

```csharp
using var provider = new PhysicalFileProvider(webRoot);

var assets = FileProviderQuery.From(provider)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.css"])
    .Execute();
```

The adapter supports physical, embedded, and in-memory providers without copying files to disk.

---

## Next steps

- [Basic patterns](basic-patterns.md) — the pattern language in detail.
- [Common recipes](common-recipes.md) — copy-paste queries for frequent tasks.
- [Entry metadata and filters](../guides/entry-metadata-and-filters.md)
- [Git ignore files](../guides/gitignore-files.md)
- [Error handling and validation](../guides/error-handling-and-validation.md)
- [Pattern reference](../reference/pattern-overview.md)
- [Engine architecture](../architecture/engine-architecture.md)
