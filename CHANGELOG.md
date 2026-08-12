# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
