# Phase 7 — Re-Profile After Phase 6 (Investigation Only)

Investigation-only phase. No production code, API, behavior, or allocation strategy was
touched; the working tree contains only this report (pre-existing committed state:
`465befc perf: optimize GitIgnore wildcard suffix matching`, `b9bd5d8 perf: add Phase 5
performance investigation`).

---

## 1. Benchmark Environment

Same machine and methodology as Phases 2–6: Windows, .NET 10 preview runtime
(SDK 10.0.400-preview.0.26322.102, net10.0), `Phase5ProfilerHarness.exe` rebuilt at the
Phase 6 commit, dataset fully OS-cached:

```
%TEMP%\Jeninnet.FileQuery\AdvancedUsage\Dataset
```

1,000,001 entries (4,097 directories + 1,000,000 files; manifest entry included).

Profiler: `dotnet-trace` 9.0.661903, `dotnet-sampled-thread-time,dotnet-common` profile.
The classic CPU-sampling provider (`Microsoft-Windows-DotNETRuntime:0x14C14FCCBD:4`
— used successfully in Phase 5) produced **zero samples** on this SDK twice (run was
repeated with 5 iterations); a deliberate footnote, not a methodology change — all
sampled results below come from the thread-time trace.

## 2. Exact Query

```
**/*.cs;!**/bin/**;!**/obj/**;!**/node_modules/**;!**/*.generated.cs
```

## 3. Match Count

**862,374 — identical to the Phase 5/6 baseline** in all 16 runs this phase (and in every
Phase 5/6 run). No discrepancy; the Phase 6 suffix fast path remains behavior-preserving
at scale. (939/939 unit tests pass, unchanged suite.)

## 4. Execution Time — Median / Average

Primary query, 10 iterations (two 5-run blocks):

```
3,382  2,806  2,833  2,715  2,639  2,924  2,641  2,560  2,997  2,765  ms
```

| Metric | Phase 5 | Phase 6 (same-session) | Phase 7 |
|--------|---------|------------------------|---------|
| Median | ~4,622 ms | 3,381–3,672 ms | **2,786 ms** |
| Average | ~4,600 ms | ~3,560 ms | **2,826 ms** |
| Best single run | 4,370 ms | 3,134 ms | **2,560 ms** |

Phase 7 is the fastest session yet: **−40 % vs Phase 5**, −18..−24 % vs the Phase 6
session. Machine noise remains ±10–15 % block-to-block (observed since Phase 5), which is
why marginal-cost and profile evidence below are the load-bearing comparisons.

## 5. Allocation and GC

All runs, all sweep configs, Phase 5→6→7 identical:

| Metric | Value |
|--------|-------|
| Allocated | 330.58 MB (345.2 B/entry — one BCL FullPath string per entry) |
| gen0 | 73–74 |
| gen1 | 1–2 |
| gen2 | 0 |
| Matching/path/decision allocations | 0 B (unchanged zero-allocation hot path) |

## 6. Pattern-Isolation Results (same cumulative sweep as Phase 5)

Median of 3 runs per config, same session:

| Patterns | P7 median | Matches | P7 marginal | P5 marginal | P6 marginal |
|----------|-----------|---------|-------------|-------------|-------------|
| (none) | 1,746 | 1,000,001 | — | — | — |
| `**/*.cs` | 2,085 | 670,001 | **+339** | +1,198 | ~+546 |
| + `!**/bin/**` | 2,193 | 745,718 | +108 | +156 | — |
| + `!**/obj/**` | 2,779 | 801,461 | +586* | +170 | — |
| + `!**/node_modules/**` | 3,033 | 848,566 | +254 | +315 | — |
| + `!**/generated.cs` (literal) | 2,771 | 848,566 | −262* | +144 | — |
| + `!**/*.generated.cs` (**wildcard**) | 2,596 | 862,374 | **−175\*** (≈0) | **+1,349** | ≈ −107 |

