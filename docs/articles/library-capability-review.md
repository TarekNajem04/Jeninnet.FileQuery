# Library Capability Review

_Date: 2026-11-01. Scope: `Jeninnet.FileQuery` at the tip of `release/v2.0.0-prep`. Peer
information was collected from the official docs and package pages on the same date; verify before
relying on a specific peer detail._

This review answers two questions:

1. Where does the library stand compared with the file-matching/globbing libraries and tools that
   users compare it with?
2. Which additions would make it materially more useful, and in what order?

The short answer: the engine is differentiated by its multi-dialect pipeline, determinism,
diagnostics, and performance discipline. The biggest gaps are **trust in the published claims**
and **one-pass results with metadata**. Both are cheaper to close than the remaining performance
work.

---

## 1. What the library does today

| Area | Capability |
|---|---|
| Pattern dialects | GitIgnore (negation, anchoring, directory-only), Glob (`*`, `**`, `?`, character classes incl. POSIX), Regex (`r:` prefix), Hybrid auto-classification |
| Precedence | Deterministic GitIgnore → Glob → Regex, last-match-wins within GitIgnore |
| Traversal | DFS/BFS, depth limit, symlink policies (ignore/follow/cycle-safe), inaccessible-entry recovery (skip/retry/abort) |
| IO | `IFileSystem` abstraction (real, mock, virtual); no direct `System.IO` in the engine |
| Observability | Async progress snapshots; per-entry audit diagnostics with the responsible pattern |
| Incremental | `RestrictToPaths` allow-list; the CLI fills it from Git commits/timestamps |
| Performance | Zero-allocation matching, pooled relative paths, literal-suffix fast path, measured 10k/1M baselines |
| Platform | .NET 10 / C# 14, AOT-ready by design (no reflection) |
| Ecosystem | NuGet packages (core, CommandLine, DI, Tooling), `dotnet-filequery` global tool, VS Code extension, GitHub Action, ASP.NET-style DI |
| Quality | 0-warning build, ~1,200 tests, DocFX with warnings-as-errors, cross-platform + soak baselines |

---

## 2. Peer comparison

Rows are capabilities; columns are the closest alternatives. “Reference tools” are `fd`/`ripgrep`
— not libraries, but the behaviour users expect from a file-query tool.

| Capability | **Jeninnet.FileQuery** | Microsoft.Extensions.FileSystemGlobbing | DotNet.Glob | GitignoreParserNet / MAB.DotIgnore | BCL `EnumerationOptions` | fd / ripgrep |
|---|---|---|---|---|---|---|
| GitIgnore dialect | ✅ | ❌ | ❌ | ✅ (parse only) | ❌ | ✅ (ignore files) |
| Glob dialect | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| Regex dialect | ✅ | ❌ | ❌ | ❌ | ❌ | rg (content), not path globs |
| Traversal included | ✅ | ✅ (`DirectoryInfoBase`) | ❌ | ❌ | ✅ | ✅ |
| Reads `.gitignore` files | ❌ | ❌ | ❌ | ✅ (parser; nesting is manual) | ❌ | ✅ (nested + global) |
| Single-path match without traversal | ❌ | ✅ (`Matcher.Match`) | ✅ (`Glob.IsMatch`) | ✅ (`Accepts`/`Denies`) | ❌ | n/a |
| Entry metadata in results | ❌ (paths only) | ❌ (`Path`/`Stem` only) | ❌ | ❌ | ❌ (paths) | ✅ (JSON: size/time) |
| Metadata filters (size/time/attributes) | ❌ | ❌ | ❌ | ❌ | attributes only | ✅ |
| Include/exclude diagnostics | ✅ | ❌ | ❌ | ❌ | ❌ | partial (`--debug`) |
| Progress reporting | ✅ (async) | ❌ | ❌ | ❌ | ❌ | ✅ |
| Cancellation | ✅ (async) | n/a | n/a | n/a | ✅ | ✅ |
| Virtual file system | ✅ (`IFileSystem`) | ✅ (`IFileProvider`, `InMemoryDirectoryInfo`) | ❌ | ❌ | n/a | n/a |
| Diff-aware queries | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ (external `git` piping) |
| AOT-ready | ✅ (design) | ✅ | ✅ | ✅ | ✅ | n/a |
| CLI / tooling | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ (tools) |
| Target frameworks | net10.0 | netstandard2.0, net462+ | netstandard1.1+ (stale) | netstandard1.3 / net8.0 | net8+ | binaries |

### What the peers do better

- **Microsoft.Extensions.FileSystemGlobbing** is the default in ASP.NET Core and works over
  `IFileProvider`/`DirectoryInfoBase`, including `InMemoryDirectoryInfo` for tests. Since v10 it
  supports ordered include/exclude evaluation. It is glob-only, path-only, and has no diagnostics.
- **DotNet.Glob** is a fast in-memory glob matcher with span support and case options; it does not
  traverse or read ignore files, and its last stable release is old (3.1.3, with 2021 alphas).
