# Project Roadmap: Jeninnet.FileQuery

This roadmap outlines the strategic direction for **Jeninnet.FileQuery**, focusing on performance, observability, and ecosystem integration.

## 🎯 Vision
To be the fastest, most predictable, and most flexible file discovery engine for the .NET ecosystem, adhering to a zero-allocation matching philosophy.

---

## Phase 1: The "Production-Ready" Foundation (v1.0)
*Goal: Ensure the foundation is immutable, leak-proof, and predictable.*

- [x] **Physical API Immutability**: Refactor `FileQueryOptions` and `PatternInput` to be truly immutable (constructor-based or frozen collections) to ensure thread-safety.
- [x] **Total IO Abstraction**: Complete `IFileSystem` coverage to ensure 100% decoupling from `System.IO`, enabling virtual/mocked filesystem support.
- [x] **Cross-Platform Baseline**: Establish and publish performance benchmarks for Windows, Linux (Ubuntu), and macOS.
- [x] **Matcher Order Optimization**: Ensure that "fail-fast" logic is applied to pattern evaluation to minimize execution time.

## Phase 2: The "Observability" Release (v1.1 - v1.x)
*Goal: Provide deep insights into the enumeration and matching process.*

- [x] **Progress Reporting**: Support `IProgress<T>` in `EnumerateFilesAsync` for real-time scan statistics.
- [x] **Match Diagnostics**: Introduce an optional audit mode to explain *why* a file was included or excluded (e.g., "Matched by Glob line 42 in .gitignore").
- [x] **Deep Cancellation**: Ensure `CancellationToken` propagation is verified across all hot paths.
- [x] **Enhanced Error Recovery**: Provide configurable strategies for handling IO errors (e.g., `Skip`, `Retry`, `Abort`).
- [x] **Pattern Validation Framework**: Implement early detection and reporting for malformed patterns.

## Phase 3: The "Scalability" Release (v2.0)
*Goal: Maximize hardware utilization for massive directory trees.*

- [ ] **Parallel Traversal**: Implement a multi-threaded, high-concurrency directory walker.
- [ ] **Cost-Based Pattern Reordering**: Dynamically reorder patterns based on historical execution cost (e.g., move expensive Regex checks after cheap Globs).
- [ ] **Matcher Caching 2.0**: Refine internal caching for frequently accessed pattern sets in high-frequency scenarios.
- [ ] **Advanced Sample Library**: Continue expanding the library of complex matching scenarios (Regex, POSIX, etc.).

## Phase 4: The "Ecosystem" Release (v2.x+)
*Goal: Broaden adoption through tooling and specialized integrations.*

- [ ] **Dotnet Global Tool**: Release `dotnet-filequery` CLI for terminal-based file discovery and piping.
- [ ] **IDE Extensions**: Create VS and VS Code plugins for real-time pattern testing and visualization.
- [ ] **Diff-Aware Queries**: Support "incremental" scans that only process files changed since a specific Git commit or timestamp.
- [ ] **Developer Tooling SDK**: Expose internal tokenizers and classifiers for use by other build-tool authors.
