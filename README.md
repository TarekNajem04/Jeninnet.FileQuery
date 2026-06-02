# Jeninnet.FileQuery

**A high-performance file discovery and filtering engine for .NET.**

`Jeninnet.FileQuery` is a modern file query system designed for applications that need **precise, deterministic, and scalable file matching** across large directory trees.

## Compatibility

- Target framework: `.NET 10`
- Language: `C# 14`
- Supported platforms: Windows, Linux, and macOS through the .NET runtime
- Packages: `Jeninnet.FileQuery`, `Jeninnet.FileQuery.CommandLine`, and `Jeninnet.FileQuery.DependencyInjection`

It combines multiple pattern languages into a unified matcher architecture while preserving predictable rule semantics inspired by GitIgnore.

The result is a library that is both **powerful and easy to integrate**.

---

# 🚀 Quick Start

Get up and running in seconds using the fluent API.

### Installation
```bash
dotnet add package Jeninnet.FileQuery
```

### Basic Usage
```csharp
using Jeninnet.FileQuery;

// 1. Define your query using the fluent builder
var query = FileQuery.From(@"C:\MyProject")
                     .Where("**")           // Exclude everything
                     .Where("!*.tmp")       // include all .tmp files
                     .Where("!src/**/*.cs") // Only include .cs files in src folder
                     .UsingHybrid()         // Auto-detect pattern types (GitIgnore/Glob/Regex)
                     .IgnoreCase()          // Case-insensitive matching
                     .Build();

// 2. Execute the query
var engine = FileQueryRuntime.Create();
var files = engine.Execute(query).ToList();

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

---

# Why Jeninnet.FileQuery Exists


Most file filtering solutions suffer from at least one of these problems:

* limited pattern syntax
* inconsistent rule semantics
* poor performance on large directory trees
* tight coupling between traversal and pattern logic

`Jeninnet.FileQuery` was designed to address these issues by separating:

```
filesystem traversal
pattern language
matcher execution
query orchestration
```

This separation allows the engine to remain **predictable, extensible, and fast**.

---

# Installation

Install from NuGet:

```
dotnet add package Jeninnet.FileQuery
```

Optional integration packages:

```
dotnet add package Jeninnet.FileQuery.CommandLine
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

---

# Quick Example

```csharp
var fileQueryEngine = FileQueryRuntime.Create();

var options = new FileQueryOptions
{
    PatternInput = new(
        patterns:
        [
            "**",
            "!*.txt"
        ]
    )
};

var result = fileQueryEngine
    .Execute(new(rootPath, options))
    .ToList();
```

This query returns **all files except `.txt` files**.

---

# Deterministic Rule Semantics

The engine follows a simple but powerful rule model inspired by GitIgnore.

Rules are evaluated **in order**, and the **last rule wins**.

Example:

```csharp
patterns:
[
    "**",
    "!*.log",
    "data.log"
]
```

Evaluation:

```
include everything
exclude *.log
include data.log
```

Final result:

```
data.log is included
```

This deterministic model avoids the ambiguity common in many glob engines.

---

# Supported Pattern Languages

The engine supports multiple pattern syntaxes that can be combined within the same query.

See `docs/reference/pattern-semantics.md` for the detailed behavior matrix covering precedence, negation, recursive wildcards, and directory-only rules.

### GitIgnore Patterns

Examples:

```
**
!*.log
!build/
```

Supports:

```
recursive wildcards (**)
negation
ordered rule evaluation
```

---

### Glob Patterns

Examples:

```
*.cs
file?.txt
file[0-9].log
```

Supports:

```
wildcards (*)
single-character matches (?)
character classes
```

---

### Regular Expressions

Regex patterns are prefixed with:

```
r:
```

Example:

```
r:^data_.*\.log$
```

This allows advanced filtering for complex naming schemes.

---

# Hybrid Matcher Architecture

Pattern languages are unified through the **HybridPathMatcher**.

```
                 HybridPathMatcher
                       │
        ┌──────────────┼──────────────┐
        │              │              │
 GitIgnoreMatcher   GlobMatcher   RegexPathMatcher
```

Patterns are classified automatically and routed to the appropriate matcher.

This design allows different pattern systems to coexist without conflict.

---

# Example: Combining Pattern Languages

```csharp
patterns:
[
    "**",
    "!*.tmp",
    "logs/*.log",
    "r:^data_.*"
]
```

This query mixes:

```
GitIgnore rules
Glob rules
Regex rules
```

The hybrid matcher resolves them deterministically.

---

# Command Line Integration

The package:

```
Jeninnet.FileQuery.CommandLine
```

provides a lightweight integration layer for CLI applications.

It converts command-line arguments into pattern structures usable by the query engine.

Example CLI usage:

```
app --patterns "*.txt;!temp.txt"
```

Internally this becomes:

```
PatternOptions
        ↓
PatternBuilder
        ↓
Dictionary<PatternKind,List<string>>
```

Which can then be executed by the engine.

---

# Dependency Injection Integration

The package:

```
Jeninnet.FileQuery.DependencyInjection
```

provides DI container integration.

Example:

```csharp
services.AddFileQuery();
```

This registers the runtime so it can be injected into application services.

---

# Example Use Cases

`Jeninnet.FileQuery` can be used to build:

```
build tools
backup utilities
code analysis tools
file synchronization systems
log processing pipelines
```

Any application that needs reliable file discovery can benefit from it.

---

# Project Structure

```
src/
  Jeninnet.FileQuery
  Jeninnet.FileQuery.CommandLine
  Jeninnet.FileQuery.DependencyInjection

test/
  Jeninnet.FileQuery.Tests

samples/
  BasicMatching
  PatternLanguage
  RecursiveTraversal
  RegexMatching
  HybridMatcher
```

---

# Installation

Install the core package:

```
dotnet add package Jeninnet.FileQuery
```

Optional integrations:

```
dotnet add package Jeninnet.FileQuery.CommandLine
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

---

# Documentation

Full documentation is available in:

```

docs/
    architecture/
    contributing/
    getting-started/
    integrations/
    introduction/
    pattern-language/
    patterns/
    performance/
    runtime/
    specification/
    whitepaper/
```

Topics include:

```
pattern language specification
matcher architecture
pattern tokenization
pattern invariants
filesystem traversal model
```

---

---

# Technology Stack

This project targets modern .NET development.

```
.NET 10
C# 14
MSTest
```

The codebase emphasizes:

* performance-aware design
* minimal allocations
* deep XML documentation
* cross-platform compatibility

---

# Contributing

Contributions are welcome.

Please read:

```
CONTRIBUTING.md
```

before submitting pull requests.

---

# Roadmap

See:

```
ROADMAP.md
```

for future plans including performance improvements and additional matcher capabilities.

---

# License

MIT License.

See `LICENSE` for details.

---

# Final Note

File discovery may appear simple, but once rule sets become complex it can quickly become unpredictable.

Jeninnet.FileQuery was designed to make filesystem querying **deterministic, expressive, and fast** for modern .NET applications.

