# Jeninnet.FileQuery.CommandLine

**Command-line argument binding for [Jeninnet.FileQuery](../Jeninnet.FileQuery/README.md).**

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub Tag](https://img.shields.io/github/v/tag/TarekNajem04/Jeninnet.FileQuery)
![GitHub contributors](https://img.shields.io/github/contributors/TarekNajem04/Jeninnet.FileQuery)
![GitHub forks](https://img.shields.io/github/forks/TarekNajem04/Jeninnet.FileQuery)
![GitHub last commit](https://img.shields.io/github/last-commit/TarekNajem04/Jeninnet.FileQuery)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-closed/tareknajem04/Jeninnet.FileQuery)
[![GitHub stars](https://img.shields.io/github/stars/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/stargazers)
[![GitHub license](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery.CommandLine)
[![NuGet downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery.CommandLine)](https://www.nuget.org/packages/Jeninnet.FileQuery.CommandLine/)



---

## ✨ Repo Stats

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/57d92552dfb25309185f7457c01037a504b5fa24.svg "Repobeats analytics image")

---

This package integrates `Jeninnet.FileQuery` with the standard `System.CommandLine` parser. It automatically maps command-line arguments and flags to structured file-query patterns.

The command-line package produces pattern inputs for the core engine. Runtime observability features such as `IProgress<FileQueryProgress>`, `FileQueryDiagnostic`, and `FileQueryErrorRecoveryOptions` are configured on the resulting core query or execution call.

---

## 🚀 Installation

Install the CLI integration package via NuGet:

```bash
dotnet add package Jeninnet.FileQuery.CommandLine
```

---

## 🛠️ Usage Example

Because [CommandLinePatternOptions](./CommandLinePatternOptions.cs) uses a `protected` constructor to encourage customization, you must subclass it in your application.

### 1. Define CLI Options
```csharp
using System.CommandLine;
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.CommandLine;

// Define a custom class inheriting from CommandLinePatternOptions
public sealed class AppCliOptions : CommandLinePatternOptions
{
    // You can add additional options here if needed
}
```

### 2. Configure Command Routing
```csharp
using System.CommandLine;
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.CommandLine;

var cliOptions  = new AppCliOptions();
var rootCommand = new RootCommand("My custom file analysis tool");

// Register the pattern matching options onto the root command
foreach (var option in cliOptions.GetCommandOptions())
{
    rootCommand.Add(option);
}

rootCommand.SetAction(parseResult =>
{
    // 1. Build pattern dictionaries from the command-line arguments
    var patterns = PatternBuilder.Build(parseResult, cliOptions);
    
    // 2. Pass patterns directly into the fluent builder
    var query = FileQuery.From(@"C:\repo-root")
                         .Where(patterns)
                         .Build();

    var engine = FileQueryRuntime.Create();
    foreach (var file in engine.Execute(query))
    {
        Console.WriteLine(file);
    }
});

await rootCommand.InvokeAsync(args);
```

---

## ⚙️ Supported Command-Line Flags

The following options are exposed by [CommandLinePatternOptions](./CommandLinePatternOptions.cs):

| Option Flag | Alias | Description | Pattern Interpretation |
| --- | --- | --- | --- |
| `--patterns` | `-p` | Semicolon-delimited pattern string. | **Auto-Classified** via [PatternClassifier](../Jeninnet.FileQuery/Patterns/Classification/PatternClassifier.cs) into GitIgnore, Glob, or Regex. |
| `--gitignore` | | Semicolon-delimited exclude pattern list. | Forced to **GitIgnore** syntax. |
| `--glob` | | Semicolon-delimited exclude pattern list. | Forced to **Glob** syntax. |
| `--regex` | | Semicolon-delimited exclude pattern list. | Forced to **Regex** syntax. |

---

## 🧠 Pattern Splitting and Fallback

*   **Semicolon Splitting**: Delimited values (e.g., `-p "*.txt;*.md;!temp.txt"`) are split and trimmed by [PatternSplitter](./PatternSplitter.cs).
*   **Default Fallback**: If no pattern arguments are supplied by the user, the builder defaults to `!**` (GitIgnore syntax: include all files recursively).
*   **Core Observability**: After CLI parsing, call `ExecuteAsync(query, progress, cancellationToken)` or configure `FileQueryBuilder.WithDiagnostics(...)` to inspect runtime traversal and match decisions.

---

## 🚫 Limitations & Notes

*   **System.CommandLine Requirement**: This package depends directly on `System.CommandLine`.
*   **Option Delimiters**: Ensure pattern strings containing semicolons are properly quoted in terminal environments (e.g., `--patterns "*.cs;!bin/"`).
