# Native Enumeration Research

**Decision: rejected for v2.0.0.** Platform-specific enumeration is not worth the measured gain
relative to its cost and risk. This page records the measurements, the analysis, and the
conditions that would justify revisiting the decision.

---

## Question

The v1.5 investigation identified .NET/BCL and operating-system filesystem enumeration as the
dominant remaining cost. Would a platform-specific enumerator (Windows `FindFirstFileExW`, Linux
`getdents64`, macOS `getdirentries`) reduce traversal time enough to justify maintaining one
implementation per platform?

---

## Measurements

### v1.5 large-scale investigation

From [Phase 7](phase7-perf-investigation.md), 1,000,001 entries (4,097 directories + 1,000,000
files), Windows, .NET 10:

| Configuration | Median | Marginal cost |
|---|---|---|
| No patterns (enumeration + path/decision work) | 1,746 ms | — |
| `**/*.cs` | 2,085 ms | +339 ms |
| Full query (four patterns) | 2,786 ms | +1,040 ms over no-pattern |
| Matching/path/decision allocations | 0 B | — |
| BCL full-path strings | 345 B/entry | — |

Enumeration and the engine's per-entry work account for ~63% of the full query; pattern matching
adds the rest. The remaining cost is already inside .NET's native enumeration wrapper.

### Managed enumeration ceiling (2026-11-01)

`EnumerationCeilingBenchmark` runs on the controlled 10,000-file tree (100 × 100) on Windows 11,
.NET 10.0.14, `MediumRun`:

| Method | Mean | Error | Allocated |
|---|---|---|---|
| `Directory.EnumerateFiles(..., AllDirectories)` (BCL) | **7.294 ms** | 0.078 ms | 2.47 MB |
| `EngineTraversalOnly` (`!**`) | **9.704 ms** | 0.164 ms | 2.50 MB |
| `EngineWithGitIgnore` (`**;!*.cs`) | **10.335 ms** | 0.100 ms | 2.51 MB |

- The engine adds **33%** over raw BCL enumeration (2.41 ms) for path composition, per-entry
  decisions, and yielded result strings.
- Matching adds **~9%** over engine traversal (0.63 ms), or ~6% of the BCL baseline.
- Allocations are flat: the 2.47 MB is the per-entry full-path strings, not traversal state.

**The ceiling.** A custom enumerator can only replace the 7.29 ms BCL baseline; it cannot remove
the engine's 2.41 ms (or the 0.63 ms matching). .NET's baseline already calls the same native APIs
(`FindFirstFileExW` / `getdents64` / `readdir`) with tuned buffering. The addressable gain is the
difference between the BCL wrapper and a hand-rolled syscall loop — a fraction of 7.29 ms, before
per-platform divergence enters the picture.

---

## Native options

| Platform | API | Current .NET mapping | Potential gain | Cost |
|---|---|---|---|---|
| Windows | `FindFirstFileExW` with `FindExInfoBasic` + `FIND_FIRST_EX_LARGE_FETCH`, `FindNextFileW` | `FileSystemEnumerable` / `Directory.Enumerate*` already use `FindFirstFileExW` | Larger buffers and skipping 8.3 names are partially already applied by .NET | P/Invoke, `WIN32_FIND_DATA` marshalling, long paths (`\\?\`), reparse points, error mapping |
| Linux | `openat` + `getdents64` + `fstatat` | `Directory.Enumerate*` uses `getdents64` and `statx` | Avoiding managed attribute mapping for entries the query never needs | Raw `dirent` parsing, `d_type` limits, encoding, symlinks, mount boundaries |
| macOS | `getdirentries`/`readdir` + `fstatat` | `Directory.Enumerate*` uses `readdir` and `fstatat` | Similar to Linux; Arm64 runner variance already dominates measurements | Darwin-specific structs, case-insensitive filesystem semantics, code signing/AOT considerations |

All three require per-OS unsafe code, and the engine would still need its managed projection:
relative paths, attributes, reparse-point detection, inaccessible-entry recovery, and deterministic
ordering. The native call is the cheap part; the semantics are the expensive part.

---

## Cost, risk, and maintenance

| Area | Impact |
|---|---|
| Implementations | One per supported OS family (Windows, Linux, macOS), plus a managed fallback for unsupported platforms and tests for each. |
| Safety | `unsafe`/`LibraryImport` code, buffer lifetimes, pointer arithmetic, and struct layout pinning; a mistake is a crash or memory corruption, not an exception. |
| Semantics | Long paths (`\\?\`, UTF-8 vs UTF-16), reparse points/symlinks, mount boundaries, inaccessible entries, directory-change races, and error-code mapping must match `IFileSystem` everywhere. |
| API | `IFileSystem` is public; a native implementation must preserve every contract and the existing tests, including the error-recovery strategies. |
| AOT | P/Invoke is AOT-compatible, but each OS adds trimming/AOT validation surface and a larger native footprint. |
| Maintenance | Behavior must be re-validated on every .NET runtime and OS update; the BCL implementation improves for free. |
| Verification | Three new integration matrices, benchmarks per OS, and a native-vs-managed equivalence suite. |
| Risk | Silent divergence in enumeration order, attributes, or edge cases is hard to detect and worse than a small performance loss. |

The v1.5 stop-gate already concluded that further native enumeration was outside scope because the
measured benefit did not justify the complexity. This research confirms that conclusion with a
direct ceiling measurement: the part a native enumerator could replace is the part .NET already
implements natively.

---

## Decision

**Reject platform-specific enumeration for v2.0.0.** No prototype was built because the ceiling
measurement bounds the addressable gain to a fraction of the 7.29 ms BCL baseline, while the
engine's own 2.41 ms overhead and the matching cost are larger, portable, and lower-risk targets.

### Revisit criteria

Revisit only when **all** of the following hold:

1. A future .NET runtime or OS API measurably regresses BCL enumeration, or exposes a substantially
   faster batch API.
2. A prototype beats the managed baseline by **≥20% end-to-end** on the controlled dataset on **at
   least two** supported operating systems, measured with `Medium`/default runs.
3. The prototype passes the full equivalence suite (attributes, symlinks, long paths, error
   recovery, determinism) on those platforms.
4. The maintenance owner accepts the per-OS code and the runtime-update verification burden.

If those conditions are met, prototype on the controlled tree first and publish the comparison
here before changing `IFileSystem`.

---

## References

- [Phase 7 — Re-Profile](phase7-perf-investigation.md) — the 1M-file measurement and stop-gate.
- [Cross-Platform Baseline](cross-platform-baseline.md) — the controlled 10k-file benchmark.
- [Long-Running Stability](long-running-stability.md) — the soak evidence.
- [Runtime Evolution](runtime-evolution.md) — how runtime changes trigger a re-baseline.
