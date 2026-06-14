# Jeninnet.FileQuery.Benchmarks

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub release](https://img.shields.io/github/v/release/TarekNajem04/Jeninnet.FileQuery)
![GitHub contributors](https://img.shields.io/github/contributors/TarekNajem04/Jeninnet.FileQuery)
[![GitHub stars](https://img.shields.io/github/stars/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/stargazers)
[![GitHub license](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery)
[![NuGet downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery)](https://www.nuget.org/packages/Jeninnet.FileQuery/)

**Performance measurement and allocation tracking suite for [Jeninnet.FileQuery](../README.md).**

This project uses **BenchmarkDotNet** to measure execution speeds, evaluate memory consumption, and verify the zero-allocation guarantees of the file discovery engine.

---

## How to Run Benchmarks

Run the project in **Release** mode from the command line:

```bash
dotnet run -c Release --project benchmarks/Jeninnet.FileQuery.Benchmarks.csproj
```

> [!IMPORTANT]
> Always run benchmarks on mains power, with other heavy applications closed, to prevent thermal throttling and ensure reproducible results.

---

## Benchmark Catalog

The suite contains the following benchmarks under `benchmarks/`:

1.  **[FileQueryBenchmark](./FileQueryBenchmark.cs)**: Measures end-to-end file queries using real directories.
2.  **[GlobMatcherBenchmark](./GlobMatcherBenchmark.cs)**: Evaluates segment matching performance with glob patterns.
3.  **[GitIgnoreMatcherBenchmark](./GitIgnoreMatcherBenchmark.cs)**: Tests GitIgnore matching and negations.
4.  **[RegexMatcherBenchmark](./RegexMatcherBenchmark.cs)**: Measures regex engine matching overhead.
5.  **[HybridMatcherBenchmark](./HybridMatcherBenchmark.cs)**: Measures routing overhead in the mixed pattern matcher.
6.  **[PatternCompilationColdStartBenchmark](./PatternCompilationColdStartBenchmark.cs)**: Evaluates cold-start cost of tokenizing and compiling pattern syntax profiles.
7.  **[PatternPipelineAllocationBenchmark](./PatternPipelineAllocationBenchmark.cs)**: Specifically tracks heap allocations during pattern compilation.
8.  **[TraversalStrategyBenchmark](./TraversalStrategyBenchmark.cs)**: Compares `DepthFirst` (stack-based) vs. `BreadthFirst` (queue-based) search methods.
9.  **[CharacterClassMatcherBenchmark](./CharacterClassMatcherBenchmark.cs)**: Evaluates range matchers, custom brackets, and POSIX class evaluations.

Phase 2 observability features are opt-in. Default benchmark paths remain focused on the non-diagnostic hot path; progress and audit sinks should be benchmarked separately when measuring instrumentation overhead.

---

## Performance & Memory Goals

*   **Zero Allocations**: Matching execution must not perform heap allocations (no `new` statements, boxing, or collection growth inside traversal/evaluation).
*   **Opt-in Observability**: Progress and diagnostics are disabled by default so baseline matcher and traversal measurements remain comparable.
*   **Throughput Target**:
    *   Pure enumeration: **~1.8M files/sec**
    *   Glob matching: **~1.2M files/sec**
    *   GitIgnore matching: **~1.1M files/sec**
