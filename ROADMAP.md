# Project Roadmap: Jeninnet.FileQuery

This roadmap outlines the strategic direction for **Jeninnet.FileQuery**, focusing on performance, observability, reliability, and ecosystem integration.

---

## 🎯 Vision

To be the fastest, most predictable, and most flexible file discovery engine for the .NET ecosystem, adhering to a zero-allocation matching philosophy.

---

## Phase 1: The "Production-Ready" Foundation (v1.0)

*Goal: Ensure the foundation is immutable, leak-proof, and predictable.*

- [x] **Physical API Immutability**: Refactor `FileQueryOptions` and `PatternInput` to be truly immutable (constructor-based or frozen collections) to ensure thread-safety.
- [x] **Total IO Abstraction**: Complete `IFileSystem` coverage to ensure 100% decoupling from `System.IO`, enabling virtual/mocked filesystem support.
- [x] **Cross-Platform Baseline**: Establish and publish performance benchmarks for Windows, Linux (Ubuntu), and macOS.
- [x] **Matcher Order Optimization**: Ensure that "fail-fast" logic is applied to pattern evaluation to minimize execution time.

---

## Phase 2: The "Observability" Release (v1.1 - v1.3)

*Goal: Provide deep insights into the enumeration and matching process.*

- [x] **Progress Reporting**: Support `IProgress<T>` in `EnumerateFilesAsync` for real-time scan statistics.
- [x] **Match Diagnostics**: Introduce an optional audit mode to explain why a file was included or excluded.
- [x] **Deep Cancellation**: Ensure `CancellationToken` propagation is verified across all hot paths. *(async paths; sync cancellation is tracked in the pre-v2.0.0 hardening plan)*
- [x] **Enhanced Error Recovery**: Provide configurable strategies for handling IO errors (`Skip`, `Retry`, `Abort`).
- [x] **Pattern Validation Framework**: Implement early detection and reporting for malformed patterns.
- [x] **Improved Path Validation**: Enhance `FileQueryValidator` for eager path structure validation.
- [ ] **OpenTelemetry Integration**: Export metrics and spans for structured observability. *(not implemented in v1.x; delivered for v2.0.0 by H-02)*
- [ ] **AOT Compilation Validation Pipeline**: Establish strict Native AOT compatibility checks in CI to ensure zero-reflection guarantees remain unbroken. *(not implemented in v1.x; delivered for v2.0.0 by H-04)*
- [ ] **Roslyn Analyzers for Pattern Validation**: Provide design-time warnings and code fixes for malformed GitIgnore, Glob, or Regex patterns. *(not implemented in v1.x; delivered for v2.0.0 by H-03)*

---

## Phase 3: The "Quality & Infrastructure" Release (v1.4)

*Goal: Elevate code quality, documentation coverage, and test infrastructure.*

- [x] **Assertion Library Extraction**: Extract shared test infrastructure into `Jeninnet.Testing.Assertions` v1.0.0, a reusable, well-documented assertion library with fluent `.Should()` syntax.
- [x] **XML Documentation Completion**: Add missing XML doc elements (`<param>`, `<exception>`, `<returns>`) across all source files; eliminate all CS1591 and RCS1141 warnings.
- [x] **Enum Hygiene**: Add explicit underlying values to all enum members to maintain binary compatibility.
- [x] **Test Migration**: Replace `TestAssertEx.*` with fluent `.Should()` assertions across 90+ test files.
- [x] **Code Quality Modernization**: Adopt collection expressions, fix minor source issues, and eliminate all analyzer warnings.
- [x] **Editorconfig Refinement**: Enable Roslynator analyzers at warning severity and align style settings.

---

## Phase 4: The "Performance Engineering" Release (v1.5)

*Goal: Establish a measured, predictable, and allocation-conscious traversal engine for large-scale datasets.*

### Performance Engineering

