# Internal Visibility Policy

`InternalsVisibleTo` is currently allowed for:

- `Jeninnet.FileQuery.Tests`, to validate invariants and internal contracts.
- `Jeninnet.FileQuery.Benchmarks`, to benchmark hot internal paths directly.
- `Jeninnet.FileQuery.CommandLine`, to reuse internal parsing and execution seams.
- `Jeninnet.FileQuery.DependencyInjection`, to compose the default engine without widening the public API.
- `Jeninnet.FileQuery.FileProviders`, to adapt an `IFileProvider` to the internal `IFileSystem` contract without exposing internals.

New friend assemblies require a documented reason and must not expose internal types through public APIs.
