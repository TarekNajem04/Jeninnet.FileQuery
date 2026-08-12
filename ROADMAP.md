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
- [x] **Deep Cancellation**: Ensure `CancellationToken` propagation is verified across all hot paths.
- [x] **Enhanced Error Recovery**: Provide configurable strategies for handling IO errors (`Skip`, `Retry`, `Abort`).
- [x] **Pattern Validation Framework**: Implement early detection and reporting for malformed patterns.
- [x] **Improved Path Validation**: Enhance `FileQueryValidator` for eager path structure validation.
- [x] **OpenTelemetry Integration**: Export metrics and spans for structured observability.
- [x] **AOT Compilation Validation Pipeline**: Establish strict Native AOT compatibility checks in CI to ensure zero-reflection guarantees remain unbroken.
- [x] **Roslyn Analyzers for Pattern Validation**: Provide design-time warnings and code fixes for malformed GitIgnore, Glob, or Regex patterns.

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

- [ ] **Dotnet Global Tool**: Release `dotnet-filequery` CLI for terminal-based file discovery and piping.
- [ ] **IDE Extensions**: Create Visual Studio and VS Code extensions for real-time pattern testing and visualization.
- [ ] **Diff-Aware Queries**: Support incremental scans that only process files changed since a specific Git commit or timestamp.
- [ ] **Developer Tooling SDK**: Expose selected tokenizers and classifiers for use by other build-tool authors.
- [ ] **Ecosystem Integrations**: Explore integrations with build systems, development tooling, CI/CD workflows, and repository analysis tools.

---

## Future Performance Work

Performance optimization remains an ongoing engineering concern, but future work will be driven by measured evidence rather than speculative optimization.

Potential future investigations include:

- [ ] **Cross-Platform Performance Validation**: Compare large-scale traversal behavior on Windows, Linux, and macOS.
- [ ] **Long-Running Stability Validation**: Validate traversal behavior under repeated large-scale execution.
- [ ] **Runtime Evolution Tracking**: Re-evaluate BCL filesystem enumeration performance as newer .NET runtimes become available.
- [ ] **Native Enumeration Research**: Revisit native or platform-specific enumeration only if future measurements demonstrate a sufficiently large benefit to justify the complexity.

Future performance work is not part of the v1.5 release scope.
