# Command-Line Integration

`Jeninnet.FileQuery.CommandLine` provides a **thin integration layer** that converts command-line arguments into pattern structures usable by `Jeninnet.FileQuery`.

It is **not a CLI application**.

Instead, it provides:

```
Command-line option definitions
Argument parsing
Pattern classification
Pattern dictionary construction
```

This allows console applications to integrate `Jeninnet.FileQuery` with minimal code.

---

# Architecture Overview

```
CLI Arguments
      │
      ▼
CommandLinePatternOptions
      │
      ▼
System.CommandLine ParseResult
      │
      ▼
CommandLinePatternParser
      │
      ▼
PatternOptions
      │
      ▼
PatternBuilder
      │
      ▼
Dictionary<PatternKind, List<string>>
      │
      ▼
HybridPathMatcher / FileQueryRuntime
```

This design cleanly separates:

```
argument parsing
pattern classification
query execution
```

---

# Core Components

## PatternOptions

Immutable container for parsed CLI values.

```csharp
public record PatternOptions(
    string? Patterns,
    string? Gitignore,
    string? Glob,
    string? RegularExpression
);
```

Fields represent **raw CLI values**.

Patterns are **not parsed at this stage**.

---

## CommandLinePatternOptions

Defines the **command-line options** used by the CLI application.

Example supported options:

```
--patterns
--gitignore
--glob
--regex
```

These map directly to `PatternOptions`.

Example:

```
--patterns "*.txt;!temp.txt"
--regex "^data_.*\\.log$"
```

---

## CommandLinePatternParser

Responsible for converting a `ParseResult` into a `PatternOptions` instance.

```csharp
var patternOptions =
    CommandLinePatternParser.Parse(parseResult, options);
```

No classification or validation occurs here.

The parser only **extracts values**.

---

## PatternBuilder

`PatternBuilder` converts `PatternOptions` into the structure required by the query engine.

```csharp
Dictionary<PatternKind,List<string>>
```

Example:

```
GitIgnore → ["**", "!*.txt"]
Glob      → ["*.cs"]
Regex     → ["^data_.*"]
```

The builder performs:

```
pattern splitting
pattern classification
pattern grouping
fallback handling
```

---

# Pattern Classification

Patterns provided through `--patterns` are automatically classified.

Classification uses:

```
PatternClassifier
```

Example:

```
*.txt         → Glob
**/*.cs       → GitIgnore
r:^data_.*    → Regex
```

The result determines which matcher will process the pattern.

---

# Pattern Splitting

Multiple patterns can be provided in a single option using `;`.

Example:

```
--patterns "*.txt;!temp.txt;data/*.log"
--patterns "r:^data_.*;*.txt;!temp.txt;data/*.log"
```

This becomes:

```
["*.txt", "!temp.txt", "data/*.log"]
```

Splitting is handled by:

```
PatternSpliter.Split(...)
```

---

# Default Behavior

If no patterns are specified, the system defaults to:

```
!**
```

Meaning:

```
include everything
```

This ensures the engine always has a valid rule set.

---

# Example CLI Integration

Minimal example of a console application using the library.

```csharp
using System.CommandLine;
using Jeninnet.FileQuery.CommandLine;

var patternOptions = new CommandLinePatternOptions();

var rootCommand = new RootCommand("File query example");

foreach (var option in patternOptions.GetCommandOptions())
{
    rootCommand.AddOption(option);
}

rootCommand.SetHandler((ParseResult result) =>
{
    var patterns = PatternBuilder.Build(result, patternOptions);

    var engine = FileQueryRuntime.Create();

    var options = new FileQueryOptions
    {
        PatternInput = new(patterns)
    };

    var files = engine.Execute(new(".", options));

    foreach (var file in files)
        Console.WriteLine(file);

});

return rootCommand.InvokeAsync(args);
```

---

# Example Command Usage

### Basic pattern usage

```
app --patterns "*.txt;!temp.txt"
```

---

### GitIgnore syntax

```
app --gitignore "**;!*.log"
```

---

### Glob syntax

```
app --glob "*.cs"
```

---

### Regex syntax

```
app --regex "^data_.*\\.log$"
```

---

# Pattern Language Interoperability

Because patterns are classified, multiple pattern languages can be combined.

Example:

```
--patterns "r:^data_.*;*.cs;!*.Tests.cs"
--regex "^data_.*"
--glob "*.json"
```

The resulting matcher pipeline will be:

```
GitIgnoreMatcher
GlobMatcher
RegexPathMatcher
```

Combined using:

```
HybridPathMatcher
```

---

# Why This Design Works

Advantages:

```
decouples CLI from query engine
supports multiple pattern languages
avoids CLI-specific logic inside the engine
keeps PatternOptions immutable
```

This architecture ensures:

```
CLI applications remain simple
the query engine remains reusable
```

---

# Recommended Usage Pattern

Applications should:

```
1 parse CLI arguments
2 build pattern dictionary
3 configure FileQueryRuntime
4 execute query
```

Example flow:

```
args
  ↓
ParseResult
  ↓
PatternOptions
  ↓
PatternBuilder
  ↓
Pattern dictionary
  ↓
FileQueryRuntime
```