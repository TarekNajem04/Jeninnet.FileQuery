# Jeninnet.FileQuery.FileProviders

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery.FileProviders)
![NuGet Downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery.FileProviders?label=Jeninnet.FileQuery.FileProviders)

Query `Microsoft.Extensions.FileProviders.IFileProvider` sources with
[Jeninnet.FileQuery](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/src/Jeninnet.FileQuery/README.md). ASP.NET Core static files, Razor assets,
embedded resources, and custom in-memory providers share the same patterns, metadata filters,
directory results, GitIgnore ingestion, and error recovery as disk queries.

**No files are copied or materialized to disk.**

## Install

```bash
dotnet add package Jeninnet.FileQuery.FileProviders
```

The package references `Microsoft.Extensions.FileProviders.Abstractions`; the core library stays
dependency-free. Install the adapter and core from the same release: the adapter uses friend access
to the engine's internal `IFileSystem`, and mixing it with an older core package fails at runtime.

## Quick start

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
    Console.WriteLine(path);
}
```

`From(provider, root)` returns a normal `FileQueryBuilder`; `root` defaults to `/` and scopes the
query below the provider root:

```csharp
var css = FileProviderQuery.From(provider, "/wwwroot/css")
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.css"])
    .ExecuteEntries();
```

## ASP.NET Core registration

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

## API

| Surface | Behavior |
|---|---|
| `FileProviderQuery.From(provider, root = "/")` | Creates a provider-backed `FileQueryBuilder` |
| `Execute` / `ExecuteAsync` | Path results rooted in virtual path space |
| `ExecuteEntries` / `ExecuteEntriesAsync` | `FileQueryEntry` metadata results |
| `IncludeDirectories` | Yield matching directories |
| `MinSize` / `MaxSize` / `ModifiedAfter` / `ModifiedBefore` | Metadata filters |
| `UseGitIgnoreFiles` | Load `.gitignore` files through the provider |
| `RestrictToPaths` | Diff-aware restrictions |
| `WithErrorRecovery` | Skip/Retry/Abort for provider failures |

## Semantics

- Paths are virtual and use forward slashes. `Execute()` returns rooted paths; `RelativePath` is
  relative to the scoped root.
- `Length` and `LastModified` come from `IFileInfo`. Providers expose one timestamp, so it maps to
  both creation and last-write time.
- Attributes are `Directory` or `Normal`; provider entries do not expose platform attribute flags.
- Directory results, `.gitignore` ingestion, diff-aware restrictions, and all pattern dialects are
  supported.
- `FileQueryBatch` is physical-only; run one query per provider root instead.
- Execute through the builder. Engines created by `FileQueryRuntime` or `AddFileQuery()` are bound
  to the physical file system.

## Related

- [Querying `IFileProvider` sources](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/file-provider-adapter.md)
- [Core package](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/src/Jeninnet.FileQuery/README.md)
- [Tests](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/tests/Jeninnet.FileQuery.FileProviders.Tests/README.md)
