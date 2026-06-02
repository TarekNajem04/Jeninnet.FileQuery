# Command-Line Integration

The `Jeninnet.FileQuery.CommandLine` package maps `System.CommandLine` arguments into the pattern structures consumed by the engine. It is not a CLI application — it is a thin integration layer that CLI applications can embed.

---

## Installation

```bash
dotnet add package Jeninnet.FileQuery.CommandLine
```

---

## Core Types

| Type | Responsibility |
|------|---------------|
| `CommandLinePatternOptions` | Defines the CLI option objects (`--patterns`, `--gitignore`, `--glob`, `--regex`) |
| `CommandLinePatternParser` | Extracts raw string values from a `ParseResult` |
| `PatternOptions` | Immutable container for the extracted raw values |
| `PatternBuilder` | Classifies and groups patterns into a `Dictionary<PatternKind, List<string>>` |
| `PatternSpliter` | Splits a semicolon-delimited pattern string into individual patterns |

---

## Minimal Example

```csharp
using System.CommandLine;
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.CommandLine;

var patternOptions = new CommandLinePatternOptions();
var rootCommand    = new RootCommand("File query tool");

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

---

## Supported Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--patterns` | `-p` | Semicolon-delimited patterns; auto-classified |
| `--gitignore` | | Semicolon-delimited GitIgnore-specific patterns |
| `--glob` | | Semicolon-delimited Glob-specific patterns |
| `--regex` | | Semicolon-delimited Regex patterns |

### Example invocations

```bash
# Auto-classified patterns
myapp --patterns "**;!*.exe;!Microsoft*.dll"

# Explicit GitIgnore syntax
myapp --gitignore "**;!src/**/*.cs;bin/;obj/"

# Mixing sources
myapp --gitignore "**;!*.cs" --regex "^src/.*Engine.*"

# Using the alias
myapp -p "*.txt;!temp.txt"
```

---

## Pattern Splitting

All option values are split on `;` and trimmed. Empty entries are discarded:

```
"**.cs ; !temp.cs ;"  →  ["**.cs", "!temp.cs"]
```

Use `PatternSpliter.Split` if you need to split patterns outside the CLI context:

```csharp
IEnumerable<string> patterns = PatternSpliter.Split("*.cs;!temp.cs");
```

A custom separator character can be supplied as a second argument:

```csharp
IEnumerable<string> patterns = PatternSpliter.Split("*.cs|!temp.cs", separator: '|');
```

---

## Classification Behaviour

Patterns supplied via `--patterns` are auto-classified by `PatternClassifier`. Patterns supplied via `--gitignore`, `--glob`, or `--regex` bypass classification and are typed explicitly.

If none of the options are provided, `PatternBuilder.Build` returns a single GitIgnore pattern `!**` — include everything. This ensures a query always has at least one rule.

---

## Advanced Usage

Extend `CommandLinePatternOptions` to add application-specific options alongside the pattern options:

```csharp
public sealed class MyOptions : CommandLinePatternOptions
{
    public Option<string> OutputPath { get; } =
        new Option<string>("--output", "Output directory");

    public override List<Option> GetCommandOptions() =>
    [
        ..base.GetCommandOptions(),
        OutputPath
    ];
}
```

---

## Without System.CommandLine

`PatternBuilder.Build` has an overload that accepts raw strings directly — no `ParseResult` required:

```csharp
var patterns = PatternBuilder.Build(
    patterns:  "**;!*.cs",
    gitignore: "bin/;obj/",
    glob:      null,
    regex:     null
);
```

This is useful for configuration-file-based pattern input or programmatic CLI argument processing.