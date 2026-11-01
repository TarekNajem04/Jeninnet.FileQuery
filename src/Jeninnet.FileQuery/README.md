# Jeninnet.FileQuery

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub release](https://img.shields.io/github/v/release/TarekNajem04/Jeninnet.FileQuery)
[![GitHub license](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery)
![NuGet Downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery?label=Jeninnet.FileQuery)

**Deterministic, high-performance file discovery for .NET.** One compiled pipeline for
GitIgnore, Glob, and Regex patterns, with metadata results, directory output, `.gitignore`
ingestion, cancellation, progress, and first-class diagnostics.

```csharp
var files = FileQuery.From(@"C:\src\MyProject")
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs", "!**/obj/**"])
    .IgnoreCase()
    .Execute();

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

---

## Why Jeninnet.FileQuery?

Most file-search code starts with `Directory.GetFiles` and grows into a pile of ad-hoc
`EndsWith`, `StartsWith`, and `Regex` checks. That approach is hard to reason about, hard to test,
and repeats the same filesystem work for every filter.

Jeninnet.FileQuery replaces it with a **compile-then-execute** pipeline:

- **One pattern model.** GitIgnore, Glob, and Regex rules coexist with documented precedence.
- **Deterministic results.** The same tree, patterns, and options always produce the same order.
- **One pass.** Metadata, size/time filters, directory results, and matching happen during the
  same enumeration.
- **Safe by default.** Inaccessible entries are skipped; cancellation and progress are built in.
- **AOT-ready.** No reflection on the query path, verified by a Native AOT smoke host in CI.

---

## Install

```bash
dotnet add package Jeninnet.FileQuery
```

Requires .NET 10 or later.

---

## Quick start

### Path results

```csharp
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;

var query = FileQuery.From(repositoryRoot)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .Build();

foreach (var path in FileQueryRuntime.Create().Execute(query))
{
    Console.WriteLine(path);
}
```

Or use the builder's terminal shortcuts:

```csharp
var paths = FileQuery.From(repositoryRoot)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .Execute();
```

### Metadata results

`ExecuteEntries()` returns `FileQueryEntry` values populated from the same enumeration — no extra
`FileInfo` calls:

```csharp
var entries = FileQuery.From(repositoryRoot)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.log"])
    .MinSize(1024)
    .ModifiedAfter(DateTimeOffset.UtcNow.AddDays(-7))
    .ExecuteEntries();

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.RelativePath}  {entry.Length} bytes  {entry.LastWriteTimeUtc:O}");
}
```

`FileQueryEntry` exposes `FullPath`, `RelativePath`, `Length`, `CreationTimeUtc`,
`LastWriteTimeUtc`, `Attributes`, `IsDirectory`, and `RootPath`.

### Async streaming and cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var progress = new Progress<FileQueryProgress>(p =>
    Console.WriteLine($"{p.EntriesScanned} scanned, {p.FilesMatched} matched"));

await foreach (var path in FileQuery.From(root)
    .UsingHybrid()
    .Where("**", "!**/*.cs")
    .ExecuteAsync(progress, cts.Token))
{
    Console.WriteLine(path);
}
```

Synchronous `Execute`/`ExecuteEntries` overloads accept the same cancellation and progress
arguments.

---

## Pattern dialects

| Dialect | Syntax | Example |
|---|---|---|
| GitIgnore | Negation, anchoring, directory-only rules, last-match-wins | `!src/**/*.cs`, `bin/` |
| Glob | `*`, `**`, `?`, character classes, POSIX classes, braces | `**/*.{cs,vb}`, `file[[:digit:]].txt` |
| Regex | `r:` prefix, full normalized relative path | `r:^src/.*\.cs$` |
| Hybrid | Automatic classification per pattern | `.UsingHybrid()` |

Precedence is **GitIgnore → Glob → Regex**. A GitIgnore inclusion is final; Glob and Regex can
re-include paths excluded by GitIgnore but cannot exclude a GitIgnore inclusion.

```csharp
var query = FileQuery.From(root)
    .UsingHybrid()
    .Where(
        "**",                 // exclude everything
        "!**/*.cs",           // re-include C# files
        "r:.*\\.(png|jpg)$")  // regex re-include for images
    .IgnoreCase()
    .Build();
```

Invalid patterns fail during compilation with a `PatternException` that carries the reason and,
when known, `Position` and `Segment`.

---

## Results and filters

```csharp
var query = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*"])
    .IncludeDirectories()                       // yield matching directories too
    .MinSize(1)                                 // inclusive
    .MaxSize(10 * 1024 * 1024)                  // inclusive
    .ModifiedAfter(cutoff)                      // exclusive
    .ModifiedBefore(DateTimeOffset.UtcNow)      // exclusive
    .AttributesToSkip(FileAttributes.System)
    .Build();
```

Filters run before matching and reuse the metadata already reported by the OS enumeration.

---

## `.gitignore` ingestion

```csharp
var query = FileQuery.From(repositoryRoot)
    .UseGitIgnoreFiles()
    .Build();
```

This loads, with Git-compatible scoping and negation:

- the root `.gitignore`;
- nested `.gitignore` files, parent before child;
- `.git/info/exclude` when the root is inside a Git working tree;
- an optional global excludes file via `GitIgnoreOptions.GlobalExcludesFile`.

No Git executable is required. The CLI exposes the same behavior with `--ignore-files`.

---

## Incremental scans

Restrict a query to paths reported by a diff, watcher, or external index:

```csharp
var query = FileQuery.From(root)
    .UsingGitIgnore()
    .RestrictToPaths(changedPaths)
    .Build();
```

