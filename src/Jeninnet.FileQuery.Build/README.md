# Jeninnet.FileQuery.Build

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery.Build)
![NuGet Downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery.Build?label=Jeninnet.FileQuery.Build)

Validate `FileQueryPatterns` during `dotnet build` and publish SARIF 2.1.0 diagnostics for GitHub
code scanning. Malformed patterns fail before compilation with `JNQ2001`; no query is executed.

```xml
<PackageReference Include="Jeninnet.FileQuery.Build" Version="2.0.0" PrivateAssets="all" />
```

The package is build-time only: it ships the MSBuild task, the Tooling SDK, and `build/` targets
under `tasks/net10.0/`. It adds no runtime reference to your application.

---

## Property usage

```xml
<PropertyGroup>
    <FileQueryPatterns>**/*.{cs,vb};!bin/</FileQueryPatterns>
    <FileQuerySarifOutput>$(MSBuildProjectDirectory)/artifacts/filequery.sarif</FileQuerySarifOutput>
</PropertyGroup>
```

- `FileQueryPatterns` is semicolon-delimited and uses the engine's normal Hybrid classification.
- Patterns are validated before `CoreCompile`.
- A malformed pattern fails the build with `JNQ2001`.
- A relative `FileQuerySarifOutput` path resolves against the project directory.
- Set `FileQuerySarifOutput` only when you want a SARIF file.

## Item usage and source locations

Use items when patterns come from a props file or need source metadata:

```xml
<ItemGroup>
    <FileQueryPattern Include="src/**/*.cs"
                      SourceFile="build/patterns.props"
                      SourceLine="12"
                      SourceColumn="4" />
</ItemGroup>
```

`SourceFile`, `SourceLine`, and `SourceColumn` are optional. When present they appear in MSBuild
diagnostics and in the SARIF physical location.

## SARIF for code scanning

```yaml
- name: Build (writes artifacts/filequery.sarif)
  run: dotnet build -c Release

- name: Upload SARIF
  uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: artifacts/filequery.sarif
```

The workflow needs `security-events: write` for code-scanning uploads. The SARIF run contains rule
`JNQ2001`, the source pattern as a property, and the segment index when known.

## What the package contains

```text
build/
├── Jeninnet.FileQuery.Build.props
└── Jeninnet.FileQuery.Build.targets
tasks/net10.0/
├── Jeninnet.FileQuery.Build.dll
├── Jeninnet.FileQuery.Tooling.dll
└── Jeninnet.FileQuery.dll
```

The task calls `PatternInspector.Validate` and never traverses the filesystem.

## Limits

- The task validates structural rules shared with the Tooling SDK, not the full runtime compiler.
- Patterns must be known at evaluation time; generated patterns need a custom task or a separate
  validation step.
- The task currently targets the .NET 10 MSBuild host (`dotnet build`).

## Related

- [MSBuild validation and SARIF guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/msbuild-validation.md)
- [Tooling SDK](../Jeninnet.FileQuery.Tooling/README.md)
- [Roslyn analyzer](../Jeninnet.FileQuery.Analyzers/README.md)