- **GitignoreParserNet** implements the GitIgnore specification closely (nested files when fed
  manually, `Accepts`/`Denies`/`Inspects`). **MAB.DotIgnore** is a simpler, older parser. Neither
  traverses or matches other dialects.
- **fd/ripgrep** set the usability bar: `.gitignore` awareness, hidden/system filters, type filters,
  size/time filters, JSON output, parallel traversal.

### What Jeninnet.FileQuery does better

- The **only** option in this comparison that combines three dialects with deterministic
  precedence, traversal, diagnostics, progress, error recovery, diff-aware scans, and a virtual
  filesystem in one pipeline.
- **Performance discipline**: zero-allocation matching, pooled paths, a measured 10k-file ceiling,
  cross-platform and soak baselines.
- **Build-tool surface**: `Jeninnet.FileQuery.Tooling` exposes classification/validation/
  tokenization without executing a query — no peer offers this.
- **Ecosystem packaging**: CLI, VS Code, GitHub Action, DI, and a versioned SDK.

---

## 3. Credibility gaps to fix before adding features

These are not feature requests; they are accuracy issues that affect adoption more than any
missing capability. Each was verified against the code at the review commit.

| Claim | Evidence | Recommendation |
|---|---|---|
| **OpenTelemetry integration** (README “Observability”; ROADMAP Phase 2 `[x]`) | No `OpenTelemetry`, `ActivitySource`, `Meter`, or package reference anywhere in `src/` | Implement it (spans for queries, metrics for entries/matches) or remove the claim and reopen the roadmap item |
| **Roslyn analyzers for pattern validation** (README; ROADMAP Phase 2 `[x]`) | No `DiagnosticAnalyzer`/`CodeFixProvider`, no analyzer package/project | Implement it (a real differentiator) or remove the claim and reopen the item |
| **AOT compilation validation pipeline** (ROADMAP Phase 2 `[x]`) | No `PublishAot`, `NativeAot`, or AOT step in any workflow | Add a Native AOT publish + run to CI (cheap, high credibility) or reword to “AOT-ready by design” only |
| **“1M–2M files/sec”** (README feature list) | Phase 7: 1,000,001 entries in 1.746 s warm cache ≈ **573k entries/s** | Qualify the number (hardware, warm cache, enumeration-only) or replace it with the measured range |
| **“Deep cancellation across all hot paths”** | Only the async API accepts a `CancellationToken`; `Execute(FileQuery)` has none | Qualify as “async paths”; add sync cancellation if users need it |

Consequence: the internal task board inherited the roadmap and marks `TASK-2-07` (OpenTelemetry),
`TASK-2-08` (AOT pipeline), and `TASK-2-09` (Roslyn analyzers) as **Done**. Re-audit those three
statuses before v2.0.0. A short “claims audit” step in the release checklist would prevent the same
drift in the future.

---

## 4. Recommended additions (ranked)

Impact/effort are relative to this repository (tests, docs, and quality gates included).

### P1 — highest user value

#### 4.1 Entry results with metadata, and metadata filters

**What**: `ExecuteEntries()` (sync/async) returning `FileQueryEntry` with `FullPath`,
`RelativePath`, `Length`, `CreationTimeUtc`, `LastWriteTimeUtc`, `Attributes`, `IsDirectory`,
`IsSymlink`; builder filters `MinSize`, `MaxSize`, `ModifiedAfter/Before`, `AttributesToSkip`.

**Why**: Users almost always need size or timestamps next to the path. Today they must call
`FileInfo` per result — N extra metadata round-trips after the engine already enumerated the
entries. `System.IO.Enumeration.FileSystemEntry` (the source in `FileSystem.TransformEntry`)
already exposes `Length`, `LastWriteTimeUtc`, and `CreationTimeUtc`, so capturing them is
**zero-extra-syscall**. `fd`/`ripgrep` expose these in JSON; no .NET peer does.

**Cost**: medium (public result type, two engine entry points, filters, tests, docs).
**Risk**: low; additive API. Keep the path-only `Execute` for compatibility and allocation parity.
**Payoff**: the single biggest usability upgrade available.

#### 4.2 `.gitignore` file ingestion

**What**: `WithGitIgnoreFiles()` / `AddIgnoreFile(path)` that loads the root `.gitignore`,
nested `.gitignore` files, `.git/info/exclude`, and (optionally) the global excludes file, with
correct nested scoping and negation.

**Why**: “point at a repo and respect its ignore files” is table stakes for repository tooling.
`GitignoreParserNet` and the Rust `ignore` crate exist because this is hard; users currently have
to parse files themselves and hand the patterns to the engine, losing nested-scope semantics.
`fd`/`rg` do it natively.

**Cost**: high (Git precedence/nesting rules, directory-only semantics, encoding, tests).
**Risk**: medium; do not guess the specification — port it against the Git test cases.
**Payoff**: removes the biggest reason to reach for another tool in repository scenarios.