\* These deltas sit inside the session noise band (the 4p→5p-literal→6p segment trended
down as runs proceeded); the reliable reading is: **every pattern is now cheap
(0–0.6 s)**, versus Phase 5 where the two wildcard patterns alone cost 2.5 s.

**Phase 6 verification:** the wildcard-name hot pattern `!**/*.generated.cs` went from
**+1,349 ms (Phase 5)** to **≈ 0 ms** — its `.generated.cs` suffix rejects ~987,000 of
1,000,001 entries with one `EndsWith` per entry. The positive wildcard `**/*.cs` dropped
from +1,198 to **+339 ms** (67 % of entries genuinely end in `.cs` and still run the
recursive matcher — the remaining unavoidable cost).

## 7. CPU Hotspot Breakdown (thread-time trace, main thread, full query)

| # | Method (exclusive samples) | Share | Inclusive |
|---|----------------------------|-------|-----------|
| 1 | `Monitor.Enter_Slowpath` (`FileSystemEnumerator` per-MoveNext lock) | **57.2 %** | 57.2 % |
| 2 | `FileSystemEnumerator`.MoveNext | **30.6 %** | **91.4 %** |
| 3 | `FileSystemEnumerator`.Init | 3.5 % | 3.6 % |
| 4 | `FileSystemEnumerator`.DirectoryFinished | 2.9 % | 3.0 % |
| 5 | Miscellaneous tail (GC/startup/engine residue) | ~5 % | — |
| … | `GitIgnoreInstructionMatcher.MatchCore` | **0.02 %** | 0.04 % |
| … | `TraversalExecutor.TakeFrame`, `MatchPrecedenceResolver.Resolve`, path build | ≤0.06 % | ~0 % |

Breakdown by area:

| Area | Sample share |
|------|--------------|
| Filesystem enumeration (BCL `FileSystemEnumerator`) | **~94 %** (locks 57 % + work 31 % + init/finish ~6 %) |
| GitIgnore matching | **< 0.1 %** (was 62.7 % of loop time in Phase 5) |
| Glob matching | 0 % (never invoked — 0 calls, as in Phase 5) |
| Regex matching | 0 % (never invoked — 0 calls) |
| Traversal / path processing / result handling | ≤ ~0.1 % sampled (engine residue) |
| Other (GC, runtime, startup) | ~5 % |

**The Phase 6 fast path removed the wildcard hotspot**: the matcher that accounted for
62.66 % (2,918 ms) of the Phase 5 loop is now at the noise floor of the profile, and the
elapsed-time evidence agrees (previous phases: timestamps and sweep said ~63 %, sampler
said <6 %; this phase all three now say ~0 %).

## 8. Comparison with Phase 5

| | Phase 5 | Phase 7 |
|---|---------|---------|
| Full query | ~4,622 ms | 2,786 ms (−40 %) |
| Matching attribution | 2,918 ms / 62.7 % (±1,349 ms for one pattern) | < 0.1 % of samples; ≈ 0.3–0.7 s measurable |
| Enumeration bucket | 1,456 ms / 31.3 % (move-next + lock + strings) | dominant (~94 % of samples) |
| Bare `Directory.EnumerateFiles` floor | 1,188–1,320 ms | 1,177 ms median (1,087–1,321) |
| Engine zero-pattern run | 1,251–1,331 ms | 1,746 ms (session variance; +569 ms over bare today) |

The pattern-matching cost that Phase 5 ranked #1 has been removed; enumeration —
previously the #2 fixed floor — is now the entire profile.

## 9. Comparison with Phase 6

- Full query: Phase 6 same-session median 3,381 ms (first block) → Phase 7 2,786 ms
  (−18 %; part session-to-session variance — Phase 6 pooled median was 3,672 ms, −24 %
  vs that).
- Wildcard-negated pattern marginal: ≈ −107 ms (P6) → ≈ −175 ms / ≈0 (P7) — confirmed
  zero, stable.
- `**/*.cs` marginal: ~+546 ms (P6 sweep) → +339 ms (P7 sweep) — stable-to-slightly-lower.
- Allocations/GC/matches: identical in both phases.

## 10. Current Dominant Bottleneck

