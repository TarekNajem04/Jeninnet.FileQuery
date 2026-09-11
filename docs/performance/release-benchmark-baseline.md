# Release Benchmark Baseline

Full BenchmarkDotNet runs are required before each tagged release.

## Automated Baselines
The dedicated benchmarks workflow (`.github/workflows/benchmarks.yml`) generates cross-platform baselines for **Windows**, **Linux**, and **macOS** on manual dispatch, weekly schedule, tags (`v*`), and published releases. These results are uploaded as build artifacts and serve as the primary validation for cross-platform performance consistency and zero-allocation enforcement.

## Local Baselines
While CI provides excellent relative validation, release candidates should ideally have a local baseline conducted in a controlled environment to ensure absolute precision.

Run:

```bash
dotnet run -c Release --project benchmarks/Jeninnet.FileQuery.Benchmarks.csproj
```

Store the exported results under `artifacts/benchmarks/<version>/` and summarize notable regressions or improvements in the release notes.
