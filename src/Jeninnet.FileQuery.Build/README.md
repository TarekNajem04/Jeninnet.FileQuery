# Jeninnet.FileQuery.Build

MSBuild integration for pattern validation and SARIF code-scanning output.

```xml
<PackageReference Include="Jeninnet.FileQuery.Build" Version="2.0.0" PrivateAssets="all" />

<PropertyGroup>
    <FileQueryPatterns>**/*.{cs,vb};!bin/</FileQueryPatterns>
    <FileQuerySarifOutput>$(MSBuildProjectDirectory)/artifacts/filequery.sarif</FileQuerySarifOutput>
</PropertyGroup>
```

Patterns are validated before `CoreCompile`; malformed patterns fail the build with `JNQ2001`.
Set `FileQuerySarifOutput` to also write a SARIF 2.1.0 file for GitHub code scanning.

For source locations, supply items instead of the property:

```xml
<ItemGroup>
    <FileQueryPattern Include="src/**/*.cs" SourceFile="build/patterns.props" SourceLine="12" />
</ItemGroup>
```

The task uses the Tooling SDK's structural validation and never executes a query.
