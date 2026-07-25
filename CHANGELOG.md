# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-07-26

### Added
- **OpenTelemetry Integration**: Export metrics and spans for structured observability.
- **AOT Compilation Validation Pipeline**: Strict Native AOT compatibility checks in CI.
- **Roslyn Analyzers for Pattern Validation**: Design-time warnings and code fixes for malformed patterns.

### Changed
- **Improved Path Validation**: `FileQueryValidator.ValidateExecution` now eagerly validates paths, providing more descriptive `ArgumentException` messages for invalid paths, excessive length, or malformed UNC structures.

---

## [1.2.1] - 2026-06-22

### Fixed
- Fixed file encoding in test files.
- Minor documentation renames and organization improvements.

---

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
- Fixed benchmark exporter "md" to "markdown" and corrected launchSettings.json syntax.
- Addressed SonarCloud quality warnings and redundant code paths.

---

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
- Improved navigation structure and cross‑references between sections in the README files for better readability.
- Added status badges to all project README files indicating build status, test coverage, and latest release version.

### Changed

- Traversal planning now carries optional observability sinks and recovery policy without changing default query behavior.
- `IgnoreInaccessible` remains supported and maps to the default skip-or-abort recovery behavior for compatibility.

---

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
- GitIgnore sub-set results take precedence over Glob and Regex sub-sets in hybrid mode. A **GitIgnore** inclusion is final; **Glob** and **Regex** matchers can re-include paths excluded by **GitIgnore** ***but cannot exclude paths that GitIgnore has included***.
