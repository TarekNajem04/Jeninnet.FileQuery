# Runtime Evolution Tracking

.NET runtime updates can change filesystem enumeration performance without any change in this
repository. This page defines how runtime changes are detected, when benchmarks are re-run, and
where the results are recorded.

The tracked values live in `docs/performance/runtime-evolution.json`; the measured numbers live in
`docs/performance/cross-platform-baseline.json` and `docs/performance/long-running-stability.md`.

---

## What is tracked

| Value | Source | Why |
|---|---|---|
| `Microsoft.NETCore.App` runtime | `dotnet --list-runtimes` | The BCL enumeration code that dominates traversal comes from the runtime. |
| Pinned SDK | `global.json` | Reproducible builds and analyzers. |
| Resolved SDK | `dotnet --version` | The runner may install a newer feature band within the roll-forward policy. |
| Benchmark baseline | `cross-platform-baseline.json` | Must record the same runtime as the tracking file. |
| History | `runtime-evolution.json` | When the runtime changed and what was done about it. |

`scripts/performance/check-runtime.ps1` compares the current machine with the recorded values:

```bash
pwsh scripts/performance/check-runtime.ps1
```

It prints the recorded/current runtime and SDK, warns on drift, and with `-Enforce` exits `1`.
It exits `2` when `dotnet` or a baseline file is missing, or when the tracking file and the
performance baseline disagree.

---

## Cadence

| Trigger | Action |
|---|---|
| **Weekly schedule** (Mondays 03:00 UTC) | `benchmarks.yml` runs on Windows, Ubuntu, and macOS; each artifact now includes `runtime-info.txt` (the full `dotnet --info`). |
| **Runtime or SDK change** (`global.json`, runner image, .NET update) | Run the check; if it reports drift, re-baseline before comparing new numbers with the old ones. |
| **Before every release tag** | Re-run the controlled benchmark and the soak; refresh the baseline and add a history entry. |
| **When the check reports drift** | Treat the next benchmark run as a new baseline rather than a regression: update the JSON files and the narrative. |

The weekly schedule is intentional: it catches silent runner-image updates. The check runs inside
`benchmarks.yml` before the benchmarks, so every scheduled run prints the comparison in its log.

---

## Re-baseline procedure

1. **Check** the runtime:

   ```bash
   pwsh scripts/performance/check-runtime.ps1
   ```

2. **Measure** the controlled dataset (use `Medium` for a baseline, not `Short`):

   ```bash
   dotnet run --project src/Jeninnet.FileQuery.Benchmarks -c Release -- \
     --filter "*CrossPlatformTraversalBenchmark*" \
     --job Medium --exporters json markdown
   ```

3. **Soak** the engine for stability:

   ```bash
   dotnet run --project src/Jeninnet.FileQuery.Soak -c Release -- \
     --duration-seconds 60 --warmup-seconds 5 \
     --directories 100 --files-per-directory 50 \
     --sample-interval 50 --output artifacts/soak
   ```

4. **Update the records**:
   - `docs/performance/cross-platform-baseline.json` — new `runtime`, `sdk`, numbers, date.
   - `docs/performance/runtime-evolution.json` — `runtime`, `latestSdk`, and a new `history` entry.
   - `docs/performance/cross-platform-baseline.md` and `long-running-stability.md` — refresh the
     tables and the interpretation.
5. **Commit** the numbers with the runtime change (`docs(performance): rebaseline on .NET X.Y.Z`).

Do not mix numbers from different runtimes in one comparison table; if both must be shown, label
the runtime per row.

---

## Current state

Recorded on 2026-11-01:

| Value | Recorded | Current (2026-11-01) |
|---|---|---|
| Runtime | 10.0.14 | 10.0.14 |
| Pinned SDK | 10.0.203 (`rollForward: latestFeature`) | — |
| Resolved SDK | 10.0.204 | 10.0.204 |

The controlled benchmark baseline and the 60-second soak were recorded on runtime 10.0.14; the
weekly scheduled run `40290984641` also reports runtime 10.0.14 on all three platforms.

## History

| Runtime | SDK | Recorded | Source | Notes |
|---|---|---|---|---|
| 10.0.14 | 10.0.203 pinned / 10.0.204 resolved | 2026-11-01 | Local Windows `MediumRun` + scheduled run `40290984641` | Initial tracked runtime for the v2.0.0 cycle. |

Add a row for every runtime change that triggers a re-baseline. A runtime update that produces no
measurable change still gets a row with "no measurable change" in the notes; the next comparison
then has a clear starting point.

---

## CI integration

`benchmarks.yml` runs the check and records the environment before measuring:

```yaml
- name: Check runtime drift
  shell: pwsh
  run: pwsh scripts/performance/check-runtime.ps1

- name: Record runtime information
  shell: bash
  run: |
    mkdir -p BenchmarkDotNet.Artifacts/results
    dotnet --info | tee BenchmarkDotNet.Artifacts/results/runtime-info.txt
```

The check is informational (exit `0` on drift); the benchmark run that follows is the new evidence.
The `runtime-info.txt` file travels with the benchmark artifact, so a result can always be traced
back to the exact SDK and runtime that produced it.

---

## Limitations

- **Patch-level runtime changes may not move the needle.** Recording them keeps the provenance
  clear even when the numbers are unchanged.
- **Runner images update silently.** The weekly run is the safety net; a scheduled run whose
  `runtime-info.txt` differs from the recorded runtime should be treated as a new baseline.
- **macOS `ShortRun` remains noisy.** Runtime comparisons on macOS need `Medium`/default runs before
  drawing conclusions.
- **This page does not automate commits.** The workflow records evidence; updating the JSON and the
  narrative is a deliberate, reviewed step.

---

## See also

- [Cross-Platform Baseline](cross-platform-baseline.md) — the measured numbers and policy.
- [Long-Running Stability](long-running-stability.md) — the soak results.
- [Benchmarking](benchmarking.md) — the suite and workflow.
