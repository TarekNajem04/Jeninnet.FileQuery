# Installation

## Requirements

- .NET 10 SDK or later
- C# 14 (included in .NET 10 SDK)

---

## Core Package

Install the core engine. This is the only package required for basic file querying:

```bash
dotnet add package Jeninnet.FileQuery
```

---

## Optional Packages

### Command-Line Integration

Provides argument parsing support for CLI applications using `System.CommandLine`:

```bash
dotnet add package Jeninnet.FileQuery.CommandLine
```

### Dependency Injection Integration

Registers `IFileQueryEngine` and related services in a `Microsoft.Extensions.DependencyInjection` container:

```bash
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

---

## Package Manager Console (Visual Studio)

```powershell
Install-Package Jeninnet.FileQuery
Install-Package Jeninnet.FileQuery.CommandLine        # optional
Install-Package Jeninnet.FileQuery.DependencyInjection # optional
```

---

## PackageReference (`.csproj`)

```xml
<ItemGroup>
  <PackageReference Include="Jeninnet.FileQuery" Version="1.2.0" />
  <!-- Optional: -->
  <PackageReference Include="Jeninnet.FileQuery.CommandLine" Version="1.2.0" />
  <PackageReference Include="Jeninnet.FileQuery.DependencyInjection" Version="1.2.0" />
</ItemGroup>
```

---

## Verify the Installation

Create a minimal console application:

```csharp
using Jeninnet.FileQuery;

var engine = FileQueryRuntime.Create();
var query  = FileQuery.From(Directory.GetCurrentDirectory()).Build();

foreach (var file in engine.Execute(query))
{
    Console.WriteLine(file);
}
```

Run it:

```bash
dotnet run
```

If the current directory contains files, their paths will be printed to the console.

---

## Symbol Packages

Symbol packages (`.snupkg`) are published alongside each release to NuGet.org. Source Link is enabled, so you can step through Jeninnet.FileQuery source code in the debugger without a local checkout.

Enable source stepping in Visual Studio: **Tools → Options → Debugging → Enable Source Link support**.