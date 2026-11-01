# Cross-Platform Performance Baseline

This page records the cross-platform traversal comparison for **Jeninnet.FileQuery 1.5.0**
(.NET 10.0.14) and the regression baseline that future runs are compared against.

The comparison covers Windows x64, Ubuntu x64, and macOS Arm64. Two datasets are used because
they answer different questions:

| Dataset | Purpose | Reproducible |
|---|---|---|
| Controlled synthetic tree (`CrossPlatformTraversalBenchmark`) | Same tree shape and file count on every OS | Yes — deterministic names and contents |
| Repository checkout (existing `TraversalBenchmark` / `FileQueryBenchmark`) | Real-world shape, including `.git` | Partially — tracked files are identical, `.git` differs by platform |

The synthetic tree is the durable baseline. The checkout numbers are recorded because they are
what the scheduled pipeline has produced so far; they are indicative, not controlled.

---

## Controlled dataset

`CrossPlatformTraversalBenchmark` creates the dataset in the system temporary directory:

- **100 directories** (`dir000` … `dir099`).
- **100 files per directory** (`file000` … `file099`), empty contents.
- Extensions cycle through `.cs`, `.txt`, `.log`, `.json`.
- **10,000 files total**, no randomness.

It runs three queries over the same tree:

| Benchmark | Query | Matched files |
|---|---|---|
| `QueryAllFiles` (baseline) | `!**` (include everything) | 10,000 |
| `QueryGitIgnoreCs` | `**;!*.cs` (GitIgnore inclusion) | 2,500 |
| `QueryRegexCs` | `r:.*\.cs$` (typed regex) | 2,500 |

Run it locally with:

```bash
dotnet run --project src/Jeninnet.FileQuery.Benchmarks -c Release -- \
  --filter "*CrossPlatformTraversalBenchmark*" \
  --job Medium --exporters json markdown
```

The scheduled workflow uses `--job Short` to keep the matrix fast; use `Medium` or the default job
when recording a baseline.

### Windows reference (recorded 2026-11-01)

Environment: Windows 11 (10.0.26300.9818), Intel Core i7-8850H (6 physical / 12 logical cores),
.NET SDK 10.0.204, host runtime .NET 10.0.14, x64 RyuJIT.

| Method | Mean | Error | StdDev | Allocated | Allocated / file |
|---|---|---|---|---|---|
| `QueryAllFiles` | **11.94 ms** | 0.348 ms | 0.522 ms | 2.48 MB | ≈ 260 B |
| `QueryGitIgnoreCs` | **12.31 ms** | 0.307 ms | 0.440 ms | 2.50 MB | ≈ 262 B |
| `QueryRegexCs` | **11.86 ms** | 0.246 ms | 0.353 ms | 2.49 MB | ≈ 261 B |

`MediumRun`: 2 launches, 10 warmups, 15 iterations. The allocation is dominated by the absolute
path strings returned for each matched file, not by traversal bookkeeping.

Observations:

- **Matching is cheap relative to traversal.** GitIgnore and regex filtering add at most ~3% over
  matching everything on the same tree.
- **Allocations are flat across query shapes.** The engine allocates the result path strings; the
  matcher does not add measurable per-entry allocations.
- Throughput is roughly **1.2 µs per file** on this machine, including enumeration and matching.

---

## Three-OS results from the scheduled pipeline

Source: GitHub Actions run `40290984641` (scheduled benchmark, 2026-10-26), commit
`47b6c6438bc96ef0a65469ef8950114f68987d90`, `ShortRun` profile. Download the artifacts with:

```bash
gh run download 40290984641 -n benchmarks-windows-latest -D artifacts/windows
gh run download 40290984641 -n benchmarks-ubuntu-latest -D artifacts/ubuntu
gh run download 40290984641 -n benchmarks-macos-latest -D artifacts/macos
```

Both benchmarks traverse the repository checkout (`.git` included; only root `.cs` files are
returned). The tree contents therefore differ slightly per runner, and macOS runs on Arm64.

| Benchmark | Windows x64 | Ubuntu x64 | macOS Arm64 |
|---|---|---|---|
| `TraversalBenchmark.QueryFiles` | 250.3 µs | 316.3 µs (+26%) | 483.3 µs (+93%) |
| `FileQueryBenchmark.Match` | 253.9 µs | 317.9 µs (+25%) | 460.0 µs (+81%) |
| Allocated (`QueryFiles`) | 29.59 KB | 32.21 KB (+9%) | 32.31 KB (+9%) |

Error bars are large because `ShortRun` uses three iterations: ±17 µs on Windows, ±18 µs on
Ubuntu, and ±366 µs on macOS (the macOS mean is not statistically stable). The Windows and Ubuntu
runners are x64; macOS is Arm64, so the comparison mixes operating system, filesystem, and CPU
architecture.

### What the numbers mean

- **Allocations are stable across platforms** within ~9%, consistent with the synthetic result:
  the query allocates result strings, not platform-specific traversal state.
- **Windows and Ubuntu differ by ~25–26%** on the checkout. That gap is dominated by the
  filesystem/runtime, not by the matcher: the synthetic tree shows matching adds ≤3%.
- **macOS looks ~2× slower, but the run is too noisy to conclude that.** The error is almost as
  large as the mean and the runner is Arm64. Treat it as "revisit with `Medium`/default job on
  macOS" rather than a fact.
- **Do not compare absolute numbers across the two datasets.** The checkout tree is small
  (hundreds of entries) and dominated by process/IO startup; the synthetic tree is 10,000 files
  and measures steady-state traversal.

---

## Regression baseline

The machine-readable baseline lives in `docs/performance/cross-platform-baseline.json` and records:

- the controlled Windows reference above (job, runtime, hardware),
- the official three-OS checkout results and the run/commit identifiers,
- the regression policy.

**Policy**

1. Compare **within the same platform, job profile, runtime, and hardware class** — never across
   architectures or dataset shapes.
2. Investigate a **>25% regression** in mean time, or a **>25% increase** in allocated bytes, on
   the controlled dataset.
3. Re-baseline deliberately when the .NET runtime, BenchmarkDotNet version, runner image, or
   dataset shape changes; record the new date and identifiers in the JSON and this page.
4. Use `--job Medium` (or the default job) for baselines and `Short` only for trend monitoring.

The next scheduled benchmarks run will add Linux and macOS rows for the controlled dataset; until
then the controlled reference is Windows and the three-OS evidence is the checkout dataset above.

---

## Limitations

- **ShortRun noise**: three iterations and shared runners produce wide confidence intervals,
  especially on macOS.
- **Hardware variance**: GitHub-hosted runners are not a fixed CPU model; local numbers come from
  one laptop. The baseline is a reference, not a guarantee.
- **Filesystem caches**: neither dataset forces cold caches; results reflect warm OS caches.
- **Checkout dataset drift**: `.git` contents, line-ending normalization, and checkout layout
  differ per platform and over time.
- **No Linux/macOS synthetic run yet** in this branch: the harness exists and is wired into the
  suite; the scheduled workflow will publish those rows.

---

## See also

- [Benchmarking](benchmarking.md) — how the suite and workflow are structured.
- [Release Benchmark Baseline](release-benchmark-baseline.md) — the v1.5 large-scale results.
- [Traversal Model](traversal-model.md) — the traversal design behind these numbers.
- [Native Enumeration Research](native-enumeration-research.md) — why platform-specific enumeration was rejected.
