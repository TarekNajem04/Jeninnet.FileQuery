# Jeninnet.FileQuery.CommandLine

Command-line integration for [Jeninnet.FileQuery](https://www.nuget.org/packages/Jeninnet.FileQuery).
Maps `System.CommandLine` arguments directly into file query patterns.

## Installation

```bash
dotnet add package Jeninnet.FileQuery.CommandLine
```

## Usage

```csharp
using System.CommandLine;
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.CommandLine;

var patternOptions = new CommandLinePatternOptions();
var rootCommand    = new RootCommand("My file tool");

foreach (var option in patternOptions.GetCommandOptions())
    rootCommand.Add(option);

rootCommand.SetAction(parseResult =>
{
    var patterns = PatternBuilder.Build(parseResult, patternOptions);
    var engine   = FileQueryRuntime.Create();
    var query    = FileQuery.From(@"C:\repo").Where(patterns).Build();

    foreach (var file in engine.Execute(query))
        Console.WriteLine(file);
});

return await rootCommand.InvokeAsync(args);
```

## Supported options

| Option | Description |
|--------|-------------|
| `--patterns` / `-p` | Semicolon-delimited patterns, auto-classified |
| `--gitignore` | GitIgnore-style patterns |
| `--glob` | Glob-style patterns |
| `--regex` | Regular expression patterns |

## Links

- [Jeninnet.FileQuery](https://www.nuget.org/packages/Jeninnet.FileQuery)
- [Documentation](https://github.com/TarekNajem04/Jeninnet.FileQuery/docs)