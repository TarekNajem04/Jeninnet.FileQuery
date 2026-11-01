# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Global Tool**: Added `Jeninnet.FileQuery.Tool`, the `dotnet-filequery` .NET global tool. It exposes the engine in the terminal with Hybrid, GitIgnore, Glob, and Regex patterns plus `--root`, `--depth`, `--no-recursive`, `--ignore-case`, `--symlinks`, `--relative`, `--null`, and `--count`, and streams deterministic output for pipelines. Includes 28 tests, a package README, and a documentation guide.
- **Diff-Aware Queries**: Added `FileQueryBuilder.RestrictToPaths` and `FileQueryOptionsConfig.IncludedPaths` to restrict a query to a root-relative allow-list, skipping unmatched files before pattern matching and pruning directories that cannot contain a listed file. The `dotnet-filequery` tool exposes it through `--changed-since <revision>` and `--changed-since-date <timestamp>`, reports added, modified, renamed, and copied files, excludes deletions, and includes untracked files unless `--no-untracked` is supplied.
- **VS Code Extension**: Added `extensions/vscode`, a dependency-free extension with *Test File Pattern* and *Explain File Pattern* commands. It drives the global tool against the selected workspace folder and reports matches or per-file include/exclude reasons in the **Jeninnet FileQuery** output channel.
- **Explain Mode**: Added `dotnet-filequery --explain`, which writes `<include|exclude>\t<path>\t<reason>` for every evaluated file. It reports the responsible pattern kind and cannot be combined with `--count` or `--null`.
- **Build Tooling SDK**: Added `Jeninnet.FileQuery.Tooling` with `PatternInspector.Classify`, `Validate`, and `Inspect` (automatic or forced dialect). The SDK returns SDK-defined inspection models (segments, tokens, structural flags) so build systems, linters, and IDE plugins can reuse the engine's pattern semantics without executing a query.
- **GitHub Action**: Added the composite action `.github/actions/filequery`. It installs or reuses the global tool, runs a query (optionally diff-aware), exposes `files` and `count` outputs, writes a job summary, and is verified end to end by the `filequery-action` CI job.
- **Cross-Platform Baseline**: Added `CrossPlatformTraversalBenchmark`, which measures traversal and matching over a deterministic 10,000-file tree so results are comparable across operating systems. Recorded the Windows reference run (`MediumRun`), the official Windows/Ubuntu/macOS artifacts from the scheduled pipeline, the analysis, and a machine-readable baseline with a 25% regression policy.
- **Long-Running Stability Harness**: Added `src/Jeninnet.FileQuery.Soak`, which repeats a query over a deterministic dataset, samples the managed heap after full collections, and fails on heap growth (>4 MB) or throughput degradation (>1.5× first-half mean). A 60-second Windows run completed 6,613 iterations with 0.06 MB heap growth and no leak; the `soak` CI job runs a shorter version and uploads the report.
- **Runtime Evolution Tracking**: Added `scripts/performance/check-runtime.ps1` and `docs/performance/runtime-evolution.json` to detect .NET runtime/SDK drift against the recorded baseline, define the weekly/release re-baseline cadence, and record the current runtime (10.0.14). The benchmarks workflow now prints the drift check and uploads `runtime-info.txt` with every artifact.
- **Enumeration Ceiling Benchmark**: Added `EnumerationCeilingBenchmark`, which measures raw BCL enumeration against engine traversal and matching on the deterministic 10,000-file tree. On Windows 11 / .NET 10.0.14, BCL enumeration took 7.29 ms, engine traversal 9.70 ms (+33%), and matching added ~0.63 ms — bounding what a native enumerator could recover.

### Changed

- **Platform Support Policy**: v2.0.0 remains .NET 10-only. .NET 8 (end of support 2026-11-10) and .NET 9 (end of support 2026-05-12) are not targeted. The release after v2.0.0 adds .NET 11 through `net10.0;net11.0` multi-targeting (ROADMAP Phase 6).
- **Capability Review**: Added `docs/articles/library-capability-review.md`, comparing the library with `Microsoft.Extensions.FileSystemGlobbing`, `DotNet.Glob`, gitignore parsers, BCL enumeration, and `fd`/`ripgrep`, with a prioritized backlog and an audit of the published claims.
- **Native Enumeration Decision**: Rejected platform-specific enumeration for v2.0.0. .NET already wraps `FindFirstFileExW`/`getdents64`/`readdir`; the addressable ceiling is a fraction of the 7.29 ms BCL baseline, while three OS-specific implementations would add unsafe code, semantic drift risk, and a permanent verification matrix. The measurements and revisit criteria are recorded in `docs/performance/native-enumeration-research.md`.

### Fixed

- **CommandLine Regex Patterns**: `PatternBuilder.Build` now adds the required `r:` dialect prefix to patterns supplied through the `--regex` option, so regex patterns work without a manual prefix.

