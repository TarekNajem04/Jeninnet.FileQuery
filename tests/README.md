# Tests Index

The repository has seven test projects. They are intentionally split by surface area so failures
point at one dependency boundary instead of a monolith.

| Project | Tests | Focus |
|---|---|---|
| [Jeninnet.FileQuery.Tests](./Jeninnet.FileQuery.Tests/README.md) | 1,059 | Core engine, patterns, traversal, metadata, GitIgnore, telemetry, architecture |
| [Jeninnet.FileQuery.Tool.Tests](./Jeninnet.FileQuery.Tool.Tests/README.md) | 54 | `dotnet-filequery` CLI and Git-aware queries |
| [Jeninnet.FileQuery.Tooling.Tests](./Jeninnet.FileQuery.Tooling.Tests/README.md) | 22 | Pattern inspection SDK |
| [Jeninnet.AdvancedUsage.Tests](./Jeninnet.AdvancedUsage.Tests/README.md) | 11 | Dataset generator and evaluation options |
| [Jeninnet.FileQuery.Analyzers.Tests](./Jeninnet.FileQuery.Analyzers.Tests/README.md) | 7 | `JNQ1001` analyzer and code fix |
| [Jeninnet.FileQuery.FileProviders.Tests](./Jeninnet.FileQuery.FileProviders.Tests/README.md) | 8 | `IFileProvider` adapter |
| [Jeninnet.FileQuery.Build.Tests](./Jeninnet.FileQuery.Build.Tests/README.md) | 5 | MSBuild task and SARIF |

**Total: 1,166 tests.**

## Run everything

```bash
dotnet build Jeninnet.FileQuery.slnx -c Release
dotnet test Jeninnet.FileQuery.slnx -c Release --no-build
```

## Quality gates

- Zero warnings (`TreatWarningsAsErrors` is enabled).
- `dotnet format --verify-no-changes`.
- DocFX with warnings as errors.
- Cross-platform CI: Windows, Ubuntu, and macOS.
- Native AOT smoke publish/run on Linux.
- Package validation for every published package.

Tests use MSTest and the external
[`Jeninnet.Assertions`](https://www.nuget.org/packages/Jeninnet.Assertions) package. No external
services, databases, or network access are required.

## Related

- [Root README](../README.md)
- [Samples](../samples/README.md)
- [Contributing](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/CONTRIBUTING.md)

