# Jeninnet.FileQuery.DependencyInjection

**Dependency Injection bindings for [Jeninnet.FileQuery](../Jeninnet.FileQuery/README.md).**

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub Tag](https://img.shields.io/github/v/tag/TarekNajem04/Jeninnet.FileQuery)
![GitHub contributors](https://img.shields.io/github/contributors/TarekNajem04/Jeninnet.FileQuery)
![GitHub forks](https://img.shields.io/github/forks/TarekNajem04/Jeninnet.FileQuery)
![GitHub last commit](https://img.shields.io/github/last-commit/TarekNajem04/Jeninnet.FileQuery)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-closed/tareknajem04/Jeninnet.FileQuery)
[![GitHub stars](https://img.shields.io/github/stars/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/stargazers)
![GitHub License](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery.DependencyInjection)
[![NuGet downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery.DependencyInjection)](https://www.nuget.org/packages/Jeninnet.FileQuery.DependencyInjection/)

---

## ✨ Repo Stats

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/57d92552dfb25309185f7457c01037a504b5fa24.svg "Repobeats analytics image")

---

This package integrates the `Jeninnet.FileQuery` engine into applications using `Microsoft.Extensions.DependencyInjection`. It registers internal parsing, compilation, and execution services.

Registered engines support the Phase 2 observability surface: async progress snapshots, opt-in match diagnostics, cancellation propagation, and configurable IO recovery strategies.

---

## 🚀 Installation

Install the Dependency Injection integration package via NuGet:

```bash
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

---

## 🛠️ Usage Example

### 1. Service Registration

Register the subsystem on startup using [ServiceCollectionExtensions.AddFileQuery](./Extensions/ServiceCollectionExtensions.cs):

```csharp
using Microsoft.Extensions.Hosting;
using Jeninnet.FileQuery.DependencyInjection.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Registers the FileQuery engine and traversal services
builder.Services.AddFileQuery();

using var host = builder.Build();
await host.RunAsync();
```

### 2. Service Consumption

Inject [IFileQueryEngine](../Jeninnet.FileQuery/IFileQueryEngine.cs) into your services:

```csharp
using Jeninnet.FileQuery;

public sealed class ProjectScanner
{
    private readonly IFileQueryEngine _engine;

    public ProjectScanner(IFileQueryEngine engine)
    {
        _engine = engine;
    }

    public List<string> ScanRepository(string rootDirectory)
    {
        // Construct the query parameters
        var query = FileQuery.From(rootDirectory)
                             .Where("**", "!src/**/*.cs") // Ignore all except C# in src/
                             .Build();

        // Execute queries using the injected engine
        return _engine.Execute(query).ToList();
    }
}
```

### 3. Progress and Diagnostics with DI

```csharp
var progress = new Progress<FileQueryProgress>(snapshot =>
{
    Console.WriteLine($"{snapshot.EntriesScanned} entries scanned");
});

var diagnostics = new Progress<FileQueryDiagnostic>(entry =>
{
    Console.WriteLine($"{entry.RelativePath}: {entry.Outcome}");
});

var query = FileQuery.From(rootDirectory)
                     .Where("**", "!src/**/*.cs")
                     .WithDiagnostics(diagnostics)
                     .WithErrorRecovery(FileQueryErrorRecoveryOptions.Retry(2))
                     .Build();

await foreach (var file in _engine.ExecuteAsync(query, progress, cancellationToken))
{
    Console.WriteLine(file);
}
```

---

## ⚙️ Service Registrations & Lifetimes

Calling `AddFileQuery()` registers the following components in the container:

| Registered Interface | Concrete Type | Lifetime | Purpose |
| --- | --- | --- | --- |
| [IFileQueryEngine](../Jeninnet.FileQuery/IFileQueryEngine.cs) | `FileQueryEngine` | `Singleton` | Primary engine executing queries. |
| `IFileSystem` | `FileSystem` | `Singleton` | Decoupled platform IO actions. |
| `ITraversalPlanBuilder` | `TraversalPlanBuilder` | `Singleton` | Prepares traversal steps. |
| `ITraversalExecutor` | `TraversalExecutor` | `Singleton` | Drives physical/virtual path discovery. |
| `IPatternCompilerRegistry` | `PatternCompilerRegistry` | `Singleton` | Manages compilers for different dialects. |
| `PatternInvariantRegistry` | `PatternInvariantRegistry` | `Singleton` | Regulates dialect checks (e.g., wildcards, ranges). |
| `PatternPipeline` | `PatternPipeline` | `Singleton` | Compiles raw inputs to matcher instructions. |

---

## 🚫 Limitations

*   **Singleton Lifetime**: Registers matching components as Singletons. If your application requires transient mock filesystems or custom per-request traversal behaviors, register them manually instead of using `AddFileQuery()`.
