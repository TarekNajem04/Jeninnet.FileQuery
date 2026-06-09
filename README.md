# Jeninnet.FileQuery

**A high-performance, deterministic file discovery and filtering engine for .NET.**

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub Tag](https://img.shields.io/github/v/tag/TarekNajem04/Jeninnet.FileQuery)
![GitHub contributors](https://img.shields.io/github/contributors/TarekNajem04/Jeninnet.FileQuery)
![GitHub forks](https://img.shields.io/github/forks/TarekNajem04/Jeninnet.FileQuery)
![GitHub last commit](https://img.shields.io/github/last-commit/TarekNajem04/Jeninnet.FileQuery)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-closed/tareknajem04/Jeninnet.FileQuery)
[![GitHub stars](https://img.shields.io/github/stars/TarekNajem04/Jeninnet.FileQuery&)](https://github.com/TarekNajem04/Jeninnet.FileQuery/stargazers)
[![GitHub license](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery)
[![NuGet downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery)](https://www.nuget.org/packages/Jeninnet.FileQuery.CommandLine/)

---

## ✨ Repo Stats

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/57d92552dfb25309185f7457c01037a504b5fa24.svg "Repobeats analytics image")

---

`Jeninnet.FileQuery` is a modern, high-performance file query system designed for .NET 10 and C# 14. It combines multiple pattern dialectsâ€”**GitIgnore**, **Glob**, and **Regular Expressions**â€”into a unified, deterministic, and allocation-free matching pipeline. Decoupled from `System.IO`, the engine allows precise, scalable file discovery across massive directory structures on Windows, Linux, and macOS.

The Phase 2 observability surface adds async progress snapshots, optional match audit diagnostics, deep cancellation verification, and configurable IO recovery strategies without changing default query behavior.

---

## 📂 Project Suite Directory

This repository contains the core library and its companion packages:

*   **[Core Engine (Jeninnet.FileQuery)](./src/Jeninnet.FileQuery/README.md)**: The main matching runtime, builders, and parser pipeline.
*   **[CommandLine Integration (Jeninnet.FileQuery.CommandLine)](./src/Jeninnet.FileQuery.CommandLine/README.md)**: Bridges command-line arguments (using `System.CommandLine`) to file query patterns.
*   **[DependencyInjection Integration (Jeninnet.FileQuery.DependencyInjection)](./src/Jeninnet.FileQuery.DependencyInjection/README.md)**: Configures and registers the engine and its components in standard .NET host applications.
*   **[Documentation Suite (docs/)](./docs/README.md)**: Deep technical specifications, guides, and architectural whitepapers.
*   **[Benchmark Suite (benchmarks/)](./benchmarks/README.md)**: Performance measurements and allocation verification.
*   **[Samples (samples/)](./samples/AdvancedUsage/README.md)**: Practical examples showing basic to advanced usage.

---

## 📊 Repo Stats

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/57d92552dfb25309185f7457c01037a504b5fa24.svg "Repobeats analytics image")

---

## 🚀 Quick Start

Get up and running in seconds using the fluent API.

### Installation

Install the core package from NuGet:

```bash
dotnet add package Jeninnet.FileQuery
```

### Basic Usage

```csharp
using Jeninnet.FileQuery;

// 1. Configure the query using the fluent builder
var query = FileQuery.From(@"C:\MyProject")
                     .Where("**")           // Exclude everything by default
                     .Where("!*.tmp")       // Include all .tmp files
                     .Where("!src/**/*.cs") // Include .cs files under the src folder
                     .UsingHybrid()         // Auto-detect pattern dialects (GitIgnore/Glob/Regex)
                     .IgnoreCase()          // Use case-insensitive matching
                     .Build();

// 2. Execute the query using the runtime engine
var engine = FileQueryRuntime.Create();
var files = engine.Execute(query);

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

---

## 🏗️ High-Level Architecture Overview

`Jeninnet.FileQuery` splits filesystem traversal, pattern compilation, and matching into distinct, highly optimized layers:

```
                          [ Client Code ]
                                 â”‚
                        [ FileQueryBuilder ]
                                 │
                                 ▼
                     ┌───────────────────────┐
                     │       FileQuery       │
                     │  (Immutable Request)  │
                     └───────────┬───────────┘
                                 │
                                 ▼
                     ┌───────────────────────┐
                     │   IFileQueryEngine    │
                     └───────────┬───────────┘
                                 │ Executes
                                 ▼
                     ┌───────────────────────┐
                     │   HybridPathMatcher   │
                     └───────────┬───────────┘
          ┌──────────────────────┼──────────────────────┐
          ▼                      ▼                      ▼
┌──────────────────┐   ┌──────────────────┐   ┌──────────────────┐
│ GitIgnoreMatcher │   │   GlobMatcher    │   │   RegexMatcher   │
└──────────────────┘   └──────────────────┘   └──────────────────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 ▼
                       ┌───────────────────┐
                       │    IFileSystem    │
                       └───────────────────┘
```

1.  **Orchestration & API Layer**: [FileQueryBuilder](./src/Jeninnet.FileQuery/FileQueryBuilder.cs) accepts configuration inputs and builds an immutable [FileQuery](./src/Jeninnet.FileQuery/FileQuery.cs) instance.
2.  **Matching Layer**: [HybridPathMatcher](./src/Jeninnet.FileQuery/Matching/Compiled/HybridPathMatcher.cs) decomposes rules and routes them to target matchers (GitIgnore, Glob, Regex). GitIgnore inclusions take final precedence.
3.  **Compilation & Parser Layer**: Tokenizes raw strings, checks them against semantic and structural invariants (e.g., recursive wildcard isolation), and produces compiled instruction sets.
4.  **IO Traversal Layer**: Traverses directories using an stack-based or queue-based execution plan, decoupling all IO through the [IFileSystem](./src/Jeninnet.FileQuery/IO/IFileSystem.cs) interface.

---

## 💡 Advanced & Developer-Oriented Features

### 1. POSIX Character Classes
The engine supports POSIX-named character classes within Glob and GitIgnore bracket expressions (e.g., `[[:digit:]]`). Supported classes include:
*   `[:digit:]`: Decimal digits (`0-9`)
*   `[:alpha:]`: Alphabetic characters
*   `[:alnum:]`: Alphanumeric characters
*   `[:space:]`: White space characters
*   `[:blank:]`: Space or horizontal tab characters
*   `[:upper:]` / `[:lower:]`: Uppercase / lowercase characters
*   `[:print:]` / `[:graph:]`: Printable / visible characters
*   `[:punct:]`: Punctuation and symbol characters
*   `[:cntrl:]`: Control characters
*   `[:xdigit:]`: Hexadecimal digits

### 2. Zero-Allocation Matching Path
To support massive file trees containing millions of files, the matching evaluation loop is completely allocation-free, utilizing `ReadOnlySpan<char>` and stack-allocated parsing arrays.

### 3. Decoupled Filesystem Abstraction
By using the [IFileSystem](./src/Jeninnet.FileQuery/IO/IFileSystem.cs) interface, the traversal engine can run against virtual or mock filesystems (useful for cloud workloads and fast test isolation).

### 4. Advanced Traversal Tuning
Through `TraversalOptions`, developers can choose between `DepthFirst` (stack-based) or `BreadthFirst` (queue-based) search strategies and control symlink behavior with policies like `Ignore`, `Follow`, or `FollowWithCycleDetection`.

### 5. Observability and Recovery
Async scans can report live statistics through `IProgress<FileQueryProgress>`:

```csharp
var progress = new Progress<FileQueryProgress>(snapshot =>
{
    Console.WriteLine($"{snapshot.EntriesScanned} entries scanned");
});

await foreach (var file in engine.ExecuteAsync(query, progress, cancellationToken))
{
    Console.WriteLine(file);
}
```

Opt-in diagnostics explain match outcomes and responsible pattern metadata:

```csharp
var diagnostics = new Progress<FileQueryDiagnostic>(entry =>
{
    Console.WriteLine($"{entry.RelativePath}: {entry.Outcome} ({entry.Pattern})");
});

var query = FileQuery.From("./src")
                     .Where("**", "!**/*.cs")
                     .WithDiagnostics(diagnostics)
                     .WithErrorRecovery(FileQueryErrorRecoveryOptions.Retry(2))
                     .Build();
```

---

## 📜 Governance and Philosophy

The project adheres to the **[Jeninnet.FileQuery Constitution](./constitution.md)**:
1.  **Engine‑First & Pattern‑Compiled**: Always tokenize, validate invariants, and compile before running; no ad-hoc string regexes or runtime heuristics.
2.  **Compile‑Time Safe**: Malformed patterns never throw during tokenization; all syntax errors are captured during the invariant phase to output rich diagnostics.
3.  **AOT-Ready**: Reflection is completely avoided to ensure compatibility with .NET Native AOT compilation.

---

## 🏛️ Compatibility & Tech Stack

*   **TFM**: `.NET 10`
*   **Language**: `C# 14`
*   **Test Suite**: MSTest + Moq + Coverlet
*   **Enforcement**: Central Package Management (CPM), strict `.editorconfig` rules, and architecture tests enforcing zero allocations.

---

## 🚀 Contributing & Roadmap

*   Please consult **[CONTRIBUTING.md](./CONTRIBUTING.md)** before submitting pull requests.
*   See **[ROADMAP.md](./ROADMAP.md)** for planned features (such as Progress Reporting, Audit Diagnostics, and Parallel Traversal).

---

## 📜 License

This project is licensed under the MIT License. See **[LICENSE](./LICENSE)** for details.
