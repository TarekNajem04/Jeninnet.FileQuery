# Library Capability Review

_Date: 2026-11-01. Status: refreshed after the v2.0.0 hardening cycle. Peer information was
collected from official docs and package pages; verify a specific peer detail before relying on it._

This review answers two questions:

1. Where does the library stand compared with the file-matching/globbing libraries and tools that
   users compare it with?
2. Which additions would make it materially more useful, and in what order?

The original review identified two gaps: **trust in the published claims** and **one-pass results
with metadata**. Both were closed during the v2.0.0 hardening cycle. The only runtime capability
still deliberately deferred is watch mode; the Visual Studio extension is also deferred.

---

## 1. What the library does today

| Area | Capability |
|---|---|
| Pattern dialects | GitIgnore (negation, anchoring, directory-only), Glob (`*`, `**`, `?`, character classes incl. POSIX), Regex (`r:` prefix), Hybrid auto-classification |
| Pattern features | Brace alternation `{a,b}` for Glob/GitIgnore; validation errors with position and segment |
| Precedence | Deterministic GitIgnore → Glob → Regex; last-match-wins within each dialect; first Include across subsets |
| Traversal | DFS/BFS, depth limit, symlink policies (ignore/follow/cycle-safe), inaccessible-entry recovery (skip/retry/abort) |
| Results | Path-only streaming plus `FileQueryEntry` metadata (`Length`, creation/last-write timestamps, attributes, directory state, root) |
| Filters | `MinSize`, `MaxSize`, `ModifiedAfter`, `ModifiedBefore`, `AttributesToSkip`, all applied in the existing pass |
| Directory results | `IncludeDirectories()` yields matching directories from the same walk |
| Single-path matching | `FileQueryPatternSet.Compile(...).IsMatch` (string/span) and `Explain` |
| Ignore files | `UseGitIgnoreFiles()` loads root/nested `.gitignore`, `.git/info/exclude`, and an optional global excludes file with Git scoping and negation |
| Incremental | `RestrictToPaths` allow-list; the CLI fills it from Git commits/timestamps |
| Multiple roots | `FileQueryBatch` with deterministic order and `FileQueryEntry.RootPath` |
| IO | `IFileSystem` abstraction (real, mock, virtual); `Jeninnet.FileQuery.FileProviders` adapts `IFileProvider` |
| Observability | Sync and async progress, per-entry audit diagnostics, built-in `ActivitySource`/`Meter` |
| Tooling | `dotnet-filequery` (`--json`, `--explain`, diff-aware), VS Code extension, GitHub Action |
| Build integration | Roslyn analyzer (`JNQ1001`), MSBuild task and SARIF 2.1.0 exporter |
| Performance | Zero-allocation matching, pooled relative paths, literal-suffix fast path, measured 10k/1M baselines |
| Platform | .NET 10 / C# 14, AOT-ready by design, Native AOT publish/run smoke test in CI |
| Quality | 0-warning build, 1,167 tests, DocFX with warnings-as-errors, cross-platform and soak baselines |

---

## 2. Peer comparison

Rows are capabilities; columns are the closest alternatives. “Reference tools” are `fd`/`ripgrep`
— not libraries, but the behaviour users expect from a file-query tool. `—` means the peer was not
assessed for that row.

| Capability | **Jeninnet.FileQuery** | Microsoft.Extensions.FileSystemGlobbing | DotNet.Glob | GitignoreParserNet / MAB.DotIgnore | BCL `EnumerationOptions` | fd / ripgrep |
|---|---|---|---|---|---|---|
| GitIgnore dialect | ✅ | ❌ | ❌ | ✅ (parse only) | ❌ | ✅ (ignore files) |
| Reads `.gitignore` files | ✅ (opt-in; nested + info/exclude + global) | ❌ | ❌ | ✅ (parser; nesting is manual) | ❌ | ✅ (nested + global) |
| Single-path match without traversal | ✅ | ✅ (`Matcher.Match`) | ✅ (`Glob.IsMatch`) | ✅ (`Accepts`/`Denies`) | ❌ | n/a |
| Entry metadata in results | ✅ | ❌ (`Path`/`Stem` only) | ❌ | ❌ | ❌ (paths) | ✅ (JSON: size/time) |
| Metadata filters (size/time/attributes) | ✅ | ❌ | ❌ | ❌ | attributes only | ✅ |
| Include/exclude diagnostics | ✅ | ❌ | ❌ | ❌ | ❌ | partial (`--debug`) |
| Progress reporting | ✅ (sync + async) | ❌ | ❌ | ❌ | ❌ | ✅ |
| Cancellation | ✅ (sync + async) | n/a | n/a | n/a | ✅ | ✅ |
| Virtual file system | ✅ (`IFileSystem` + `IFileProvider` adapter) | ✅ (`IFileProvider`, `InMemoryDirectoryInfo`) | ❌ | ❌ | n/a | n/a |
| Diff-aware queries | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ (external `git` piping) |
| AOT-ready | ✅ (design + CI smoke) | ✅ | ✅ | ✅ | ✅ | n/a |
| CLI / tooling | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ (tools) |
| Target frameworks | net10.0 runtime/tool packages; netstandard2.0 analyzer | netstandard2.0, net462+ | netstandard1.1+ (stale) | netstandard1.3 / net8.0 | net8+ | binaries |

