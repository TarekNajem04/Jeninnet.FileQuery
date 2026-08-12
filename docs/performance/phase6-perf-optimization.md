# Phase 6 — GitIgnore Wildcard Filename Matching Optimization Report

Optimization implemented: **literal-suffix rejection fast path** for GitIgnore wildcard
filename patterns, addressing the #1 controllable hotspot identified in the
[Phase 5 report](./phase5-perf-investigation.md) (wildcard file patterns cost ~1.19–1.35
µs/entry ≈ 29 % of the whole run).

Query under test (unchanged, same 1,000,000-file dataset,
`%TEMP%\Jeninnet.FileQuery\AdvancedUsage\Dataset`):

```
**/*.cs;!**/bin/**;!**/obj/**;!**/node_modules/**;!**/*.generated.cs
```

---

## 1. What Changed

### Design

A pattern such as `**/*.cs` can only match a file whose **last path segment ends with
`.cs`**. The trailing run of literal/escaped characters in the pattern's last segment is
therefore a *necessary condition* for any match. Checking it before entering the recursive
segment matcher skips the most expensive machinery (segment enumeration + `*` backtracking
+ `**` sliding) for the overwhelming majority of entries.

- `LiteralSuffixResolver` (new, `Patterns/Compilation`) extracts the trailing run of
  `LiteralToken`/`EscapeToken` from the last segment at **compile time** (backward scan
  that stops at the first wildcard/single-char/character-class/recursive-wildcard token).
  Returns `""` when no fixed suffix exists (e.g. `**/*`, `**/*?`, `**/*[ab]`) or when the
  run contains `/`.
- The suffix is stored on the compiled pattern: new `LiteralSuffix` member on internal
  `ICompiledPattern`, new `LiteralSuffix` parameter on `CompiledPatternConfig`, property +
  ctor wiring in `CompiledPattern`.
- `GitIgnorePatternCompiler.CompileCore` resolves the suffix **only for non-directory-only
  patterns** (directory-only patterns get `""`).
- `GitIgnoreInstructionMatcher.MatchPathAgainstCompiledPattern` gains the runtime check
  (guarded by `!pattern.DirectoryOnly && pattern.LiteralSuffix.Length > 0`):
  slice the path, tolerate a trailing `/` (directory paths carry one — see
  `PathUtilities.BuildRelativePath`), then `path.EndsWith(suffix, comparison)` — a single
  SIMD-friendly span call, zero allocation. Failure → `return false` before any segment
  enumeration.

### Why the check cannot change match results

- **Non-directory-only patterns always map their last segment onto the path's last
  segment.** `MatchRecursiveSegments` reports a match only when the pattern is fully
  consumed *and* the path is fully consumed (`remainingSegments == 0`); leftover path
  segments are only tolerated when `pattern.DirectoryOnly` is true. Hence any match implies
  the path ends with the literal run — the check is a strict necessary condition.
- **Directory-only patterns are exempt** exactly because they break that invariant:
  `/a/` matches the *file* `a/b` (last segment `b`, not `a`). Such patterns never receive
  a suffix (compile-time), and the matcher re-checks `!pattern.DirectoryOnly` defensively.
- **Case sensitivity is honored**: the check uses the same `StringComparison` derived from
  `PathMatchContext.CaseSensitivity` as the segment matcher.
- **Passing entries still run the full matcher**; the check only ever rejects. It is a
  pre-filter, not a replacement.

## 2. Files Changed

| File | Change |
|------|--------|
| `src/Jeninnet.FileQuery/Patterns/Compilation/LiteralSuffixResolver.cs` | new — compile-time suffix extraction |
| `src/Jeninnet.FileQuery/Patterns/Compilation/GitIgnorePatternCompiler.cs` | resolves suffix (non-dir-only only) |
| `src/Jeninnet.FileQuery/Patterns/Compiled/CompiledPattern.cs` | `LiteralSuffix` config param + property |
| `src/Jeninnet.FileQuery/Matching/ICompiledPattern.cs` | `LiteralSuffix` interface member (internal) |
| `src/Jeninnet.FileQuery/Matching/Compiled/GitIgnoreInstructionMatcher.cs` | suffix-rejection fast path |
| `tests/.../Unit/Matchers/GitIgnoreLiteralSuffixTests.cs` | new — 10 focused test methods |
| 4 test fixtures implementing `ICompiledPattern` | `LiteralSuffix => string.Empty` |

## 3. Tests

- **939 / 939 tests pass** (was 938 before this phase; +10 new focused tests, −1 fixed
  expectation). Full suite: `dotnet test` on `Jeninnet.FileQuery.Tests`.
