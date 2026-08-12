# Jeninnet.FileQuery

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub release](https://img.shields.io/github/v/release/TarekNajem04/Jeninnet.FileQuery)
![GitHub contributors](https://img.shields.io/github/contributors/TarekNajem04/Jeninnet.FileQuery)
[![GitHub stars](https://img.shields.io/github/stars/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/stargazers)
[![GitHub license](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery)
[![NuGet downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery)](https://www.nuget.org/packages/Jeninnet.FileQuery.CommandLine/)

**A high-performance, deterministic file discovery and filtering engine for .NET.**

Jeninnet.FileQuery combines three pattern dialects — **GitIgnore**, **Glob**, and **Regular Expressions** — into a single, deterministic query pipeline. Patterns are tokenized, validated against structural invariants, and compiled into immutable instruction sets before any filesystem work begins. The compiled matchers then evaluate every entry through one unified pipeline where GitIgnore, Glob, and Regex rules coexist with well-defined precedence. The result is predictable, scalable file discovery across Windows, Linux, and macOS — decoupled from `System.IO` through the [`IFileSystem`](./src/Jeninnet.FileQuery/IO/IFileSystem.cs) abstraction.

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

## ✨ Features

### Pattern Queries

*   **Three dialects, one pipeline.** Write patterns in GitIgnore, Glob, or Regex (`r:` prefix) syntax. In Hybrid mode the engine classifies each pattern automatically; in Specific mode you choose a single dialect explicitly.
*   **Deterministic precedence.** GitIgnore inclusions take final precedence over Glob and Regex sub-sets. A GitIgnore inclusion is final; Glob and Regex matchers can re-include paths excluded by GitIgnore but cannot exclude paths that GitIgnore has included.
*   **POSIX character classes.** Bracket expressions support named classes (`[:digit:]`, `[:alpha:]`, `[:alnum:]`, `[:space:]`, `[:blank:]`, `[:upper:]`, `[:lower:]`, `[:print:]`, `[:graph:]`, `[:punct:]`, `[:cntrl:]`, `[:xdigit:]`).
*   **Compile-time validation.** Patterns are tokenized and checked against semantic and structural invariants (e.g., `**` isolation, valid character ranges) before execution. Malformed patterns produce rich diagnostics without throwing during tokenization.

### Traversal

*   **Depth-first or breadth-first.** Choose between stack-based (`DepthFirst`) or queue-based (`BreadthFirst`) traversal strategies through `TraversalOptions`.
*   **Symlink policies.** Control symbolic link behavior with `Ignore`, `Follow`, or `FollowWithCycleDetection`.
*   **Decoupled filesystem.** The [`IFileSystem`](./src/Jeninnet.FileQuery/IO/IFileSystem.cs) interface decouples all IO, enabling virtual or mock filesystems for cloud workloads and test isolation.
*   **Configurable error recovery.** Handle IO errors with `Skip`, `Retry(n)`, or `Abort` strategies through `FileQueryErrorRecoveryOptions`.

### Observability

*   **Async progress snapshots.** Report live traversal statistics (entries scanned, files matched, directories visited) through `IProgress<FileQueryProgress>`.
*   **Match audit diagnostics.** Opt-in per-entry explanations of match outcomes, including the responsible pattern and its source metadata, through `IProgress<FileQueryDiagnostic>`.
*   **Deep cancellation.** `CancellationToken` propagation is verified across all hot paths.
*   **OpenTelemetry integration.** Export metrics and spans for structured observability.
*   **Roslyn analyzers.** Design-time warnings and code fixes for malformed GitIgnore, Glob, or Regex patterns.

### Performance

*   **Zero-allocation matching hot path.** The matching evaluation loop is designed to avoid per-entry heap allocations, using `ReadOnlySpan<char>`, stack-allocated `ref struct` types (`PathView`, `PathSegmentEnumerator`), and index-based loops. Allocation-free matching is enforced by architecture tests.
*   **Pooled relative-path buffering.** Relative paths are composed in a reusable `ArrayPool<char>`-backed buffer (`RelativePathBuffer`) rather than allocating a new string for every traversed entry. A `string` is materialized only for matched results.
*   **GitIgnore literal-suffix fast path.** Eligible wildcard patterns resolve their literal suffix at compile time, allowing zero-allocation `EndsWith` rejection before recursive matching.
*   **AOT-ready.** Reflection is completely avoided to ensure compatibility with .NET Native AOT compilation.

---

## ⚡ Performance at Scale

Jeninnet.FileQuery v1.5.0 has been validated against a reproducible evaluation environment containing **1,000,000 files across 4,096 directories**. This dataset is used for traversal performance measurement and regression detection.

### v1.5.0 Performance Investigation

The v1.5.0 release completed a systematic performance investigation cycle covering filesystem enumeration, traversal allocations, GitIgnore matching cost, and the remaining BCL/OS-bound overhead. The investigation was conducted across multiple profiling phases using the same 1,000,000-file dataset with the following query:

```
**/*.cs;!**/bin/**;!**/obj/**;!**/node_modules/**;!**/*.generated.cs
```

Key findings from the investigation:

*   **Redundant per-entry attribute lookups** were eliminated by consuming filesystem attributes directly from .NET enumeration results.
*   **Per-entry relative-path string allocations** were replaced with a pooled `ArrayPool<char>`-backed buffer, reducing total allocations by ~36% for the measured query.
*   **The GitIgnore literal-suffix fast path** reduced the marginal matching cost of eligible wildcard negation patterns (e.g., `!**/*.generated.cs`) to effectively zero in the profiled workload, down from being the single largest matching cost prior to this optimization.
*   **After all optimizations**, profiling confirmed that the matching engine, traversal logic, and path processing together account for a small fraction of total execution time. The dominant remaining cost is attributable to .NET/BCL filesystem enumeration (`FileSystemEnumerator`) and underlying operating-system filesystem operations.

Further native filesystem enumeration was not introduced because the measured benefit did not justify the additional platform-specific complexity and maintenance risk. The detailed investigation methodology and per-phase measurements are documented under [`docs/performance/`](./docs/performance/).

---

## 📊 Repo Stats

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/57d92552dfb25309185f7457c01037a504b5fa24.svg "Repobeats analytics image")

---

## Upgrading

If you are upgrading from v1.0.0 or v1.1.0 to v1.2.0, please see the [Migration Guide](Migration.md) for important breaking changes regarding `FileQueryOptions`.

---

## 📂 Project Suite Directory

This repository contains the core library and its companion packages:

*   **[Core Engine (Jeninnet.FileQuery)](./src/Jeninnet.FileQuery/README.md)**: The main matching runtime, builders, and parser pipeline.
*   **[CommandLine Integration (Jeninnet.FileQuery.CommandLine)](./src/Jeninnet.FileQuery.CommandLine/README.md)**: Bridges command-line arguments (using `System.CommandLine`) to file query patterns.
*   **[DependencyInjection Integration (Jeninnet.FileQuery.DependencyInjection)](./src/Jeninnet.FileQuery.DependencyInjection/README.md)**: Configures and registers the engine and its components in standard .NET host applications.
*   **[Documentation Suite (docs/)](./docs/README.md)**: Deep technical specifications, guides, and architectural whitepapers.
*   **[Benchmark Suite](./src/Jeninnet.FileQuery.Benchmarks/README.md)**: Performance measurements and allocation verification.
*   **[Samples (samples/)](./samples/AdvancedUsage/README.md)**: Practical examples showing basic to advanced usage.

---

## 🏗️ High-Level Architecture Overview

`Jeninnet.FileQuery` splits filesystem traversal, pattern compilation, and matching into distinct layers:

```txt
                     ┌───────────────────────┐
                     │     Client Code       │
                     └───────────┬───────────┘
                                 │
                                 ▼
                     ┌───────────────────────┐
                     │   FileQueryBuilder    │
                     └───────────┬───────────┘
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

### Layer Responsibilities

1.  **Orchestration & API Layer**: [`FileQueryBuilder`](./src/Jeninnet.FileQuery/FileQueryBuilder.cs) accepts configuration inputs and builds an immutable [`FileQuery`](./src/Jeninnet.FileQuery/FileQuery.cs) instance.
2.  **Matching Layer**: [`HybridPathMatcher`](./src/Jeninnet.FileQuery/Matching/Compiled/HybridPathMatcher.cs) decomposes rules and routes them to target matchers (GitIgnore, Glob, Regex). GitIgnore inclusions take final precedence.
3.  **Compilation & Parser Layer**: Tokenizes raw strings, checks them against semantic and structural invariants (e.g., recursive wildcard isolation), and produces compiled instruction sets.
4.  **IO Traversal Layer**: Traverses directories using a stack-based or queue-based execution plan, decoupling all IO through the [`IFileSystem`](./src/Jeninnet.FileQuery/IO/IFileSystem.cs) interface.

---

## 💡 Advanced Usage

### Async Traversal with Progress Reporting

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

### Match Audit Diagnostics

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

### Decoupled Filesystem Abstraction

By using the [`IFileSystem`](./src/Jeninnet.FileQuery/IO/IFileSystem.cs) interface, the traversal engine can run against virtual or mock filesystems (useful for cloud workloads and fast test isolation).

---

## 📋 Project Status

**Current release: v1.5.0** — This release completes a focused performance engineering cycle for large-scale filesystem traversal and pattern matching.

The optimization cycle was validated against a reproducible **1,000,000-file / 4,096-directory** dataset and included:

- Elimination of redundant per-entry filesystem attribute lookups.
- Removal of per-entry relative-path string allocations through pooled path buffering.
- A compile-time GitIgnore literal-suffix fast path for inexpensive wildcard rejection.
- Measurement-driven profiling of traversal, matching, allocation, and filesystem enumeration costs.
- Verification that the remaining dominant cost is the underlying .NET/BCL filesystem enumeration and operating-system I/O boundary.

The measured optimization work reduced execution time substantially while preserving matching semantics, public APIs, and allocation behavior outside the targeted hot paths.

The performance investigation is intentionally stopped at this boundary. Further optimization of the remaining BCL/OS-bound enumeration cost would require substantially more platform-specific and runtime-dependent techniques, with a higher complexity and maintenance cost.

All optimization decisions were based on measurements rather than speculative micro-optimizations.

See **[CHANGELOG.md](./CHANGELOG.md)** for the release history and **[ROADMAP.md](./ROADMAP.md)** for planned future work.

---

## 📜 Governance and Philosophy

The project adheres to the **Jeninnet.FileQuery Constitution**:
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
*   See **[ROADMAP.md](./ROADMAP.md)** for planned features and the project's strategic direction.

---

## 📜 License

This project is licensed under the MIT License. See **[LICENSE](./LICENSE)** for details.