- [x] **Large-Scale Performance Baseline**: Establish a reproducible evaluation workflow using datasets containing up to 1,000,000 files and 4,096 directories.
- [x] **Filesystem Enumeration Optimization**: Eliminate redundant per-entry filesystem attribute lookups by consuming attributes directly from .NET filesystem enumeration.
- [x] **Relative Path Allocation Reduction**: Replace per-entry relative-path string construction with a reusable `ArrayPool<char>`-backed buffer.
- [x] **GitIgnore Suffix Fast Path**: Add literal-suffix rejection for eligible wildcard GitIgnore patterns before recursive matching.
- [x] **Performance Investigation**: Profile traversal, matching, allocation, and filesystem enumeration costs using measured large-scale workloads.
- [x] **Performance Stop-Gate**: Identify the remaining dominant cost as .NET/BCL and operating-system filesystem enumeration and stop further optimization where additional native or platform-specific complexity would not provide sufficient value.

### Validation

- [x] **Large Dataset Validation**: Validate traversal against a reproducible 1,000,000-file dataset.
- [x] **Match Stability**: Verify stable match results across optimization phases for equivalent queries.
- [x] **Allocation Measurement**: Measure and document allocation behavior across the traversal hot path.
- [x] **Regression Coverage**: Add focused tests for the optimized traversal and relative-path buffering paths.
- [x] **API Stability**: Preserve the existing public API and matching semantics throughout the optimization cycle.
- [x] **Build and Quality Gates**: Maintain successful builds, test suites, analyzer checks, and formatting verification throughout the cycle.

### Outcome

The v1.5 performance cycle is complete.

The remaining dominant cost is associated with .NET/BCL filesystem enumeration and underlying operating-system filesystem operations. Further native enumeration work is intentionally outside the scope of v1.5 because the measured benefit does not justify the additional platform-specific complexity and maintenance risk.

The resulting performance measurements and engineering decisions are documented under `docs/performance/`.

---

## Phase 5: The "Ecosystem" Roadmap (v2.x+)

*Goal: Broaden adoption through tooling, integrations, and specialized developer workflows.*

- [x] **Dotnet Global Tool**: Release `dotnet-filequery` CLI for terminal-based file discovery and piping. *(v2.0.0)*
- [x] **IDE Extensions**: Create Visual Studio and VS Code extensions for real-time pattern testing and visualization. *(VS Code shipped; Visual Studio deferred as a follow-up)*
- [x] **Diff-Aware Queries**: Support incremental scans that only process files changed since a specific Git commit or timestamp. *(v2.0.0)*
- [x] **Developer Tooling SDK**: Expose selected tokenizers and classifiers for use by other build-tool authors. *(v2.0.0)*
- [x] **Ecosystem Integrations**: Explore integrations with build systems, development tooling, CI/CD workflows, and repository analysis tools. *(inventory + GitHub Action prototype)*

---

## Phase 5.5: Pre-v2.0.0 Hardening (credibility & capability)

*Goal: Make the published claims true, close the highest-value capability gaps, and raise the
adoption ceiling before v2.0.0. Every item is free of paid services and new runtime dependencies
unless stated. Detailed tasks live in `private/Tasks/pre-v2-hardening/` (`TASK-H-01…16`).*

### Credibility (P0 — required before v2.0.0)

- [x] **H-01 Claims audit**: reconcile the README and roadmap with the code, qualify the
  performance/cancellation claims, and re-audit the three Phase 2 statuses below.
- [x] **H-02 OpenTelemetry instrumentation**: `ActivitySource` for query spans and `Meter` for
  entries/scanned/matched counters and duration, with tests and documentation. No external
  OpenTelemetry package is required; consumers attach exporters.
- [x] **H-03 Roslyn analyzers for pattern validation**: design-time diagnostics and code fixes for
  malformed pattern literals; package, document, and test them.
- [x] **H-04 AOT validation pipeline**: publish and run a Native AOT sample in CI on .NET 10, with
  trimming/AOT warnings as errors.

### One-pass results (P1 — highest user value)

- [x] **H-05 Entry metadata and filters**: `FileQueryEntry` (`Length`, `CreationTimeUtc`,
  `LastWriteTimeUtc`, `Attributes`, `IsDirectory`) with `ExecuteEntries()` and
  `MinSize`/`MaxSize`/`ModifiedAfter`/`ModifiedBefore`/`AttributesToSkip`. The enumeration already
  carries this data, so no extra syscalls.
- [x] **H-06 Directory results**: opt-in directory matches, with documented interaction with
  directory-only patterns and pruning.
