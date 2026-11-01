# Long-Running Stability

The soak harness validates that repeated queries reach a steady state: throughput must not
degrade and the managed heap must not grow. It complements the cross-platform benchmark, which
measures single-run latency but cannot detect leaks.

The harness lives in `src/Jeninnet.FileQuery.Soak` and is not packaged.

---

## How it works

For each run it:

1. Creates a deterministic dataset in the system temporary directory
   (`--directories` × `--files-per-directory`; extensions cycle `.cs/.txt/.log/.json`).
2. Runs the query during a warmup period.
3. Measures for `--duration-seconds`, recording every iteration's duration.
4. Every `--sample-interval` iterations, forces a full GC and samples the managed heap.
5. Computes throughput, p50/p95 latency, first/second-half means, heap growth, and the
   least-squares heap slope.
6. Writes `soak-report.json` and `soak-report.md` and exits non-zero when a check fails.

### Verdict rules

| Check | Rule |
|---|---|
| Duration | At least 10 iterations |
| Determinism | Every iteration must return the same match count as the probe run |
| Throughput | Second-half mean must not exceed 1.5× the first-half mean |
| Managed heap | Growth between the first and last sample must stay under `--max-growth-mb` (default 4 MB) |

The heap check catches a leak that survives a full collection; the slope (bytes/iteration) is
reported for trend analysis.

---

## Recorded run (2026-11-01, Windows)

Environment: Windows 11 (10.0.26300), 12 logical processors, .NET 10.0.14.

```bash
dotnet run --project src/Jeninnet.FileQuery.Soak -c Release -- \
  --duration-seconds 60 --warmup-seconds 5 \
  --directories 100 --files-per-directory 50 \
  --sample-interval 50 --output artifacts/soak
```

| Metric | Value |
|---|---|
| Verdict | **PASS** |
| Dataset | 5,000 files (100 × 50) |
| Query | `**;!*.cs` |
| Matches per iteration | 1,300 |
| Iterations | 6,613 |
| Throughput | 110.2 iterations/s |
| Iteration time (mean / p50 / p95) | 9.05 / 8.88 / 10.44 ms |
| First half / second half mean | 8.49 / 9.60 ms |
| Managed heap first / last | 0.33 / 0.39 MB |
| Managed heap growth | **0.06 MB** (threshold 4 MB) |
| Heap slope | 12.0 B/iteration |
| Allocated during the run | 7,524 MB |

### Interpretation

- **No managed leak.** The heap sits at ~0.39 MB after the warmup and grows by 60 KB over 60
  seconds (slope ~12 B/iteration, consistent with sample noise), far below the 4 MB threshold.
- **Throughput is stable within the policy.** The second half is 13% slower than the first, below
  the 1.5× failure threshold. The drift is consistent with thermal/frequency variation on the
  laptop, not with unbounded work.
- **Allocation rate is expected.** Each iteration materializes 1,300 absolute path strings
  (≈ 7.5 GB over 6,613 iterations). The GC reclaims them; the live heap does not grow.
- **Determinism holds.** All 6,613 iterations returned exactly 1,300 matches.

This is evidence of steady-state behavior for the measured query and dataset shape; it is not a
proof for every query, filesystem, or platform. Re-run with different queries and shapes when the
traversal or matcher changes.

---

## Continuous validation

The `soak` CI job runs a shorter version on every push/PR that touches the source:

```bash
dotnet run --project src/Jeninnet.FileQuery.Soak -c Release -- \
  --duration-seconds 20 --warmup-seconds 3 \
  --directories 50 --files-per-directory 50 \
  --output artifacts/soak
```

The report is uploaded as the `soak-report` artifact even when the job fails, so a regression can
be inspected after the fact.

---

## Limitations

- **Managed heap only.** Native allocations and file-handle leaks are not sampled; file handles are
  process-scoped and released on exit.
- **Forced collections distort micro-timing.** Sampling every N iterations adds GC pauses outside the
  measured iteration, but the sample frequency still affects the overall run.
- **Warm caches.** The dataset is created immediately before the run, so the OS cache is warm.
- **Single query by default.** Pass `--query` to soak a different pattern, and vary
  `--directories`/`--files-per-directory` to change the tree shape.
- **Thresholds are defaults.** A machine with a very small heap or a deliberately allocating query
  may need `--max-growth-mb` adjusted; record the reason when changing it.

---

## See also

- [Cross-Platform Baseline](cross-platform-baseline.md) — single-run traversal measurements.
- [Benchmarking](benchmarking.md) — the BenchmarkDotNet suite and workflow.
