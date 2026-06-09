# Jeninnet.FileQuery

Building a Deterministic File Query Engine for .NET
Technical Whitepaper
Version 1.0
Author: Tarek Najem
GitHub: <https://github.com/TarekNajem04/Jeninnet.FileQuery>
NuGet: <https://www.nuget.org/packages/Jeninnet.FileQuery>
License: MIT

<div style="page-break-after: always;"></div>

## Table of Contents

- [Jeninnet.FileQuery](#jeninnetfilequery)
  - [Table of Contents](#table-of-contents)
  - [1. Introduction](#1-introduction)
  - [2. The Problem With Traditional File Matching](#2-the-problem-with-traditional-file-matching)
    - [2.1 Glob-only libraries](#21-glob-only-libraries)
    - [2.2 Regular expression libraries](#22-regular-expression-libraries)
    - [2.3 GitIgnore-inspired libraries](#23-gitignore-inspired-libraries)
  - [3. Deterministic Rule Evaluation](#3-deterministic-rule-evaluation)
  - [4. The Pattern Language](#4-the-pattern-language)
    - [4.1 GitIgnore Patterns](#41-gitignore-patterns)
    - [4.2 Glob Patterns](#42-glob-patterns)
    - [4.3 Regular Expression Patterns](#43-regular-expression-patterns)
    - [4.4 POSIX Character Classes](#44-posix-character-classes)
  - [5. Architecture](#5-architecture)
    - [5.1 Separation of Concerns](#51-separation-of-concerns)
    - [5.2 The Compilation Pipeline](#52-the-compilation-pipeline)
    - [5.3 The HybridPathMatcher](#53-the-hybridpathmatcher)
    - [5.4 Traversal](#54-traversal)
    - [5.5 Path Normalization](#55-path-normalization)
  - [6. Performance](#6-performance)
    - [6.1 Zero-Allocation Hot Path](#61-zero-allocation-hot-path)
    - [6.2 Benchmark Results](#62-benchmark-results)
    - [6.3 Compilation Pipeline Allocations](#63-compilation-pipeline-allocations)
  - [7. Getting Started](#7-getting-started)
    - [7.1 Installation](#71-installation)
    - [7.2 Basic Usage](#72-basic-usage)
    - [7.3 Pattern-Based Filtering](#73-pattern-based-filtering)
    - [7.4 Hybrid Pattern Mixing](#74-hybrid-pattern-mixing)
    - [7.5 Async Enumeration](#75-async-enumeration)
    - [7.6 Dependency Injection](#76-dependency-injection)
    - [7.7 Command-Line Integration](#77-command-line-integration)
  - [8. Package Reference](#8-package-reference)
    - [Supported Targets](#supported-targets)
  - [9. Design Goals and Non-Goals](#9-design-goals-and-non-goals)
    - [Goals](#goals)
    - [Non-Goals](#non-goals)
  - [10. Conclusion](#10-conclusion)

<div style="page-break-after: always;"></div>

## 1. Introduction

Every non-trivial software system eventually encounters the same deceptively simple task: finding files.
Build systems search for source files. Backup tools scan directories to determine what has changed.
Code analyzers walk entire repositories. Log processors filter terabytes of archived data.

At first glance, file discovery appears trivial. Operating systems provide directory enumeration APIs, and many environments include globbing utilities.
But once a project grows, developers discover deeper issues:

- Pattern languages behave inconsistently.
- Traversal becomes expensive at scale.
- Rule ordering is unclear.
- Pattern syntaxes cannot easily coexist.

These challenges led to the creation of Jeninnet.FileQuery, a library that treats file discovery as a first-class architectural problem.

<div style="page-break-after: always;"></div>

## 2. The Problem With Traditional File Matching

Most libraries approach file matching from one of three directions.

### 2.1 Glob-only libraries

Glob patterns are simple and familiar.
However, limitations appear when rule sets grow:

- No rule ordering
- No negation
- No mixing with regex
- No hierarchical semantics

### 2.2 Regular expression libraries

Regex is expressive but not suited for hierarchical filesystem rules.
Patterns become unreadable and difficult to maintain.

### 2.3 GitIgnore-inspired libraries

GitIgnore introduces:

- Rule ordering
- Negation
- Directory-aware semantics

But these libraries rarely allow mixing GitIgnore, Glob, and Regex in the same rule set.

The deeper issue is not syntax but evaluation order.
Ambiguities lead to unpredictable results.

<div style="page-break-after: always;"></div>

## 3. Deterministic Rule Evaluation

**Jeninnet.FileQuery** adopts a simple and explicit rule model:

Patterns are evaluated sequentially, and the last matching rule determines the final result.

Example rule set:

```
**           # Exclude everything
!*.log       # Include all .log files
data.log     # Exclude this specific file again
```

Evaluation:

- Rule 1: exclude all files
- Rule 2: include files ending in .log
- Rule 3: exclude data.log specifically

Final result:

- data.log is excluded
- all other .log files are included
- all other files remain excluded

This deterministic model eliminates ambiguity and ensures predictable behavior.

<div style="page-break-after: always;"></div>

## 4. The Pattern Language

Jeninnet.FileQuery supports three pattern dialects that can coexist in the same rule set:

- GitIgnore patterns
- Glob patterns
- Regular expressions

The engine automatically classifies each pattern and routes it to the correct matcher.

### 4.1 GitIgnore Patterns

GitIgnore patterns are the default and most expressive for hierarchical rules.

| Syntax | Meaning |
|--------|---------|
| ** | Match zero or more path segments |
| * | Match any characters within one segment |
| ? | Match exactly one character |
| ! | Negate the pattern |
| /pattern | Anchor to root |
| pattern/ | Directory-only |
| [abc] | Character set |
| [a-z] | Character range |
| [!abc] | Negated set |
| [[:digit:]] | POSIX digit class |

Example:

```txt
**               # exclude everything
!src/**/.cs      # include C# files under src
src/obj/**       # exclude obj
src/bin/**       # exclude bin
```

### 4.2 Glob Patterns

Glob patterns follow classical Unix rules and are always anchored.

Examples:

```txt
*.cs
**/*.cs
data/??.log
report.[0-9].txt
```

Negation is not supported in glob patterns.

### 4.3 Regular Expression Patterns

Regex patterns are prefixed with `r:` and evaluated against the full normalized path.

Examples:

```txt
r:^src/.*\.cs$
r:^data_\d{4}\.log$
r:^(?!.*test).*\.dll$
```

### 4.4 POSIX Character Classes

Supported inside `[: :]`:

| Class | Matches |
|--------|---------|
| [:digit:] | 0–9 |
| [:alpha:] | a–z, A–Z |
| [:alnum:] | digits and letters |
| [:space:] | whitespace |
| [:upper:] | A–Z |
| [:lower:] | a–z |
| [:xdigit:] | hex digits |
| [:punct:] | punctuation |

Example:

```txt
**
![[:digit:]]*.txt
```

<div style="page-break-after: always;"></div>

## 5. Architecture

### 5.1 Separation of Concerns

The engine separates:

- Pattern compilation
- Filesystem traversal
- Matching execution

Each layer is isolated and enforced by architecture tests.

### 5.2 The Compilation Pipeline

Patterns pass through four phases:

| Phase | Responsibility |
|--------|----------------|
| Lexical invariant | Validate raw text |
| PatternScanner | Tokenize into tokens |
| Structural invariants | Validate token structure |
| Semantic invariants | Apply dialect transforms |

The scanner is purely lexical; semantics are applied later for clarity and testability.

### 5.3 The HybridPathMatcher

The matcher coordinates three sub-matchers:

- GitIgnoreInstructionMatcher
- GlobInstructionMatcher
- RegexInstructionMatcher

Routing is precomputed, so evaluation is fast and allocation-free.

### 5.4 Traversal

Supports:

- Depth-first traversal
- Breadth-first traversal

Options include recursion depth, symlink policy, case sensitivity, and error handling.

### 5.5 Path Normalization

All paths are normalized:

- Forward slashes
- Collapsed duplicates
- Preserved UNC roots
- Uppercased drive letters

This ensures cross-platform consistency.

<div style="page-break-after: always;"></div>

## 6. Performance

Jeninnet.FileQuery is designed for high performance, especially in large directory trees and complex rule sets.

### 6.1 Zero-Allocation Hot Path

The engine ensures that pattern matching produces zero heap allocations in the hot path.

Before optimization:

```csharp
foreach (var pattern in patterns)
{
    ...
}
```

This caused boxing of the enumerator (~40 bytes per evaluation).

After optimization:

```csharp
for (var i = 0; i < patterns.Count; i++)
{
    var pattern = patterns[i];
    ...
}
```

This eliminates all allocations during matching.

### 6.2 Benchmark Results

Environment:
Intel Core i7-8850H 2.60 GHz
.NET 10.0.5
Windows 11
BenchmarkDotNet v0.15.8

| Component | Mean | Allocated |
|----------|------|-----------|
| PatternClassifier | 64 ns | 0 B |
| GlobMatcher | 261 ns | 0 B |
| RegexMatcher | 85 ns | 0 B |
| GitIgnoreMatcher | 771 ns | 0 B |
| HybridMatcher | 742 ns | 0 B |
| PatternTokenizer | 4.85 µs | ~9 KB (one-time) |
| Traversal (QueryFiles) | 2.0 ms | ~30 KB |

Most allocations come from returning actual file paths, not from the engine itself.

### 6.3 Compilation Pipeline Allocations

When the caller specifies a PatternKind explicitly, the engine skips classification and reduces allocations by ~400–500 bytes per pattern.

Sub-lists for pattern kinds are allocated lazily.
If only GitIgnore patterns are used, no Glob or Regex lists are created.

<div style="page-break-after: always;"></div>

## 7. Getting Started

### 7.1 Installation

Install the core package:

```powershell
dotnet add package Jeninnet.FileQuery
```

Optional packages:

```powershell
dotnet add package Jeninnet.FileQuery.CommandLine
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

### 7.2 Basic Usage

```csharp
var engine = FileQueryRuntime.Create();

var query = FileQuery.From(@"C:\repo")
                     .Build();

foreach (var file in engine.Execute(query))
{
    Console.WriteLine(file);
}
```

### 7.3 Pattern-Based Filtering

```csharp
var query = FileQuery.From(@"C:\repo")
                     .Where(
                         "**",
                         "!src/**/*.cs",
                         "src/obj/**",
                         "src/bin/**"
                     )
                     .Build();

var results = engine.Execute(query).ToList();
```

### 7.4 Hybrid Pattern Mixing

```csharp
var query = FileQuery.From(@"C:\repo")
                     .UsingHybrid()
                     .Where(
                         "**",
                         "!*Global*.cs",
                         "r:^src/.*Engine.*"
                     )
                     .IgnoreCase()
                     .Build();
```

### 7.5 Async Enumeration

```csharp
await foreach (var file in engine.ExecuteAsync(query, cancellationToken))
{
    await ProcessFileAsync(file, cancellationToken);
}
```

### 7.6 Dependency Injection

```csharp
builder.Services.AddFileQuery();

public class FileScanner(IFileQueryEngine engine)
{
    public IEnumerable<string> Scan(string root)
        => engine.Execute(FileQuery.From(root).Build());
}
```

### 7.7 Command-Line Integration

```csharp
// Usage: myapp --patterns "**;!*.exe" --gitignore "bin/;obj/"

var options = new CommandLinePatternOptions();
var rootCmd = new RootCommand("File scanner");

foreach (var opt in options.GetCommandOptions())
    rootCmd.Add(opt);

rootCmd.SetAction(result =>
{
    var patterns = PatternBuilder.Build(result, options);
    var query = FileQuery.From(root).Where(patterns).Build();

    foreach (var file in engine.Execute(query))
        Console.WriteLine(file);
});
```

<div style="page-break-after: always;"></div>

## 8. Package Reference

| Package | Description |
|---------|-------------|
| Jeninnet.FileQuery | Core engine. GitIgnore, Glob, Regex. Zero-allocation matching. |
| Jeninnet.FileQuery.CommandLine | Maps System.CommandLine results to pattern options. |
| Jeninnet.FileQuery.DependencyInjection | Registers IFileQueryEngine for DI containers. |

### Supported Targets

All packages target:

```txt
net10.0
```

Symbol packages and Source Link are enabled.

<div style="page-break-after: always;"></div>

## 9. Design Goals and Non-Goals

### Goals

- Deterministic behavior
- Composable pattern dialects
- Zero-allocation hot path
- Streaming traversal
- Cross-platform normalization
- Strong architectural boundaries
- Extensible compiler and matcher pipeline

### Non-Goals

- File content inspection
- Parallel traversal (planned for v1.1)
- Mutable filesystem operations
- Pattern caching across queries (planned for v1.1)

<div style="page-break-after: always;"></div>

## 10. Conclusion

File discovery becomes complex at scale.
Jeninnet.FileQuery solves this by:

- Providing deterministic rule evaluation
- Supporting mixed pattern dialects
- Ensuring high performance with zero allocations
- Offering a clean, extensible architecture

Try it:

```powershell
dotnet add package Jeninnet.FileQuery
```

GitHub: github.com/TarekNajem04/Jeninnet.FileQuery
NuGet: nuget.org/packages/Jeninnet.FileQuery
License: MIT
