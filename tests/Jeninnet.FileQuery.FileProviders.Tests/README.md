# Jeninnet.FileQuery.FileProviders.Tests

Tests for the `IFileProvider` adapter: physical and in-memory providers, scoped roots, metadata
filters, directory results, async execution, GitIgnore ingestion, and error recovery.

**8 tests.**

## What is covered

- Physical `PhysicalFileProvider` querying with `FileQueryEntry` metadata.
- A custom in-memory `IFileProvider` with nested files and directories.
- Scoped roots (`/src`) and metadata filters.
- `IncludeDirectories()` and `ExecuteEntriesAsync`.
- `.gitignore` loading through a provider.
- Provider enumeration failures under `Skip` and `Abort`.
- Null provider / empty root argument guards.

## Run

```bash
dotnet test tests/Jeninnet.FileQuery.FileProviders.Tests/Jeninnet.FileQuery.FileProviders.Tests.csproj -c Release
```

## Design notes

- `InMemoryFileProvider` is a minimal provider used to prove portability without materializing files.
- The physical-provider test uses a temporary directory and cleans it up.
- Tests assert virtual paths (`/src/file.cs`) and root-relative paths.
- Assertions use the external `Jeninnet.Assertions` package.

## Related

- [File provider adapter guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/file-provider-adapter.md)
- [FileProviders package](../../src/Jeninnet.FileQuery.FileProviders/README.md)
- [All tests](../README.md)