### Additional capabilities shipped in v2.0.0

- **Directory results** from the existing traversal, with directory-only patterns, negation, and
  pruning preserved.
- **Brace expansion** `{a,b}` with nesting, escaping, and regex exclusion.
- **Multiple roots** with deterministic ordering and per-entry root metadata.
- **JSON output** from the CLI, including size, timestamps, attributes, and directory state.
- **Roslyn analyzer** (`JNQ1001`) with a safe code fix, plus an **MSBuild task** (`JNQ2001`) and
  **SARIF 2.1.0** export for code scanning.
- **Built-in OpenTelemetry instrumentation** (`ActivitySource`/`Meter`) without an external package.
- **Native AOT publish/run smoke test** in CI with trimming/AOT warnings treated as errors.
- **`IFileProvider` adapter package** for ASP.NET Core, embedded, and in-memory providers.

### What the peers do better

- **Microsoft.Extensions.FileSystemGlobbing** remains the default in ASP.NET Core and works over
  `IFileProvider`/`DirectoryInfoBase`, including `InMemoryDirectoryInfo` for tests. It is glob-only,
  path-only, and has no diagnostics; the adapter package now closes its main ecosystem advantage.
- **DotNet.Glob** is a fast in-memory glob matcher with span support and case options; it does not
  traverse or read ignore files, and its last stable release is old.
- **GitignoreParserNet** implements the GitIgnore specification closely (nested files when fed
  manually, `Accepts`/`Denies`/`Inspects`). **MAB.DotIgnore** is a simpler, older parser. Neither
  traverses or matches other dialects.
- **fd/ripgrep** still set the usability bar for terminal workflows: `.gitignore` awareness, hidden
  and system filters, type filters, size/time filters, JSON output, and parallel traversal.

### What Jeninnet.FileQuery does better

- The **only** option in this comparison that combines three dialects with deterministic
  precedence, traversal, metadata results, filters, diagnostics, progress, error recovery,
  diff-aware scans, and a virtual filesystem in one pipeline.
- **Performance discipline**: zero-allocation matching, pooled paths, a measured 10k-file ceiling,
  cross-platform and soak baselines.
- **Build-tool surface**: Tooling, Roslyn, and MSBuild/SARIF packages share one validation model —
  no peer offers this.
- **Ecosystem packaging**: CLI, VS Code extension, GitHub Action, DI, and a versioned SDK.

---

## 3. Claims audit (closed in v2.0.0)

The original review found published claims that the code did not back. Each was resolved and is now
covered by code, tests, and documentation.

| Claim | Original finding | v2.0.0 resolution |
|---|---|---|
| **OpenTelemetry integration** | No `ActivitySource`, `Meter`, or package reference | H-02 added stable `ActivitySource`/`Meter` instrumentation, five tests, and a guide. No external OpenTelemetry package is required. |
| **Roslyn analyzers** | No `DiagnosticAnalyzer`/`CodeFixProvider` or analyzer project | H-03 shipped `Jeninnet.FileQuery.Analyzers` (`JNQ1001`) with a safe code fix, tests, and a CI consumer-install job. |
| **AOT validation pipeline** | No `PublishAot` or AOT step in CI | H-04 added the `AotSmoke` host and a linux-x64 `PublishAot` CI job that runs a real query with warnings as errors. |
| **“1M–2M files/sec”** | Measured ~573k entries/s in a warm-cache enumeration benchmark | H-01 removed the unsupported claim and qualified the measured baseline. |
| **“Deep cancellation across all hot paths”** | Only the async API accepted a token | H-11 added synchronous `Execute`/`ExecuteEntries` overloads with cancellation and progress parity. |
| **Phase 2 Done statuses** | `TASK-2-07/08/09` were marked Done without code | The three tasks are documented as rejected for v1 and superseded by the completed H-02/H-03/H-04 tasks. |

