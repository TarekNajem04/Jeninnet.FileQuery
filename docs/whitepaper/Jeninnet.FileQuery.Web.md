# Building a Deterministic File Query Engine for .NET

## How Jeninnet.FileQuery Combines GitIgnore, Glob, and Regex Into a Predictable High-Performance Query Engine

**Author:** Tarek Najem

**GitHub:** (https://github.com/TarekNajem04/Jeninnet.FileQuery)[https://github.com/TarekNajem04/Jeninnet.FileQuery]

**NuGet:** (https://www.nuget.org/packages/Jeninnet.FileQuery)[https://www.nuget.org/packages/Jeninnet.FileQuery]

**Documentation:** (https://tareknajem04.github.io/Jeninnet.FileQuery/)[https://tareknajem04.github.io/Jeninnet.FileQuery/]

**License:** MIT

---

## Contents

* Introduction
* The Problem With Traditional File Matching

  * Glob-only Libraries
  * Regular Expression Libraries
  * GitIgnore-Inspired Libraries
* Deterministic Rule Evaluation
* The Pattern Language

  * GitIgnore Patterns
  * Glob Patterns
  * Regular Expression Patterns
  * POSIX Character Classes
* Architecture

  * Separation of Concerns
  * Compilation Pipeline
  * HybridPathMatcher
  * Traversal
  * Path Normalization
* Performance

  * Zero-Allocation Hot Path
  * Benchmark Results
  * Compilation Pipeline Allocations
* Getting Started
* Package Reference
* Design Goals and Non-Goals
* Conclusion

---

## Introduction

Every non-trivial software system eventually encounters the same deceptively simple task: finding files.

Build systems search for source files. Backup tools scan directories to determine what has changed. Code analyzers walk entire repositories. Log processors filter terabytes of archived data.

At first glance, file discovery appears trivial. Operating systems provide directory enumeration APIs, and many environments include globbing utilities. As projects grow, however, several problems emerge:

* Pattern languages behave inconsistently.
* Traversal becomes expensive at scale.
* Rule ordering is unclear.
* Multiple pattern syntaxes cannot easily coexist.

Jeninnet.FileQuery was created to address these challenges by treating file discovery as a first-class architectural concern rather than a utility function.

## The Problem With Traditional File Matching

Most file matching libraries fall into one of three categories.

### Glob-only Libraries

Advantages:

* Simple
* Familiar
* Widely supported

Limitations:

* No rule ordering
* No negation
* No regex integration
* No hierarchical semantics

### Regular Expression Libraries

Regular expressions are extremely expressive but become difficult to maintain when modeling hierarchical filesystem rules.

Typical issues include:

* Reduced readability
* Complex maintenance
* Difficult onboarding for teams

### GitIgnore-Inspired Libraries

GitIgnore semantics introduce powerful concepts:

* Rule ordering
* Negation
* Directory awareness

However, most implementations force users into a single pattern dialect and do not allow seamless integration with glob and regex patterns.

The real challenge is not syntax—it is deterministic evaluation.

## Deterministic Rule Evaluation

Jeninnet.FileQuery adopts a simple rule model:

> The last matching rule determines the final outcome.

Example:

```txt
**
!*.log
data.log
```

Evaluation order:

1. Exclude everything.
2. Include all `.log` files.
3. Exclude `data.log`.

Final result:

* `data.log` → excluded
* Other `.log` files → included
* All remaining files → excluded

This approach removes ambiguity and produces predictable results.

## The Pattern Language

The engine supports three pattern dialects within the same rule set:

* GitIgnore patterns
* Glob patterns
* Regular expressions

The compiler automatically detects and routes each pattern to the appropriate matcher.

### GitIgnore Patterns

Supported syntax:

* `**` — Match zero or more path segments
* `*` — Match any characters within a segment
* `?` — Match exactly one character
* `!` — Negation
* `/pattern` — Root anchored
* `pattern/` — Directory only
* `[abc]` — Character set
* `[a-z]` — Character range
* `[!abc]` — Negated set
* `[[:digit:]]` — POSIX character class

Example:

```txt
**
!src/**/*.cs
src/obj/**
src/bin/**
```

### Glob Patterns

Examples:

```txt
*.cs
**/*.cs
data/??.log
report.[0-9].txt
```

Characteristics:

* Unix-style matching
* Root anchored
* No negation support

### Regular Expression Patterns

Regex patterns are prefixed with `r:`.

Examples:

```txt
r:^src/.*\.cs$
r:^data_\d{4}\.log$
r:^(?!.*test).*\.dll$
```

### POSIX Character Classes

Supported classes include:

* `[:digit:]`
* `[:alpha:]`
* `[:alnum:]`
* `[:space:]`
* `[:upper:]`
* `[:lower:]`
* `[:xdigit:]`
* `[:punct:]`

Example:

```txt
**
![[:digit:]]*.txt
```

## Architecture

### Separation of Concerns

The architecture separates:

* Pattern compilation
* Filesystem traversal
* Matching execution

Each layer can evolve independently and is enforced through architecture tests.

### Compilation Pipeline

Compilation consists of four stages:

1. Lexical Invariant

   * Validate raw pattern text.

2. PatternScanner

   * Tokenize input.

3. Structural Invariants

   * Validate token structure.

4. Semantic Invariants

   * Apply dialect-specific transformations.

### HybridPathMatcher

The matching layer coordinates:

* GitIgnoreInstructionMatcher
* GlobInstructionMatcher
* RegexInstructionMatcher

Routing decisions are precomputed during compilation to keep runtime matching fast and allocation-free.

### Traversal

Supported traversal modes:

* Depth-first
* Breadth-first

Additional options:

* Maximum depth
* Symlink handling
* Case sensitivity
* Error behavior

### Path Normalization

Normalization guarantees cross-platform consistency:

* Forward slashes
* Duplicate separator collapse
* UNC preservation
* Upper-case drive letters

## Performance

### Zero-Allocation Hot Path

The matching engine performs no heap allocations during evaluation.

Before optimization:

```csharp
foreach (var pattern in patterns)
{
    ...
}
```

After optimization:

```csharp
for (var i = 0; i < patterns.Count; i++)
{
    var pattern = patterns[i];
    ...
}
```

This eliminates enumerator boxing and allocation overhead.

### Benchmark Results

Environment:

* Intel Core i7-8850H
* Windows 11
* .NET 10
* BenchmarkDotNet 0.15.8

Selected results:

* PatternClassifier — 64 ns, 0 B
* GlobMatcher — 261 ns, 0 B
* RegexMatcher — 85 ns, 0 B
* GitIgnoreMatcher — 771 ns, 0 B
* HybridMatcher — 742 ns, 0 B

Most memory allocations originate from returned file paths rather than the matching engine itself.

### Compilation Pipeline Allocations

When PatternKind is specified explicitly:

* Pattern classification is skipped.
* Approximately 400–500 bytes are saved per pattern.

Collections for pattern categories are allocated lazily.

## Getting Started

### Installation

```powershell
dotnet add package Jeninnet.FileQuery
```

Optional packages:

```powershell
dotnet add package Jeninnet.FileQuery.CommandLine
dotnet add package Jeninnet.FileQuery.DependencyInjection
```

### Basic Usage

```csharp
var engine = FileQueryRuntime.Create();

var query = FileQuery.From(@"C:\repo")
                     .Build();

foreach (var file in engine.Execute(query))
{
    Console.WriteLine(file);
}
```

### Pattern-Based Filtering

```csharp
var query = FileQuery.From(@"C:\repo")
                     .Where(
                         "**",
                         "!src/**/*.cs",
                         "src/obj/**",
                         "src/bin/**"
                     )
                     .Build();
```

### Hybrid Pattern Mixing

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

## Package Reference

Packages:

* Jeninnet.FileQuery — Core engine
* Jeninnet.FileQuery.CommandLine — Command-line integration
* Jeninnet.FileQuery.DependencyInjection — Dependency injection support

Supported target:

```txt
net10.0
```

## Design Goals and Non-Goals

### Goals

* Deterministic behavior
* Composable pattern dialects
* Zero-allocation hot path
* Streaming traversal
* Cross-platform normalization
* Strong architectural boundaries
* Extensible compilation pipeline

### Non-Goals

* File content inspection
* Mutable filesystem operations
* Pattern caching across queries
* Parallel traversal (planned)

## Conclusion

File discovery becomes surprisingly complex at scale.

Jeninnet.FileQuery addresses this challenge through:

* Deterministic rule evaluation
* Mixed pattern dialect support
* Zero-allocation matching
* A modular architecture

The result is a predictable and high-performance file query engine designed specifically for modern .NET applications.

Try it today:

```powershell
dotnet add package Jeninnet.FileQuery
```

**GitHub:** (https://github.com/TarekNajem04/Jeninnet.FileQuery)[https://github.com/TarekNajem04/Jeninnet.FileQuery]

**NuGet:** (https://www.nuget.org/packages/Jeninnet.FileQuery)[https://www.nuget.org/packages/Jeninnet.FileQuery]

**Documentation:** (https://tareknajem04.github.io/Jeninnet.FileQuery/)[https://tareknajem04.github.io/Jeninnet.FileQuery/]

**License:** MIT
