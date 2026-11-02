# MSBuild Validation and SARIF

`Jeninnet.FileQuery.Build` validates `FileQueryPatterns` during MSBuild evaluation and can export
SARIF 2.1.0 diagnostics for GitHub code scanning. It never executes a query.

---

## Installation

```xml
<PackageReference Include="Jeninnet.FileQuery.Build" Version="2.0.0" PrivateAssets="all" />
```

The package contains the MSBuild task, its Tooling SDK dependency, and `build/` targets. It is a
build-time package; it adds no runtime reference.

---

## Property usage

```xml
<PropertyGroup>
    <FileQueryPatterns>**/*.{cs,vb};!bin/</FileQueryPatterns>
    <FileQuerySarifOutput>$(MSBuildProjectDirectory)/artifacts/filequery.sarif</FileQuerySarifOutput>
</PropertyGroup>
```

`FileQueryPatterns` is semicolon-delimited. A malformed pattern fails the build with `JNQ2001`
before `CoreCompile`. Relative SARIF paths resolve against the project directory.

---

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

`SourceFile`, `SourceLine`, and `SourceColumn` are optional. When present, they appear in MSBuild
diagnostics and in SARIF physical locations.

---

## SARIF output

Set `FileQuerySarifOutput` to write a single-run SARIF 2.1.0 document. The rule id is `JNQ2001`;
results include the pattern text as a property and segment index when known.

Upload it from GitHub Actions:

```yaml
- name: Build (writes artifacts/filequery.sarif)
  run: dotnet build -c Release

- name: Upload SARIF
  uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: artifacts/filequery.sarif
```

The `security-events: write` permission is required for code scanning uploads.

---

## Tooling API

The same projection is available without MSBuild:

```csharp
using Jeninnet.FileQuery.Tooling;

var sarif = SarifExporter.ToJson([
    new PatternDiagnostic("src/[abc", "Missing closing bracket.", 4, 1)
]);
```

`SarifExporter` validates diagnostics from `PatternInspector.Validate` and is covered by its own
tests. See the [Build Tooling SDK](tooling-sdk.md) for pattern inspection APIs.
