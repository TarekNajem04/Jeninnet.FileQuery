# Querying `IFileProvider` Sources

`Jeninnet.FileQuery.FileProviders` adapts `Microsoft.Extensions.FileProviders.IFileProvider` to the
engine, so physical, embedded, and in-memory providers share the same patterns, filters, metadata,
directory results, and error-recovery behavior. No files are copied or materialized to disk.

---

## Installation

```bash
dotnet add package Jeninnet.FileQuery.FileProviders
```

The package references `Microsoft.Extensions.FileProviders.Abstractions`; the core library keeps
its zero-dependency contract. Install the adapter and core from the same release: the adapter uses
friend access to the engine's internal `IFileSystem`, and mixing it with an older core package fails
at runtime.

---

## Basic usage

`FileProviderQuery.From` returns a normal `FileQueryBuilder` rooted at the provider:

```csharp
using Jeninnet.FileQuery.Enums;
using Jeninnet.FileQuery.FileProviders;
using Microsoft.Extensions.FileProviders;

using var provider = new PhysicalFileProvider(@"C:\MyProject");

var files = FileProviderQuery.From(provider)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .Execute();

foreach (var path in files)
{
    Console.WriteLine(path); // /src/Program.cs
}
```

Every terminal method is available, including `ExecuteAsync`, `ExecuteEntries`,
`ExecuteEntriesAsync`, cancellation/progress overloads, and `Build()`.

Paths are virtual and use forward slashes. The default root is `/`; pass a subdirectory to scope the
query below the provider root:

```csharp
var css = FileProviderQuery.From(provider, "/wwwroot/css")
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.css"])
    .ExecuteEntries();
```

`Execute()` returns rooted virtual paths (`/wwwroot/css/site.css`), while
`FileQueryEntry.RelativePath` is relative to the scoped root (`site.css`).

---

## Embedded and in-memory providers

The adapter only uses `IFileProvider`, so it works with manifests, embedded resources, and custom
providers without special cases:

```csharp
var provider = new ManifestEmbeddedFileProvider(typeof(Program).Assembly, "wwwroot");

var assets = FileProviderQuery.From(provider)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.css"])
    .Execute();
```

Entry metadata maps provider data as follows:

| Engine value | Provider source |
|---|---|
| `Length` | `IFileInfo.Length` (files only) |
| `LastWriteTimeUtc` | `IFileInfo.LastModified` |
| `CreationTimeUtc` | `IFileInfo.LastModified` (providers expose one timestamp) |
| `Attributes` | `Directory` for directories, `Normal` for files |

Because providers do not expose platform attribute flags, `AttributesToSkip(Hidden | System)` cannot
detect hidden or system entries in a provider-backed query. Size, timestamp, and directory filters
work normally.

---

## ASP.NET Core and dependency injection

Register the provider with the container and inject it where the query runs:

```csharp
builder.Services.AddSingleton<IFileProvider>(_ =>
    new PhysicalFileProvider(builder.Environment.WebRootPath!));

builder.Services.AddScoped<StaticAssetScanner>();
```

```csharp
public sealed class StaticAssetScanner(IFileProvider provider)
{
    public IReadOnlyList<FileQueryEntry> FindStylesheets() =>
        FileProviderQuery.From(provider, "/css")
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.css"])
            .MinSize(1)
            .ExecuteEntries()
            .ToList();
}
```

The builder owns its provider-backed execution engine. Execute through the builder's terminal
methods rather than handing a provider-built `FileQuery` to an `IFileQueryEngine` created by
`FileQueryRuntime` or `AddFileQuery()`; those engines are bound to the physical file system.

---

## Semantics and limitations

- Directory results, GitIgnore ingestion (`.gitignore` files inside the provider), diff-aware path
  restrictions, and all pattern dialects behave as they do on disk.
- `FileQueryBatch` (multiple roots) is not supported with provider-backed builders: it creates a
  physical engine internally. Run one query per root instead.
- `IgnoreInaccessible()` and the error-recovery options apply to provider exceptions that map to
  `IOException` or `UnauthorizedAccessException`.
- Providers expose no asynchronous enumeration API, so `EnumerateAsync` yields synchronously and
  honors cancellation between entries.
- `ResolveRealPath` returns the normalized virtual path because providers do not expose real-path
  or symbolic-link information. Use `FollowWithCycleDetection` with physical providers; other
  providers do not report links.
