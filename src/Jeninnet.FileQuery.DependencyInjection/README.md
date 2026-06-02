# Jeninnet.FileQuery.DependencyInjection

Dependency injection integration for [Jeninnet.FileQuery](https://www.nuget.org/packages/Jeninnet.FileQuery).
Registers the engine and all internal services with `Microsoft.Extensions.DependencyInjection`.

## Installation

```bash
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

## Registration

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFileQuery();
```

## Injection

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

## Links

- [Jeninnet.FileQuery](https://www.nuget.org/packages/Jeninnet.FileQuery)
- [Documentation](https://github.com/TarekNajem04/Jeninnet.FileQuery/docs)