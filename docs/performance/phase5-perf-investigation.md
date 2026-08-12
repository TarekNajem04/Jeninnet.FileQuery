# Phase 5 — Performance Investigation Report

Read-only CPU profiling of the evaluation hot path on the same 1,000,000-file generated
dataset (`%TEMP%\Jeninnet.FileQuery\AdvancedUsage\Dataset`, 4,097 directories + 1,000,000 files,
1,000,001 entries total), using the exact query specified for this phase:

```
**/*.cs;!**/bin/**;!**/obj/**;!**/node_modules/**;!**/*.generated.cs
```

> **Match-count discrepancy (resolved):** this query consistently returns **862,374 matches**,
> not the 876,017 expected from the phase brief. 876,017 belongs to the EvaluationRunner query
> used in Phases 3–4 (`**; !bin/; !obj/; !node_modules/; !*.tmp; !*.generated.cs;
> r:^.*\.(cs|csproj|json|xml|md)$`), which was reproduced here: **876,017 matches** in two
> consecutive runs. The brief's expected count is therefore the runner-query count, not the
> string-query count. 862,374 was stable across every run (20+ iterations, all modes).

---

## 1. Method

Two complementary techniques were used:

1. **Exact elapsed-time attribution** — temporary, environment-gated instrumentation
   (`PHASE5_PROFILE` / `PHASE5_ALLOC`) added timestamp and allocation taps around each stage,
   plus invocation counters. Zero-allocation (no behavior change) and removed afterward;
   the working tree is byte-identical to HEAD after the investigation.
2. **Sampling profiler** (`dotnet-trace`, two profiles) — for a method-level hotspot list with
   no instrumentation. Runs 1–2 used `dotnet-sampled-thread-time,dotnet-common`; run 3 used the
   classic CPU-sampling provider set (`0x14C14FCCBD:4`).

Timestamps and the pattern-sweep (below) agree with each other to the millisecond; the
sampler's method-level percentages do not (see §7 caveat) and are used only as corroboration.

## 2. Measured Totals (pristine, post-revert)

| Run | Elapsed | Matches | Allocated | gen0 / gen1 / gen2 |
|-----|---------|---------|-----------|--------------------|
| pristine ×3 | 4,536 / 4,622 / 5,421 ms | 862,374 | 330.58 MB | 74 / 1–2 / 0 |
| post-revert ×3 | 4,429 / 4,370 / 4,570 ms | 862,374 | 330.58 MB | 74 / 1–2 / 0 |

Median ≈ **4.6 s** per iteration (Phase 4 baseline: 4,417 ms median; same order).

## 3. Ranked CPU Hotspots (measured evidence)

The loop total of the attributed run was **4,656.7 ms** (harness timer for the same iteration:
4,657 ms — instrumentation added no measurable distortion).

| # | Stage | Elapsed | % of loop | Calls | Avg / call |
|---|-------|---------|-----------|-------|------------|
| 1 | **GitIgnore matching** (`!**/*.generated.cs` etc.) | **2,917.9 ms** | **62.66 %** | 1,004,097 | 2.906 µs |
| 2 | **BCL enumeration** (MoveNext + lock + Win32 + strings) | **1,456.0 ms** | **31.27 %** | 1,004,097 | 1.450 µs |
| 3 | Relative-path construction | 164.0 ms | 3.52 % | 1,004,097 | 0.163 µs |
| 4 | Other entry processing (context, decision, symlink, prune) | 71.1 ms | 1.53 % | 1,004,097 | 0.071 µs |
| 5 | Residual (frames, yields, visited set) | < 1 ms | ~0 % | — | — |
| | **Entry processing total** | **3,200.2 ms** | **68.72 %** | 1,004,097 | 3.187 µs |
| | **Loop total** | **4,656.7 ms** | **100 %** | 4,097 dirs | — |

Matching layer (resolver + matchers) = 2,965.1 ms (63.67 %); Glob and Regex subsets were
never invoked (0 calls) — all cost is GitIgnore.

**Method-level sampling corroboration** (main thread, both traces): `FileSystemEnumerator.MoveNext`
at the leaf in ~53 % (thread-time) / 44.5 % (CPU-sampled) of samples, with
`Monitor.Enter_Slowpath` immediately below at ~48 % / 41.3 % — i.e. the sampler's top
entry-point is also the enumeration machinery, and the sampler shows the BCL per-`MoveNext`
`lock (_lock)` as the single hottest leaf method.

## 4. Per-Pattern Marginal Cost (stopwatch sweep, same dataset)