**BCL `FileSystemEnumerator` machinery** — `Monitor.Enter_Slowpath` + `MoveNext` +
`Init`/`DirectoryFinished` ≈ **94 % of samples**, and ≈ 1.18 s of the 2.79 s elapsed is
bare `Directory.EnumerateFiles` (hard floor). The measured engine-side remainder is
≈ 1.6 s, split between: ~0.57 s zero-pattern loop overhead over bare enumeration (path
build, context/decision/yield, GC, and the lock premium of 4,097 per-directory
enumerators) and ~1.0 s for all pattern evaluation combined (each pattern now ≤0.3–0.6 s).

## 11. Controllable by Jeninnet.FileQuery, or BCL/OS-Bound?

Mostly **BCL/OS-bound**:

1. The ~1.2 s bare-enumeration floor is BCL `NtQueryDirectoryFile`/`FindNextFile`
   machinery (per-entry FullPath string, per-instance monitor lock). The engine adds no
   allocation and ~0 measurable per-entry cost beyond it.
2. The lock-slowpath premium (57 % of samples vs ~15 % for a single tree-wide
   enumerator — Phase 5 observation) is *amplified* by the engine's per-directory
   enumerator design (4,097 enumerators — required for prune/re-inclusion semantics), but
   the lock itself is inside the BCL class. Reducing it means fewer/longer-lived
   enumerators or bypassing `FileSystemEnumerator` with native directory handles
   (`NtQueryDirectoryFile`/`FindFirstFile` in a pooled loop) — OS-specific, high-risk on
   a preview runtime, and outside the pattern-matching scope this phase series has been
   stepping through.
3. Pattern matching is now ~15 % of the total and already implicit-clean; further gains
   would be a *segment-level* fast path for the ~67 % of entries that genuinely carry
   `.cs` (e.g. recognizing `**/*.cs` as prefix+startswith without `**` backtracking) —
   worth at most ~0.1–0.3 s (~5–10 %), with the explosion risk of re-touching the
   hottest-tested code for a fraction of the Phase 5 win.

## 12. Recommendation for Next Phase

**Evidence-based recommendation: stop the optimization phase series here.**

- Phase 5 target met and exceeded: 4.6 s → 2.8 s (−40 %), zero behavior/API/allocation
  change, hot pattern marginal +1,349 ms → ≈0.
- The remaining dominant cost (lock-slowpath + MoveNext, ~94 % of samples) is inside
  `System.IO.Enumeration.FileSystemEnumerator`; what little of it is engine-shaped
  (per-directory enumerator count) can only be addressed by native enumeration — a risky,
  OS-specific, preview-runtime-hostile project worth far less than its cost, and it does
  not touch the library's actual value proposition (pattern semantics, API, allocation
  guarantees — all already clean).
- The only further pattern-side candidate (segment fast path for `.cs`-carrying entries)
  is measurable but small (~5–10 %) and would churn the most correctness-critical code
  for diminishing returns.

If work is desired regardless, a *bounded, investigation-only* Phase 8 could instrument
the ~0.57 s zero-pattern overhead (Phase-5-style taps, no changes) to decide whether the
engine's own loop has anything actionable; the strong prior from both the sampler
(engine <0.1 %) and Phase 4 (allocation and path work already zeroed) is that it does
not. Otherwise: **no further optimization phase is justified by the evidence.**

---

## Verification (unchanged tree)

- Tests: 939 / 939 passed (`dotnet test tests/Jeninnet.FileQuery.Tests -c Release`).
- Build: 0 warnings / 0 errors (`dotnet build src/Jeninnet.FileQuery -c Release`).
- `dotnet format --verify-no-changes`: clean (src + tests).
- `git status`: no modifications; this report is the only new file.
- Local env note (pre-existing, unrelated): workload-manifest mismatch requires
  `-p:MSBuildEnableWorkloadResolver=false` for dotnet build/test/format on this machine,
  and the dotnet-trace CPU-sampling provider emitted no samples on this SDK (thread-time
  profile used instead).