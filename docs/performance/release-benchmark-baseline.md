# Release Benchmark Baseline

A full BenchmarkDotNet run in Release mode is required locally before each tagged release. The
CI workflow is a `Short` cross-platform consistency check, not the full baseline.

## Automated Baselines
The dedicated benchmarks workflow (`.github/workflows/benchmarks.yml`) generates cross-platform `Short`
results for **Windows**, **Linux**, and **macOS** on manual dispatch, weekly schedule, tags (`v*`),
and published releases. These results are uploaded as build artifacts and provide cross-platform
consistency and zero-allocation evidence; the local Medium/full run provides the release baseline.

## Local Baselines
While CI provides excellent relative validation, release candidates should ideally have a local baseline conducted in a controlled environment to ensure absolute precision.

Run:

```bash
dotnet run -c Release --project src/Jeninnet.FileQuery.Benchmarks/Jeninnet.FileQuery.Benchmarks.csproj
```

Store the exported results under `artifacts/benchmarks/<version>/` and summarize notable regressions or improvements in the release notes.

The controlled cross-platform reference (deterministic 10,000-file tree, Windows/Linux/macOS
policy) is recorded in [Cross-Platform Baseline](cross-platform-baseline.md).
