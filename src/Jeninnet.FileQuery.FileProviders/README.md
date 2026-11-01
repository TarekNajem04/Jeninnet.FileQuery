# Jeninnet.FileQuery.FileProviders

Query `Microsoft.Extensions.FileProviders.IFileProvider` sources with
[Jeninnet.FileQuery](../Jeninnet.FileQuery/README.md). Physical, embedded, and in-memory providers
share the same patterns, filters, metadata, directory results, GitIgnore ingestion, and error
recovery. No files are copied or materialized to disk.

## Installation

```bash
dotnet add package Jeninnet.FileQuery.FileProviders
```

Install the adapter and core from the same release: the adapter uses friend access to the engine's
internal `IFileSystem`, and mixing it with an older core package fails at runtime.

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
query below the provider root.

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

See [Querying `IFileProvider` Sources](../../docs/guides/file-provider-adapter.md) for ASP.NET Core,
dependency injection, and limitations.
