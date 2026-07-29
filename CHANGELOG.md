# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
