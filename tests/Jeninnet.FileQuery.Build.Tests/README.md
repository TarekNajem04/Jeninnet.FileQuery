# Jeninnet.FileQuery.Build.Tests

Tests for the MSBuild validation task and the SARIF 2.0.1 exporter.

**5 tests.**

## What is covered

- Valid patterns return success and produce an empty SARIF run.
- Invalid patterns log `JNQ2001` with file/line/column metadata.
- Empty pattern sets succeed.
- The `FileQueryPatterns` property is split into items and validated by the target.
- The task and Tooling SDK agree on validation semantics.
- SARIF physical locations and result properties.

## Run

```bash
dotnet test tests/Jeninnet.FileQuery.Build.Tests/Jeninnet.FileQuery.Build.Tests.csproj -c Release
```

Two tests invoke `dotnet msbuild` against a temporary project to validate the packaged
`build/` targets end to end.

## Design notes

- `MockBuildEngine` captures MSBuild errors/messages without a full build.
- The target tests use a temporary `.proj` and the task assembly path, so they exercise the same
  `UsingTask` mechanism consumers use.
- No query is executed; validation is purely structural.
- Assertions use the external `Jeninnet.Assertions` package.

## Related

- [MSBuild validation guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/msbuild-validation.md)
- [Build package](../../src/Jeninnet.FileQuery.Build/README.md)
- [All tests](../README.md)