## [1.5.0] - 2026-08-12

### Added

- **Large-Scale Performance Evaluation**: Added a reproducible evaluation workflow using datasets containing up to 1,000,000 files and 4,096 directories for realistic traversal performance validation.
- **Performance Investigation Documentation**: Added measured performance investigations covering filesystem enumeration, traversal allocations, GitIgnore matching, and remaining BCL/OS-bound costs.
- **Relative Path Buffering**: Added a pooled `ArrayPool<char>`-backed relative-path buffer to eliminate per-entry relative-path string allocations during traversal.
- **GitIgnore Literal Suffix Fast Path**: Added compile-time literal-suffix resolution for eligible wildcard GitIgnore patterns, allowing inexpensive zero-allocation rejection before recursive pattern matching.
- **Performance Regression Coverage**: Added focused tests covering relative-path buffer behavior, growth, reuse, disposal, traversal integration, ordering, and synchronous/asynchronous equivalence.

### Changed

- **Filesystem Enumeration**: `FileSystem` now consumes filesystem attributes directly from .NET enumeration results instead of performing a separate attribute lookup for each entry.
- **Traversal Allocation Strategy**: Relative paths are now composed using a reusable pooled character buffer rather than allocating a new string for every traversed entry.
- **GitIgnore Matching**: Eligible wildcard patterns can now reject paths using their resolved literal suffix before invoking the recursive matcher.
- **Performance Engineering**: Traversal performance was systematically measured and optimized against a reproducible 1,000,000-file dataset while preserving matching semantics and public API behavior.
- **Performance Documentation**: Documented measured allocation profiles, benchmark methodology, optimization decisions, and the remaining filesystem-enumeration boundary.
- **Performance Stop-Gate**: Further optimization of the remaining BCL/OS-bound filesystem enumeration cost was deliberately stopped where additional native or platform-specific complexity was not justified by the measured benefit.

### Fixed

- **Redundant Attribute Enumeration**: Removed the redundant per-entry filesystem attribute lookup from the traversal hot path.
- **Relative Path Allocation Overhead**: Removed unnecessary per-entry relative-path string allocations from the traversal processing path.
- **GitIgnore Wildcard Matching Overhead**: Avoided recursive matching for paths that cannot satisfy an eligible literal suffix constraint.

### Verification

- Full solution test suite maintained with all tests passing throughout the performance optimization cycle.
- Performance results were validated repeatedly against the same 1,000,000-file dataset.
- Match counts remained stable across the optimization phases for equivalent queries.
- Public APIs and matching semantics were preserved.
- Build completed with zero warnings and zero errors.
- Repository formatting verification completed successfully.

### Notes

- The remaining dominant traversal cost is associated with .NET/BCL filesystem enumeration and underlying operating-system filesystem operations.
- Further native filesystem enumeration was not introduced because the measured benefit did not justify the additional platform-specific complexity and maintenance risk.
- `v1.5.0` therefore represents the completion of the current performance optimization cycle and establishes a measured performance baseline for future cross-platform evaluation.

## [1.4.0] - 2026-07-29

### Added

- **Jeninnet.Testing.Assertions Library** (`v1.0.0`): Extracted shared test infrastructure into a reusable, well-documented assertion library with fluent `.Should()` syntax. Supports Action, AsyncAction, Bool, Collection, Exception, Object, and String assertions.
- **Traversal Optimization**: `TraversalExecutor` now skips unnecessary directory traversal for negated directory-only patterns with a literal suffix (e.g., `!*.github/`), reducing IO for excluded directory subtrees.
- **New Test Suites**: Added FileEnumeration, Unit, and Integration test directories with improved organization.

### Changed

- **Test Assertion Migration**: All 90+ test files migrated from `TestAssertEx.*` to fluent `.Should()` assertion syntax.
- **Test Infrastructure**: Removed `Shared/` and `Infrastructure/` folders in favor of assertion library types.
- **Enums**: Added explicit underlying values to all enum members for binary compatibility.
- **Editorconfig**: Enabled Roslynator analyzer category, elevated RCS1141 to warning, refined code style preferences.
- **Collection Expressions**: Modernized `.ToArray()` calls to collection expressions (`[.. x]`) across benchmarks and test files.
- **XML Documentation**: Added missing `<param>`, `<exception>`, and `<returns>` XML doc elements; converted inline comments to proper `<summary>` tags.

### Fixed

- **PatternException**: Removed redundant `: base()` constructor call.
- **CS0104 Ambiguity**: Resolved `PathUtilities` naming conflict by renaming assertion library utility to `PathHelper`.
- **CS1591 Warnings**: Added XML documentation to all public types and members in the assertion library.
- **RCS1141 Warnings**: Added missing `<param>` elements to `Should()` methods and primary constructors.
- **All Analyzer Warnings**: Solution now builds with 0 warnings and 0 errors.

