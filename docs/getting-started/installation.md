# Installation

## Requirements

- .NET 10 SDK or later
- C# 14 (included in the .NET 10 SDK)

---

## Core package

The core engine is the only package required for basic file querying:

```bash
dotnet add package Jeninnet.FileQuery
```

```csharp
using Jeninnet.FileQuery;

var files = FileQuery.From(Directory.GetCurrentDirectory()).Execute();

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

`Execute()` builds and runs with the default engine. For explicit composition, use
`Build()` + `FileQueryRuntime.Create()`.

---

## Optional packages

| Package | Install | Use it for |
|---|---|---|
| `Jeninnet.FileQuery.CommandLine` | `dotnet add package Jeninnet.FileQuery.CommandLine` | `System.CommandLine` pattern options |
| `Jeninnet.FileQuery.DependencyInjection` | `dotnet add package Jeninnet.FileQuery.DependencyInjection` | `AddFileQuery()` registration |
| `Jeninnet.FileQuery.FileProviders` | `dotnet add package Jeninnet.FileQuery.FileProviders` | ASP.NET Core / in-memory `IFileProvider` |
| `Jeninnet.FileQuery.Tooling` | `dotnet add package Jeninnet.FileQuery.Tooling` | Pattern inspection without execution |
| `Jeninnet.FileQuery.Analyzers` | `dotnet add package Jeninnet.FileQuery.Analyzers` | Design-time `JNQ1001` diagnostics |
| `Jeninnet.FileQuery.Build` | `dotnet add package Jeninnet.FileQuery.Build` | MSBuild validation and SARIF |
| `Jeninnet.FileQuery.Tool` | `dotnet tool install --global Jeninnet.FileQuery.Tool` | `dotnet-filequery` terminal tool |

The analyzer and build packages are build-time dependencies; use `PrivateAssets="all"`.

Test projects consume the standalone
[`Jeninnet.Assertions`](https://www.nuget.org/packages/Jeninnet.Assertions) package.

---

## Package Manager Console (Visual Studio)

```powershell
Install-Package Jeninnet.FileQuery
Install-Package Jeninnet.FileQuery.CommandLine        # optional
Install-Package Jeninnet.FileQuery.DependencyInjection # optional
Install-Package Jeninnet.FileQuery.FileProviders      # optional
```

---

## PackageReference (`.csproj`)

```xml
<ItemGroup>
  <PackageReference Include="Jeninnet.FileQuery" Version="2.0.0" />
  <!-- Optional: -->
  <PackageReference Include="Jeninnet.FileQuery.CommandLine" Version="2.0.0" />
  <PackageReference Include="Jeninnet.FileQuery.DependencyInjection" Version="2.0.0" />
  <PackageReference Include="Jeninnet.FileQuery.FileProviders" Version="2.0.0" />
</ItemGroup>
```

For build-time packages:

```xml
<ItemGroup>
  <PackageReference Include="Jeninnet.FileQuery.Analyzers" Version="2.0.0" PrivateAssets="all" />
  <PackageReference Include="Jeninnet.FileQuery.Build" Version="2.0.0" PrivateAssets="all" />
</ItemGroup>
```

---

## Verify the installation

Create a minimal console application:

```csharp
using Jeninnet.FileQuery;

var files = FileQuery.From(Directory.GetCurrentDirectory()).Execute();

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

Run it:

```bash
dotnet run
```

If the current directory contains files, their absolute paths are printed.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `DirectoryNotFoundException` on `Build()` | Root does not exist or is relative to the wrong directory | Use an absolute path or verify the working directory |
| `PatternException` with `Position`/`Segment` | Malformed pattern | Fix the reported character; see [Error handling](../guides/error-handling-and-validation.md) |
| No files returned | `**` excludes everything and no later rule re-includes | Add a negated pattern such as `!**/*.cs` |
| `.gitignore` rules ignored | Ingestion is opt-in | Call `UseGitIgnoreFiles()` or pass `--ignore-files` |
| `IFileProvider` source returns nothing | Adapter uses virtual rooted paths | Start at `/` or the correct provider subpath |

---

## Symbol packages

Symbol packages (`.snupkg`) are published alongside each release. The package includes XML
documentation and source metadata for supported build configurations.

---

## Next

- [Quick start](quick-start.md) — build your first real query.
- [Basic patterns](basic-patterns.md) — the pattern language.
- [Common recipes](common-recipes.md) — ready-made queries.