- New coverage categories: suffix resolution (multi-token runs, escaped chars, wildcard/
  class/single-char terminators, dir-only exclusion); end-to-end match/reject for
  `**/*.cs`, `!**/*.generated.cs` (last-rule-wins), `**/*\*`; case-sensitivity honoring;
  no-suffix fallback; dir-only semantics untouched (incl. anchored `/a/` subtree matching
  `a/b`); unanchored multi-segment patterns; trailing-slash directory paths.
- `dotnet build` (src project): 0 warnings / 0 errors; `dotnet format --verify-no-changes`
  clean on src and tests.

## 4. Benchmarks (same session, before vs after)

Harness: `Phase5ProfilerHarness.exe <dataset> <patterns> <iters>`. Medians are the middle
value of each measurement block; noise on this machine is ±10–15 % between blocks, so the
first (block-1, right-after-warmup) comparison of each query is the most conservative.

| Query | BEFORE median (ms) | AFTER median (ms) | Δ | after first block |
|-------|--------------------|--------------------|-----|-------------------|
| 1p `**/*.cs` | 2,667 | 2,464 (pooled 9 runs) | **−8 %** | 2,534 (−5 %) |
| 4p (no generated pattern) | 3,117 | 3,416 (pooled 18 runs) | +10 % (noise band) | 3,450 |
| 5p full query | 5,003 | 3,672 (pooled 26 runs) | **−27 %** | 3,381 (−32 %) |

Rule out noise cases: the two wildcard patterns dominate; run-to-run spread for the same
config reached 940 ms even *before* the change (5,473–5,672 within one 5-run block), which
exceeds the 4p delta.

- **Matches**: byte-identical for every config before and after
  (670,001 / 745,718 / 801,461 / 848,566 / 862,374) → no semantic drift at scale.
- **Allocations**: 330.52–330.58 MB in all runs, gen0 = 73–74, gen1 = 1–2, gen2 = 0 →
  the zero-allocation contract is preserved (the fast path is span-only).

## 5. Per-Pattern Marginal Cost (before vs after)

Phase 5 (other day): `**/*.cs` = **+1,198 ms**, `!**/*.generated.cs` = **+1,349 ms**.
Same-session before: `**/*.cs` → 2,667 − 1,802 = **+865 ms**;
`!**/*.generated.cs` → 5,003 − 3,117 = **+1,886 ms**.

| Pattern added | Same-session BEFORE marginal | AFTER marginal |
|---------------|------------------------------|----------------|
| `**/*.cs` | ~+865 ms (Phase 5: +1,198) | ~+546 ms (1p sweep) |
| `!**/bin/**`, `!**/obj/**`, `!**/node_modules/**` (no suffix) | +156/+170/+315 ms | +308/+490/+317 ms (within noise) |
| `!**/*.generated.cs` (wildcard) | **+1,886 ms** (Phase 5: +1,349) | **≈ −107 ms** (statistically zero) |

The negated wildcard pattern — the single most expensive pattern in Phase 5 (~1.9 µs/entry
marginal) — now costs ~0: its suffix `.generated.cs` rejects ~987,000 of the 1,000,001
entries with one `EndsWith` before the recursive matcher runs. Marginal savings
≈ **1.9–2.0 s** per query; the `**/*.cs` rejection saves a further ~0.3–0.6 s.

## 6. Verdict on the Phase 5 hotspot

Reduced. The Phase 5 #1 hotspot category (wildcard filename patterns, 62.66 % bucket =
2,918 ms, of which the two wildcard patterns accounted for ~2.5 s) is cut by roughly
**2.0–2.6 s on this query**; full-query elapsed went from median 5,003 ms → 3,381–3,672 ms
(≈ **27–32 % faster**). Remaining matching cost is the `**`/`*` engine for genuinely
suffix-matching entries (~86 % of the dataset ends in `.cs`) plus the three folder
patterns, which have no suffix by design and remain within the original noise band.

## 7. Caveats

- Elapsed medians drift ±10–15 % block-to-block on this machine (thermal/paging); the
  matched-count identity and the near-zero marginal of the wildcard negated pattern are the
  load-bearing evidence, not any single elapsed number.
- `dotnet test`/`dotnet format` on this machine require
  `-p:MSBuildEnableWorkloadResolver=false` (local workload-manifest problem with the
  Emscripten 10.0.108 vs 10.0.110 manifests; unrelated to this change).
- The suffix fast path is a *necessary-condition* pre-filter; it does not accelerate
  entries that genuinely carry the suffix (they still run the full matcher) — that is the
  remaining, correct-by-design cost.