## [1.3.0] - 2026-07-26

### Added

- **OpenTelemetry Integration**: Export metrics and spans for structured observability.
- **AOT Compilation Validation Pipeline**: Strict Native AOT compatibility checks in CI.
- **Roslyn Analyzers for Pattern Validation**: Design-time warnings and code fixes for malformed patterns.

### Changed

- **Improved Path Validation**: `FileQueryValidator.ValidateExecution` now eagerly validates paths, providing more descriptive `ArgumentException` messages for invalid paths, excessive length, or malformed UNC structures.

## [1.2.1] - 2026-06-22

### Fixed

- Fixed file encoding in test files.
- Minor documentation renames and organization improvements.

## [1.2.0] - 2026-06-22

### Added

- **Validation Pipeline**: Implemented centralized `FileQueryValidator` for pre-execution configuration and root path validation.
- **Pattern Results**: Introduced `PatternResult<T>` to improve compilation flow and replace exceptions.
- **New Sample Projects**: Added 5 complex pattern scenarios (NestedGlobMatching, NegationAndRecursive, POSIXClassMatching, RegexAnchoredMatching, DirectoryOnlyMatching).
- **Benchmarks**: Expanded suite for pattern classification and moved benchmarks to `src/Jeninnet.FileQuery.Benchmarks`.

### Changed

- **Refactoring & Quality**: Resolved multiple SonarCloud quality warnings (S107, S3776, S2589, S1066) across `FileQueryOptions`, `CompiledPattern`, `TraversalExecutor`, and `FileSystem`.
- **Compiler Registry**: Refactored `IPatternCompilerRegistry` to return `PatternResult` and updated `PatternPipeline` to handle results gracefully.
- **Infrastructure**: Fixed benchmark exporter configuration, updated `launchSettings.json`, and moved benchmarks to `src/`.
- **Documentation & Cleanup**: Improved exception messages, updated project configurations, and removed deprecated `SECURITY.md`.
- **Breaking Change**: `FileQueryOptions` constructor updated. [See Migration Guide](Migration.md).

### Fixed

- Fixed benchmark exporter "md" to "markdown" and corrected `launchSettings.json` syntax.
- Addressed SonarCloud quality warnings and redundant code paths.

## [1.1.0](https://github.com/TarekNajem04/Jeninnet.FileQuery/releases/tag/v1.1.0) - 2026-06-10

### Added

- Async traversal progress reporting through `IProgress<FileQueryProgress>` overloads on `IFileQueryEngine.ExecuteAsync` and the fluent builder.
- Optional match audit diagnostics through `FileQueryOptions.AuditMatches`, `FileQueryOptions.Diagnostics`, and `FileQueryBuilder.WithDiagnostics`.
- Diagnostic source metadata on compiled patterns, including source text and source order index for responsible-pattern reporting.
- Configurable IO recovery with `FileQueryErrorRecoveryOptions` and `FileQueryErrorAction` strategies: `Skip`, `Retry`, and `Abort`.
- Regression coverage for progress reporting, match diagnostics, cancellation propagation, and recovery strategies.
- Revised all README.md files across the repository to include new observability features and updated usage examples.
- Updated feature descriptions and improved documentation clarity for all existing features in the README files.
- Standardized formatting and ensured consistency across all documentation files.
- Improved navigation structure and cross-references between sections in the README files for better readability.
- Added status badges to all project README files indicating build status, test coverage, and latest release version.

### Changed

- Traversal planning now carries optional observability sinks and recovery policy without changing default query behavior.
- `IgnoreInaccessible` remains supported and maps to the default skip-or-abort recovery behavior for compatibility.

## [1.0.0](https://github.com/TarekNajem04/Jeninnet.FileQuery/releases/tag/v1.0.0) - 2026-06-02

### Added

- Six sample projects covering basic matching, pattern language, recursive traversal, hybrid matching, regex matching, and advanced DI + CLI usage.
- BenchmarkDotNet benchmark suite covering all matchers, pattern tokenization, classification, and traversal.
- Architecture tests enforcing layer boundaries, zero-allocation contract, and matcher construction authority.
- Full XML documentation on all public members.
- Symbol packages (`.snupkg`) for debugger step-through support.

### Notes

- Targets `net10.0`. Requires .NET 10 or later.
- All three packages versioned together at `1.0.0`.
- The `PatternMatchingMode` property on `FileQueryOptions` applies only when `PatternInterpretationMode.Specific` is set. In the default `Hybrid` mode it is ignored.
- GitIgnore sub-set results take precedence over Glob and Regex sub-sets in hybrid mode. A **GitIgnore** inclusion is final; **Glob** and **Regex** matchers can re-include paths excluded by **GitIgnore** but cannot exclude paths that GitIgnore has included.