The repository now runs `scripts/docs/check-readme-claims.ps1` in the documentation workflow, and the
release checklist requires the claims and link checks before tagging. This prevents the same drift
from returning silently.

---

## 4. Shipped additions (v2.0.0)

| Addition | Priority | Status | Evidence |
|---|---|---|---|
| Entry metadata and filters | P1 | **Shipped** (H-05) | `FileQueryEntry`, `ExecuteEntries`, `MinSize`/`MaxSize`/`ModifiedAfter`/`ModifiedBefore`/`AttributesToSkip` |
| `.gitignore` ingestion | P1 | **Shipped** (H-09) | `UseGitIgnoreFiles`, root/nested/info/global loading, CLI `--ignore-files` |
| Single-path matcher | P1 | **Shipped** (H-07) | `FileQueryPatternSet.IsMatch` (string/span) and `Explain` |
| Directory results | P1 | **Shipped** (H-06) | `IncludeDirectories()` with pruning and negation semantics |
| Sync cancellation and progress | P2 | **Shipped** (H-11) | Sync `Execute`/`ExecuteEntries` token and progress overloads |
| `IFileProvider` adapter | P2 | **Shipped** (H-10) | `Jeninnet.FileQuery.FileProviders`, physical/in-memory tests |
| CLI `--json` | P2 | **Shipped** (H-08) | NDJSON with metadata, explain composition, conflict rules |
| Brace expansion | P2 | **Shipped** (H-12) | Nested/escaped alternation, regex exclusion, bounded expansion |
| Multiple roots | P2 | **Shipped** (H-13) | `FileQueryBatch`, `FileQueryEntry.RootPath`, ordered output |
| Validation error positions | P2 | **Shipped** (H-14) | `PatternException.Position`/`Segment` and Tooling diagnostics |
| MSBuild task and SARIF | P3 | **Shipped** (H-15) | `Jeninnet.FileQuery.Build`, `JNQ2001`, `SarifExporter` |
| Watch mode | P3 | **Deferred** (H-16) | Event-loss/lifecycle risk; physical-only; revisit after Phase 6 or on demand |
| Parallel traversal | — | **Not recommended** | Breaks deterministic ordering and can thrash network storage |
| File-content predicates | — | **Out of scope** | Belongs to `ripgrep` and specialized search tools |

---

## 5. What not to add

- **`netstandard2.0` / .NET Framework runtime targets** — the engine’s span/async/regex design would be
  rewritten for a shrinking audience.
- **Native enumeration** — rejected with measurements (see
  [Native Enumeration Research](../performance/native-enumeration-research.md)).
- **A second glob engine or a regex-to-glob converter** — keep one compiled pipeline.
- **Built-in file-content search** — `ripgrep` wins; compose via paths instead.
- **Parallel traversal as a default** — determinism and predictable ordering are differentiators.

---

## 6. Next steps

1. **Keep the claims gate green**: run `scripts/docs/check-readme-claims.ps1`,
   `scripts/docs/check-doc-links.ps1`, and `scripts/docs/check-api-reference.ps1` on every release.
2. **.NET 11 (next release)**: execute Phase 6 after GA — multi-target `net10.0;net11.0`, CI,
   AOT validation, and a performance re-baseline.
3. **Watch mode** only if a concrete IDE/dev-tool consumer needs push updates; define event,
   coalescing, and failure semantics before implementation.
4. **Low-priority debt** (dead code, anchor deduplication, cycle-set comparer, broader symlink
   policy tests) remains tracked in `private/TODO.md` and `private/TECHNICAL_DEBT.md`, not in the
   release scope.

---

## Appendix — evidence

- Claims audited: `README.md` (features, performance), `ROADMAP.md` (Phase 2 and Phase 5.5),
  `private/API.md`.
- Enumeration metadata source: `src/Jeninnet.FileQuery/IO/FileSystem.cs`
  (`TransformEntry(ref System.IO.Enumeration.FileSystemEntry entry)`).
- Ceiling measurement: `docs/performance/native-enumeration-research.md`.
- 1M-file measurement: `docs/performance/phase7-perf-investigation.md`.
- Test and release evidence: `private/TESTING.md`, `private/CHANGELOG.md`.
- Peer sources: Microsoft Learn (`Microsoft.Extensions.FileSystemGlobbing.Matcher`), DotNet.Glob
  GitHub/NuGet, GitignoreParserNet NuGet/GitHub, MAB.DotIgnore NuGet, `fd`/`ripgrep` documentation.

