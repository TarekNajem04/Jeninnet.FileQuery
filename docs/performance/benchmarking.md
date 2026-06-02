# Benchmarking and Performance Validation

## Overview
Performance is a first-class citizen in Jeninnet.FileQuery. The solution uses a dedicated benchmarking suite to ensure that new changes do not introduce latency or excessive memory allocations during file traversal.

## Why it is Used
File system querying can easily become a bottleneck in large repositories. We use benchmarking to validate the efficiency of the `TraversalExecutor` and `PathMatcher` implementations.

## Implementation in this Solution
The project includes a `Jeninnet.FileQuery.Benchmarks` project using **BenchmarkDotNet**.

### Automated Baselines (CI)
To ensure performance stability across all supported platforms, the CI pipeline (`.github/workflows/ci.yml`) executes a benchmark matrix on every push to `main` and for every Pull Request.

- **Platforms**: Windows (Latest), Ubuntu (Latest), macOS (Latest).
- **Artifacts**: Benchmark results (Markdown, JSON) are uploaded as build artifacts (e.g., `benchmarks-windows-latest`).
- **Profile**: Uses the `Short` job profile to balance execution time and data quality in a shared environment.

### Key Metrics Tracked
- **Execution Time**: Average time to traverse a directory tree of $N$ files.
- **Allocations**: Number of bytes allocated per file matched to keep GC pressure low.
- **Throughput**: Files processed per second.

## How to Run Benchmarks
1. Navigate to the benchmarks project:
   `cd benchmarks`
2. Run the benchmark suite in Release mode:
   `dotnet run -c Release`

## Best Practices
- **Warmup**: Always allow BenchmarkDotNet to perform warmup iterations.
- **Environment**: Run benchmarks on a dedicated machine to avoid "noisy neighbor" interference.
- **Comparison**: Compare new results against the baseline stored in the performance documentation.
