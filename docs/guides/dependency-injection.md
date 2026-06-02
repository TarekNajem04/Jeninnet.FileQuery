# Dependency Injection

The `Jeninnet.FileQuery.DependencyInjection` package registers the engine and all its internal services with a `Microsoft.Extensions.DependencyInjection` container.

---

## Installation

```bash
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

---

## Registration

Call `AddFileQuery()` on `IServiceCollection` during application startup:

```csharp
// ASP.NET Core / Generic Host
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFileQuery();
```

```csharp
// Manual container setup
var services = new ServiceCollection();
services.AddFileQuery();
var provider = services.BuildServiceProvider();
```

---

## Registered Services

| Service | Lifetime | Implementation |
|---------|----------|---------------|
| `IFileQueryEngine` | Singleton | `FileQueryEngine` |
| `ITraversalPlanBuilder` | Singleton | `TraversalPlanBuilder` |
| `ITraversalExecutor` | Singleton | `TraversalExecutor` |
| `PatternInvariantRegistry` | Singleton | Default invariant set |
| `IPatternCompilerRegistry` | Singleton | `PatternCompilerRegistry` |
| `PatternPipeline` | Singleton | Default pipeline |

All services are singletons. The engine carries no per-request state — concurrent requests share one `IFileQueryEngine` instance safely.

---

## Injecting the Engine

```csharp
public sealed class ProjectScanner(IFileQueryEngine engine)
{
    public IReadOnlyList<string> FindSourceFiles(string root)
    {
        var query = FileQuery.From(root)
                             .Where("**", "!src/**/*.cs")
                             .Build();

        return engine.Execute(query).ToList();
    }
}
```

Register and inject:

```csharp
services.AddFileQuery();
services.AddScoped<ProjectScanner>();
```

---

## Async Usage in a Background Service

```csharp
public sealed class FileSyncService(IFileQueryEngine engine) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var query = FileQuery.From(@"/data/incoming")
                             .Where("**", "!*.csv")
                             .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var file in engine.ExecuteAsync(query, stoppingToken))
            {
                await ProcessAsync(file, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

---

## Advanced Usage Sample

The `samples/AdvancedUsage` project demonstrates full DI integration including the CommandLine package:

```csharp
builder.Services.AddFileQuery();
builder.Services.AddTransient<IFileQueryCommand, FileQueryCommand>();
builder.Services.AddSingleton<IPrinter, ConsolePrinter>();
```

`FileQueryCommand` receives `IFileQueryEngine` through constructor injection, parses CLI arguments using the CommandLine package, and prints results using `IPrinter`.

---

## Without the DI Package

If you prefer not to take a dependency on `Microsoft.Extensions.DependencyInjection`, use `FileQueryRuntime.Create()` directly:

```csharp
// No DI package needed
IFileQueryEngine engine = FileQueryRuntime.Create();
```

Both approaches produce an identical engine. `AddFileQuery()` is a convenience for applications that already use the Microsoft DI container.