#### 4.3 Single-path matcher (in-memory `IsMatch`)

**What**: a public compiled pattern set with `IsMatch(string relativePath)` and
`IsMatch(ReadOnlySpan<char>)`, plus `Explain(relativePath)` returning the deciding rule.

**Why**: FileSystemGlobbing (`Matcher.Match`), DotNet.Glob (`Glob.IsMatch`), and GitignoreParserNet
(`Accepts`) all offer in-memory matching. It is the natural API for linters, IDE plugins, and
“should this file be included?” checks without touching the disk. The Tooling SDK inspects
patterns but does not match paths.

**Cost**: low–medium (reuse the compiled matcher; expose an allocation-free context).
**Risk**: low; additive.
**Payoff**: makes the engine usable inside editors and analyzers, where traversal is irrelevant.

#### 4.4 Directory results

**What**: an option to yield directories (or entries with `IsDirectory`) instead of files only.

**Why**: cleanup/sync/inventory queries need directory matches; the traversal already evaluates
directories, but the evaluator drops them. Peers either return files only or require a separate
walk.

**Cost**: low (evaluator + result type).
**Risk**: medium semantic questions (directory-only patterns, pruning); document clearly.
**Payoff**: unlocks a class of queries without a second pass.

### P2 — ecosystem fit

| Addition | Why | Cost | Risk |
|---|---|---|---|
| **Sync cancellation and progress** | Parity with async; console apps and UI threads | Medium | Low |
| **`IFileProvider` adapter package** | ASP.NET Core/Razor/static-files ecosystem; FileSystemGlobbing’s main advantage | Medium (new package) | Low–medium |
| **CLI `--json` output** | Pairs with 4.1; `fd`/`rg` parity for scripts | Low after 4.1 | Low |
| **Brace expansion `{a,b}`** | Common glob feature (DotNet.Glob/GlobMatcher); FileSystemGlobbing lacks it | Medium | Medium (ambiguity/escapes) |
| **Multiple roots** | Query several directories in one pipeline | Low | Low |
| **Validation error positions** | Better editor/tooling diagnostics; API.md notes the missing index | Medium | Low |

### P3 — differentiators (only with demand)

| Addition | Why | Cost | Risk |
|---|---|---|---|
| **Watch mode** (`FileSystemWatcher` + diff set) | Continuous queries for dev tools; pairs with diff-aware | High | High (event loss, debouncing, lifecycle) |
| **MSBuild task / SARIF exporter** | Already a roadmap candidate; evaluation-time and code-scanning value | Medium | Medium |
| **Parallel traversal** | Raw speed on multi-core, but breaks deterministic order and can thrash network storage | High | High — **not recommended** |
| **File content predicates** | Belongs to `ripgrep`/specialized tools; out of scope | High | High |

---

## 5. What not to add

- **`netstandard2.0` / .NET Framework targets** — the engine’s span/async/regex design would be
  rewritten for a shrinking audience.
- **Native enumeration** — rejected with measurements (see
  [Native Enumeration Research](../performance/native-enumeration-research.md)).
- **A second glob engine or a regex-to-glob converter** — keep one compiled pipeline.
- **Built-in file-content search** — `ripgrep` wins; compose via paths instead.
- **Parallel traversal as a default** — determinism and predictable ordering are differentiators.

---

## 6. Recommended sequence

1. **Trust (before v2.0.0)**: fix the claims table above, re-audit the three Phase 2 items, qualify
   the performance number, and add a claims check to the release checklist.
2. **One-pass results (v2.x)**: 4.1 metadata + filters, 4.4 directory results, then CLI `--json`.
3. **Ecosystem (v2.x)**: 4.2 `.gitignore` ingestion, 4.3 single-path matcher, and the
   `IFileProvider` adapter.
4. **.NET 11 (next release)**: execute the roadmap’s Phase 6 (multi-target `net10.0;net11.0`, CI,
   AOT validation, re-baseline).

The order is deliberate: metadata results are additive and cheap; `.gitignore` ingestion is the
largest user-visible gap; trust fixes remove a risk that no feature can offset.

---

## Appendix — evidence

- Claims audited: `README.md` (features, performance), `ROADMAP.md` (Phase 2), `private/API.md`.
- Enumeration metadata source: `src/Jeninnet.FileQuery/IO/FileSystem.cs`
  (`TransformEntry(ref System.IO.Enumeration.FileSystemEntry entry)`).
- Ceiling measurement: `docs/performance/native-enumeration-research.md`.
- 1M-file measurement: `docs/performance/phase7-perf-investigation.md`.
- Peer sources: Microsoft Learn (`Microsoft.Extensions.FileSystemGlobbing.Matcher`), DotNet.Glob
  GitHub/NuGet, GitignoreParserNet NuGet/GitHub, MAB.DotIgnore NuGet, `fd`/`ripgrep` documentation.