Paths use forward slashes and are relative to the query root. The engine skips unrelated files
and prunes directories that cannot contain a listed path.

---

## Single-path matching without traversal

For linters, IDE actions, and path classifiers:

```csharp
var patternSet = FileQueryPatternSet.Compile(
    new PatternInput(
        TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>>
        {
            [PatternKind.GitIgnore] = ["**", "!*.cs", "!bin/"]
        },
        InterpretationMode: PatternInterpretationMode.Specific),
    PatternMatchingMode.GitIgnore);

bool match = patternSet.IsMatch("src/Program.cs");
var explanation = patternSet.Explain("src/Program.cs");

Console.WriteLine(explanation.Outcome); // Include
Console.WriteLine(explanation.Reason);  // Include by GitIgnore pattern.
```

`IsMatch(ReadOnlySpan<char>)` performs no allocation on the matching path.

---

## Multiple roots

```csharp
var batch = FileQueryBatch.From(
    FileQuery.From(firstRoot).UsingGlob().Where(PatternKind.Glob, ["**/*.cs"]),
    FileQuery.From(secondRoot).UsingGlob().Where(PatternKind.Glob, ["**/*.cs"]));

foreach (var entry in batch.ExecuteEntries())
{
    Console.WriteLine($"{entry.RootPath}: {entry.RelativePath}");
}
```

Results are emitted in root order, and `RootPath` disambiguates identical relative paths.

---

## Telemetry and diagnostics

- **Progress:** `IProgress<FileQueryProgress>` on sync and async execution.
- **Match audit:** `WithDiagnostics(IProgress<FileQueryDiagnostic>)` reports the deciding pattern.
- **OpenTelemetry-ready:** `ActivitySource` and `Meter` instrumentation under the
  `Jeninnet.FileQuery` name. No OpenTelemetry package is required; attach your own exporter.

```csharp
var query = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .WithDiagnostics(new Progress<FileQueryDiagnostic>(d =>
        Console.WriteLine($"{d.RelativePath}: {d.Outcome} ({d.Reason})")))
    .Build();
```

---

## Architecture at a glance

```text
FileQueryBuilder
    → pattern classification
    → structural validation
    → immutable compiled pattern set
    → traversal plan
    → single-pass enumeration
        → metadata filters
        → compiled matcher
        → diagnostics / progress
        → path or FileQueryEntry result
```

- **Builder:** mutable configuration; `Build()` validates and produces an immutable `FileQuery`.
- **Pattern pipeline:** classification → lexical/structural/semantic invariants → compiled
  instruction sets.
- **Traversal:** DFS or BFS, depth limits, symlink policies (`Ignore`, `Follow`,
  `FollowWithCycleDetection`), error recovery (`Skip`, `Retry`, `Abort`).
- **IO boundary:** `IFileSystem` keeps `System.IO` out of the engine and enables virtual/mock
  filesystems. The separate `Jeninnet.FileQuery.FileProviders` package adapts `IFileProvider`.

See the [architecture overview](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/architecture/engine-architecture.md)
for the full design.

---

## API at a glance

| Surface | Purpose |
|---|---|
| `FileQuery.From(root)` | Start a query with the physical filesystem |
| `FileQueryBuilder.Where(...)` | Add untyped, typed, or delimited patterns |
| `UsingHybrid/UsingGitIgnore/UsingGlob/UsingRegex` | Select interpretation and matching mode |
| `IgnoreCase` / `IgnoreInaccessible` | Case and inaccessible-entry policy |
| `MinSize` / `MaxSize` / `ModifiedAfter` / `ModifiedBefore` / `AttributesToSkip` | Metadata filters |
| `UseGitIgnoreFiles` | Load Git ignore files |
| `RestrictToPaths` | Diff-aware allow-list |
| `IncludeDirectories` | Yield directories from the same walk |
| `WithDiagnostics` / `WithErrorRecovery` | Diagnostics and IO recovery |
| `Build` | Produce an immutable `FileQuery` |
| `Execute` / `ExecuteAsync` | Path results |
| `ExecuteEntries` / `ExecuteEntriesAsync` | `FileQueryEntry` metadata results |
| `FileQueryBatch` | Multiple roots as one ordered stream |
| `FileQueryPatternSet` | In-memory `IsMatch` / `Explain` |
| `FileQueryRuntime.Create()` | Compose the default engine without DI |

---

## Package family

| Package | Use it for |
|---|---|
| **Jeninnet.FileQuery** | Core engine |
| `Jeninnet.FileQuery.CommandLine` | `System.CommandLine` pattern options |
| `Jeninnet.FileQuery.DependencyInjection` | `AddFileQuery()` composition |
| `Jeninnet.FileQuery.FileProviders` | Query ASP.NET Core / in-memory `IFileProvider` sources |
| `Jeninnet.FileQuery.Tooling` | Pattern inspection without execution |
| `Jeninnet.FileQuery.Analyzers` | Design-time `JNQ1001` diagnostics and code fix |
| `Jeninnet.FileQuery.Build` | MSBuild validation and SARIF export |
| `Jeninnet.FileQuery.Tool` | `dotnet-filequery` global tool |

Test projects use the standalone
[`Jeninnet.Assertions`](https://www.nuget.org/packages/Jeninnet.Assertions) package.

---

## Documentation

- [Quick start](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/getting-started/quick-start.md)
- [Pattern language](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/specification/pattern-language.md)
- [Entry metadata and filters](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/entry-metadata-and-filters.md)
- [Git ignore files](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/gitignore-files.md)
- [Directory results](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/directory-results.md)
- [Observability and diagnostics](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/observability-and-diagnostics.md)
- [Native AOT](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/native-aot.md)

---

## License

MIT. See [LICENSE](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE).