| Patterns | Median ms | Δ | Matches |
|----------|-----------|-----|---------|
| (none — enumeration + loop only) | 1,290 | — | 1,000,001 |
| `**/*.cs` | 2,488 | +1,198 | 670,001 |
| + `!**/bin/**` | 2,644 | +156 | 745,718 |
| + `!**/obj/**` | 2,814 | +170 | 801,461 |
| + `!**/node_modules/**` | 3,129 | +315 | 848,566 |
| + `!**/generated.cs` (literal name) | 3,273 | +144 | 848,566 |
| + `!**/*.generated.cs` (**wildcard** name) | ~4,622 | **+1,349** | 862,374 |
| runner query (regex-extended, reference) | 4,375–4,572 | — | 876,017 |

The marginal costs add up exactly to the measured total (1,290 + 1,198 + 156 + 170 + 315 +
144 + 1,349 = 4,622 ms), independently confirming the per-stage attribution.

Bare `Directory.EnumerateFiles(root, "*", AllDirectories)` over the same dataset:
**1,188–1,320 ms**, statistically identical to the engine's zero-pattern run (1,251–1,331 ms)
→ the engine's own loop, frontier, decision and yield machinery add ~0 ms; the ~1.3 s floor
is 100 % BCL enumeration.

## 5. Allocation Attribution

| Bucket | Allocated | Avg / call |
|--------|-----------|------------|
| Entry processing total (path build + matching + decision) | **0 B** | 0 B |
| Enumeration buckets (per-entry FullPath strings, incl. results) | **330.58 MB** | 345.22 B |
| Harness-measured total | **330.59 MB** | — |

The Phase 4 zero-allocation hot path holds exactly: nothing allocates during path
composition or matching. Every one of the 1,004,097 entries allocates one FullPath string in
the BCL enumerator (~345 B); matched results reuse that string (no second allocation).

## 6. Findings

1. **The #1 controllable cost is wildcard file-pattern matching.** One negated wildcard-name
   pattern (`!**/*.generated.cs`) costs ~1.35 µs/entry ≈ **29 % of the entire run**; the
   positive wildcard `**/*.cs` costs ~1.19 µs/entry. Literal-name patterns cost 0.14–0.31
   µs/entry — about **10× cheaper**. GitIgnore "last matching rule wins" semantics prevent
   short-circuiting, so every entry evaluates all five patterns; the expensive two are the
   wildcard ones.
2. **The #2 cost is the BCL enumerator, and it is fixed.** ~1.29 s of 4.6 s (28 %) is
   `System.IO.Enumeration.FileSystemEnumerator`: per-`MoveNext` `lock (_lock)`, Win32
   `NtQueryDirectoryFile` / `FindNextFile` work, and one FullPath string per entry (345 B).
   Bare enumeration costs the same — the engine adds nothing on top. The per-directory
   enumerator style of the engine (4,097 enumerators) shows ~3× the `Enter_Slowpath` sample
   share (48 %) of a single-tree enumerator (15 %) on the otherwise identical BCL code.
3. **GC is significant but not split-out.** 73–74 gen0 + 1–2 gen1 collections per iteration
   on 330.58 MB; the net10-preview runtime exposes no per-collection pause API
   (`GCMemoryInfo.TotalPauseDuration` unavailable), so pause time could not be attributed
   separately; it is embedded in the buckets above (GC suspends wherever the mutator was).
4. **Highest-leverage remediation candidates** (not implemented — read-only phase):
   - Pre-index GitIgnore patterns by trailing literal suffix (`.cs`, `.generated.cs`) and
     evaluate the literal suffix before entering the recursive segment matcher, skipping the
     wildcard engine when the path cannot match. Estimated saving ≈ 2.5 s of 4.6 s (the two
     wildcard patterns).
   - For folder-exclusion patterns (`!**/X/**`), the concrete-prefix anchor (already
     implemented for pruning) can gate pattern evaluation; literal folder patterns cost
     ~0.2 µs/entry.
   - To attack the 1.3 s floor, the library would have to bypass `FileSystemEnumerator`
     (e.g. pooled `FindFirstFile`/`NtQueryDirectoryFile` calls) — outside the pattern-matching
     scope; the floor is BCL-bound, not engine-bound.

## 7. Measurement Caveats

- **Sampler under-attributes JIT-leaf managed code.** Timestamps (+ sweep) say matching is
  ~63 % of the loop, but sampled leaf shares show the matcher at <6 %; the sampler heavily
  biases toward the enumerator's lock-slow-path and "CPU_TIME" leaves. Both sampling runs and
  both independent timing techniques were used, with timings mutually consistent; sampling
  percentages should be read directionally, not absolutely.
- **GC pause time and lock-wait time are not separated** within the enumeration bucket; the
  1.45 µs/entry figure includes Win32 I/O and any GC suspension on the mutator thread.
- Environment: Windows, .NET 10 preview runtime, same machine as Phases 2–4, dataset fully
  cached (cold OS cache would shift the enumeration floor upward).