- [x] **H-07 Single-path matcher**: public `IsMatch(string)`/`IsMatch(ReadOnlySpan<char>)` and
  `Explain(path)` over a compiled pattern set, for linters and IDE plugins.
- [x] **H-08 CLI `--json` output**: path + metadata for scripts, mirroring `fd`/`ripgrep`.

### Ecosystem and parity (P1/P2)

- [x] **H-09 `.gitignore` file ingestion**: root, nested, `.git/info/exclude`, and optional global
  excludes, with Git-accurate scoping and negation.
- [ ] **H-10 `IFileProvider` adapter package**: query an `IFileProvider` (ASP.NET Core, Razor,
  static files, in-memory providers).
- [x] **H-11 Sync cancellation and progress**: `Execute(query, CancellationToken)` and sync progress
  parity with the async API.
- [ ] **H-12 Brace expansion** `{a,b}` for glob patterns, with escaping rules and tests.
- [x] **H-13 Multiple roots**: query several directories in one pipeline with deterministic order.
- [ ] **H-14 Validation error positions**: surface the offending index/segment in
  `PatternException`/Tooling diagnostics.
- [ ] **H-15 MSBuild task and SARIF exporter**: evaluation-time validation and code-scanning output.

### Deferred (P3)

- [ ] **H-16 Watch mode**: `FileSystemWatcher`-backed continuous queries. Deferred until the
  one-pass result and `.gitignore` work lands.

---

## Platform Support Policy

- **v2.0.0 targets .NET 10.0 only.** .NET 8 reaches end of support on 2026-11-10 and .NET 9 on 2026-05-12, so neither is added as a package target. A `net8.0` target is technically feasible (one `GetAlternateLookup` fallback) but would extend support beyond Microsoft's lifecycle and add a permanent verification matrix.
- **The release after v2.0.0 adds .NET 11.** It multi-targets `net10.0;net11.0`, keeps .NET 10 LTS as the baseline, and still does not target .NET 8/9.
- **No `netstandard2.0` or .NET Framework targets.** The engine relies on spans, `IAsyncEnumerable`, `GeneratedRegex`, and modern file enumeration; supporting them would be a rewrite.

---

## Phase 6: The ".NET 11" Release (post-v2.0.0)

*Goal: Adopt .NET 11 as it ships while keeping the .NET 10 LTS baseline and the current quality bar.*

- [ ] **Multi-target libraries and tool**: `net10.0;net11.0` for the core, CommandLine, DependencyInjection, Tooling, and the global tool.
- [ ] **CI matrix**: build and test both target frameworks on Windows, Linux, and macOS.
- [ ] **AOT and trimming validation on .NET 11**: publish a sample with `PublishAot` and treat trimming warnings as errors.
- [ ] **Performance re-baseline on .NET 11**: refresh `docs/performance/cross-platform-baseline.json`, `runtime-evolution.json`, and the soak reference.
- [ ] **Package validation and documentation**: validate both targets, update the framework badges, README, and the supported-frameworks policy.

> Capability opportunities beyond the roadmap (entry metadata, `.gitignore` ingestion, single-path matching, and others) are analysed in [`docs/articles/library-capability-review.md`](docs/articles/library-capability-review.md).

---

## Future Performance Work

Performance optimization remains an ongoing engineering concern, but future work will be driven by measured evidence rather than speculative optimization.

Potential future investigations include:

- [x] **Cross-Platform Performance Validation**: Compare large-scale traversal behavior on Windows, Linux, and macOS. *(completed; see the cross-platform baseline)*
- [x] **Long-Running Stability Validation**: Validate traversal behavior under repeated large-scale execution. *(completed; see the soak report)*
- [x] **Runtime Evolution Tracking**: Re-evaluate BCL filesystem enumeration performance as newer .NET runtimes become available. *(completed; tracked by `check-runtime.ps1` and the runtime-evolution record)*
- [x] **Native Enumeration Research**: Revisit native or platform-specific enumeration only if future measurements demonstrate a sufficiently large benefit to justify the complexity. *(rejected with measurements; see `docs/performance/native-enumeration-research.md`)*

Future performance work is not part of the v1.5 release scope.
