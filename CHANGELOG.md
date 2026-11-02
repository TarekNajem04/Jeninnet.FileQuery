# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-11-02

### Breaking Changes

- **`IFileQueryEngine` contract expanded**: custom engine implementations must add seven new members:
  `Execute(query, CancellationToken)`, `Execute(query, IProgress<FileQueryProgress>, CancellationToken)`,
  `ExecuteEntries(query)`, `ExecuteEntries(query, CancellationToken)`,
  `ExecuteEntries(query, IProgress<FileQueryProgress>, CancellationToken)`,
  `ExecuteEntriesAsync(query, CancellationToken)`, and
  `ExecuteEntriesAsync(query, IProgress<FileQueryProgress>, CancellationToken)`.
  The built-in engine already implements them.
  See [Migration.md](Migration.md#upgrading-to-v200).
- **`Specific` pattern validation is enforced**: untyped patterns and typed dialects that do not
  match the selected `PatternMatchingMode` now throw `PatternException` instead of being compiled
  into buckets that can never match. `FileQueryBuilder.Where(string)` assigns the forced dialect
  when `UsingGitIgnore`/`UsingGlob`/`UsingRegex` is active.
- **Empty and whitespace-only patterns are rejected**: `PatternInput`, `FileQueryBuilder.Where`,
  and the pattern-set compiler now throw `ArgumentException` instead of silently treating the
  entry as a no-op.
- **Windows separators in Glob patterns are rejected**: a backslash used as a path separator now
  throws `PatternException` with an actionable message. Backslashes before glob metacharacters
  (including escaped braces) remain valid escapes.
- **GitIgnore directory-negation ordering corrected**: a later explicit rule now overrides an
  earlier directory-only negation (`["**", "!keep/", "keep/file.txt"]` excludes the file), matching
  Git's last-match-wins semantics.
- **Non-matching later dialects preserve GitIgnore exclusions**: Glob/Regex fallbacks no longer
  turn a GitIgnore `Exclude` into `NoMatch`, so explanations and directory pruning agree with the
  documented precedence model. Glob/Regex can still re-include below a GitIgnore-excluded directory
  when they could match a descendant.
- **`FileQueryOptionsConfig` constructor expanded**: the record now has 17 positional parameters
  (10 in v1.5.0), adding the metadata/filter, `.gitignore`, and directory-result options. Source
  using named arguments or optional defaults usually recompiles, but precompiled consumers bind to
  the old constructor/`Deconstruct` signatures and must be rebuilt. See
  [Migration.md](Migration.md#2-rebuild-filequeryoptionsconfig-consumers).
- **Regex timeout exception changed**: an expired `Regex` match now throws `PatternException` with the
  source pattern and an inner `RegexMatchTimeoutException`, instead of leaking the timeout exception.
  Update `catch(RegexMatchTimeoutException)` blocks.
- **Case-insensitive character classes now fold**: `IgnoreCase()` makes `[a-z]` match `A`–`Z`
  (and literals inside classes fold too). Queries that depended on the previous, narrower behavior
  can return additional matches.
- **Malformed-pattern diagnostics changed**: untyped malformed patterns now report the concrete
  validator reason plus `Position`/`Segment` instead of the generic “No compiler registered” message.
  Code that matches exception text should use the typed position data instead.

### Added

- **Global Tool**: Added `Jeninnet.FileQuery.Tool`, the `dotnet-filequery` .NET global tool. It exposes the engine in the terminal with Hybrid, GitIgnore, Glob, and Regex patterns plus `--root`, `--depth`, `--no-recursive`, `--ignore-case`, `--symlinks`, `--relative`, `--null`, and `--count`, and streams deterministic output for pipelines. Includes 55 tests, a package README, and a documentation guide.
- **Diff-Aware Queries**: Added `FileQueryBuilder.RestrictToPaths` and `FileQueryOptionsConfig.IncludedPaths` to restrict a query to a root-relative allow-list, skipping unmatched files before pattern matching and pruning directories that cannot contain a listed file. The `dotnet-filequery` tool exposes it through `--changed-since <revision>` and `--changed-since-date <timestamp>`, reports added, modified, renamed, and copied files, excludes deletions, and includes untracked files unless `--no-untracked` is supplied.
- **VS Code Extension**: Added `extensions/vscode`, a dependency-free extension with *Test File Pattern* and *Explain File Pattern* commands. It drives the global tool against the selected workspace folder and reports matches or per-file include/exclude reasons in the **Jeninnet FileQuery** output channel.
- **Explain Mode**: Added `dotnet-filequery --explain`, which writes `<include|exclude>\t<path>\t<reason>` for every evaluated file. It reports the responsible pattern kind and cannot be combined with `--count` or `--null`.
- **Build Tooling SDK**: Added `Jeninnet.FileQuery.Tooling` with `PatternInspector.Classify`, `Validate`, and `Inspect` (automatic or forced dialect). The SDK returns SDK-defined inspection models (segments, tokens, structural flags) so build systems, linters, and IDE plugins can reuse the engine's pattern semantics without executing a query.
- **GitHub Action**: Added the composite action `.github/actions/filequery`. It installs or reuses the global tool, runs a query (optionally diff-aware), exposes `files` and `count` outputs, writes a job summary, and is verified end to end by the `filequery-action` CI job.
- **Cross-Platform Baseline**: Added `CrossPlatformTraversalBenchmark`, which measures traversal and matching over a deterministic 10,000-file tree so results are comparable across operating systems. Recorded the Windows reference run (`MediumRun`), the official Windows/Ubuntu/macOS artifacts from the scheduled pipeline, the analysis, and a machine-readable baseline with a 25% regression policy.
- **Long-Running Stability Harness**: Added `src/Jeninnet.FileQuery.Soak`, which repeats a query over a deterministic dataset, samples the managed heap after full collections, and fails on heap growth (>4 MB) or throughput degradation (>1.5× first-half mean). A 60-second Windows run completed 6,613 iterations with 0.06 MB heap growth and no leak; the `soak` CI job runs a shorter version and uploads the report.
- **Runtime Evolution Tracking**: Added `scripts/performance/check-runtime.ps1` and `docs/performance/runtime-evolution.json` to detect .NET runtime/SDK drift against the recorded baseline, define the weekly/release re-baseline cadence, and record the current runtime (10.0.14). The benchmarks workflow now prints the drift check and uploads `runtime-info.txt` with every artifact.
- **Enumeration Ceiling Benchmark**: Added `EnumerationCeilingBenchmark`, which measures raw BCL enumeration against engine traversal and matching on the deterministic 10,000-file tree. On Windows 11 / .NET 10.0.14, BCL enumeration took 7.29 ms, engine traversal 9.70 ms (+33%), and matching added ~0.63 ms — bounding what a native enumerator could recover.
- **OpenTelemetry Instrumentation**: Added stable `ActivitySource` (`Jeninnet.FileQuery`, spans `FileQuery.Execute`/`FileQuery.ExecuteAsync`) and `Meter` instrumentation (`filequery.queries.*`, `filequery.entries.scanned`, `filequery.files.matched`). No external OpenTelemetry package is required; consumers attach exporters.
- **Entry Metadata and Filters**: Added `FileQueryEntry` and `ExecuteEntries()`/`ExecuteEntriesAsync()` with full/relative paths, size, creation/last-write timestamps, attributes, and directory state. Added `MinSize`, `MaxSize`, `ModifiedAfter`, `ModifiedBefore`, and `AttributesToSkip` filters that run before matching without extra file-system calls.
- **Single-Path Matcher**: Added `FileQueryPatternSet` with `IsMatch(string)`, allocation-free `IsMatch(ReadOnlySpan<char>)`, and `Explain(path)` for linters and IDE plugins.
- **Directory Results**: Added `IncludeDirectories()` to yield matching directories from the existing traversal, with directory-only patterns, negation, filters, and pruning semantics preserved. Files remain the default.
- **CLI JSON Output**: Added `dotnet-filequery --json`, emitting newline-delimited objects with absolute/relative paths, size, timestamps, attributes, and directory state. It composes with `--explain` and diff-aware queries and conflicts with `--null`/`--count`.
- **Sync Cancellation and Progress**: Added synchronous `Execute`/`ExecuteEntries` overloads that accept `CancellationToken` and `IProgress<FileQueryProgress>`, with the same snapshot shape and counters as the async path.
- **Multiple Roots**: Added `FileQueryBatch` to execute several roots as one ordered stream, with `FileQueryEntry.RootPath` for unambiguous relative paths and per-query error-recovery settings.
- **Pattern Error Positions**: `PatternException` now exposes `Position` and `Segment`, and the Tooling SDK's `PatternValidation` reports the same data for structural failures.
- **Brace Expansion**: Glob and GitIgnore patterns support nested `{a,b}` alternation with escaped/class-literal braces, regex exclusion, bounded expansion, and position-bearing validation errors.
- **MSBuild Validation and SARIF**: Added `Jeninnet.FileQuery.Build` to validate `FileQueryPatterns` properties/items at build time and export SARIF 2.1.0 via the Tooling SDK's `SarifExporter`.
- **File Provider Adapter**: Added `Jeninnet.FileQuery.FileProviders` with `FileProviderQuery.From(IFileProvider, root)` so physical, embedded, and in-memory providers use the same matching, metadata, directory-result, and GitIgnore pipeline without materializing files.
- **Git Ignore File Ingestion**: Added `UseGitIgnoreFiles()` / `GitIgnoreOptions` to load root and nested `.gitignore` files, `.git/info/exclude`, and an optional global excludes file with parent-before-child precedence, directory scoping, negation, escaped characters, and directory-only rules. The global tool opts in with `--ignore-files` and `--global-ignore`.
- **Roslyn Analyzer**: Added the `Jeninnet.FileQuery.Analyzers` package with `JNQ1001` (malformed pattern literal) and a safe code fix that closes a character class.
- **Inaccessible-Entry Policy**: Added `FileQueryBuilder.IgnoreInaccessible(bool)` so callers can surface recoverable access failures instead of always skipping them. Disabling the default selects the abort recovery policy unless `WithErrorRecovery` is supplied.
- **Traversal Controls**: Added `FileQueryBuilder.WithMaxDepth(int)` and `WithTraversal(TraversalOptions)`, exposing depth limits, DFS/BFS strategy, and symlink policy through the fluent API.
- **Project Icon**: Added `assets/icon.svg` and a 1024×1024 PNG, wired the icon into all eight NuGet packages via shared build targets, and used it as the DocFX logo/favicon and README mark.
- **Native AOT Validation**: Added the `Jeninnet.FileQuery.AotSmoke` host and a `linux-x64` CI job that publishes with `PublishAot`, treats trimming/AOT warnings as errors, and runs a real query.

### Changed

- **Documentation Accuracy**: Corrected unbacked claims (OpenTelemetry integration, Roslyn analyzers, AOT validation pipeline), qualified the throughput and sync-cancellation statements, and scheduled the missing capabilities in ROADMAP Phase 5.5 (pre-v2.0.0 hardening). The affected Phase 2 items were reopened and the task-board statuses reclassified.
- **Platform Support Policy**: v2.0.0 remains .NET 10-only. .NET 8 (end of support 2026-11-10) and .NET 9 (end of support 2026-05-12) are not targeted. The release after v2.0.0 adds .NET 11 through `net10.0;net11.0` multi-targeting (ROADMAP Phase 6).
- **Package Version 2.0.0**: all product packages now use `2.0.0`; `FileProviders` must be installed with the matching core version because the adapter uses friend access to the engine's internal `IFileSystem`.
- **Standalone Assertions Package**: Replaced the in-repo `Jeninnet.Testing.Assertions` project and its 182-test suite with the published `Jeninnet.Assertions` 1.0.0 package. Test projects now consume the external package; product packages and runtime behavior are unchanged.
- **README Coverage and Package Front Pages**: Added READMEs for every src/test/sample project and the GitHub Action, rewrote the core package README with the current API, expanded the Analyzer/Build/FileProviders pages, and added sample/test indices. No runtime change.
- **Solution Synchronization**: Added the new documentation, assets, GitHub Action files, and images to `Jeninnet.FileQuery.slnx` (229 unique entries). Project READMEs stay with their projects (included via `Directory.Build.props`); only the root README remains in **Solution Items**. No runtime change.
- **VS Code Guide**: Expanded the VS Code chapter with installation, eight worked pattern examples, settings, output/limits, troubleshooting, architecture, and development/packaging instructions; linked it from the README, docs home, and extension README.
- **Coverage Reporting**: Added `coverlet.collector` to the Analyzers, Build, and FileProviders test projects so every test project contributes coverage. The full report now covers 9 assemblies: **89.5% line**, **79.8% branch**, **92.0% method**.
- **Generated Badges**: `scripts/badges/generate.ps1` now reads `artifacts/coverage/Summary.txt`, renders `badges/coverage.svg`, and the root README displays all four generated badges. Added coverage/badge usage documentation.
- **Test Suite Dogfooding**: Replaced every MSTest `Assert` call in the seven test projects (1,771 calls) with the `Jeninnet.Assertions` fluent API (`Should()` and `FluentActions`). MSTest now supplies only attributes, `TestContext`, and runner integration; the test suite itself demonstrates the assertion package end to end.
- **Analyzer Cleanup**: Resolved every IDE, MSTest, and Roslynator diagnostic in the solution, including collection expressions, primary constructors, exception documentation, naming conventions, `TestContext.CancellationToken` usage, and conditional-access/chaining rules. A refreshed export cleared the remaining 9 `IDE0305` collection hints and the `RCS1201` chaining warning. No runtime behavior change.
- **Local Sonar Baseline**: Ran SonarQube Community Build locally with OpenCover coverage and exported issues: zero open issues, Quality Gate OK, 86.0% Sonar coverage (88.0% lines, 82.0% branches), and 0.0% duplicated lines. Excluded the Soak and AotSmoke executable harnesses from the coverage metric while keeping them in issue analysis. Documented the Java 21, Git object-store, and re-query workflow in the Sonar tooling README.
- **Duplication Elimination**: Extracted the shared brace scanner, traversal entry-processing step, and plan telemetry helpers. Sonar now reports zero duplicated lines and blocks, with no new issues.
- **Release Packaging**: The tag publish workflow now includes `Jeninnet.FileQuery.Analyzers`, packs all eight packages, and ships the LICENSE file plus symbol packages for the six library/tool packages. CI validates the analyzer package and keeps the Sonar coverage exclusions aligned with the local script.
- **Release Documentation**: Refreshed the current-release status, release checklist, Sonar baseline, package-version examples, architecture notice, and stale internal version references.
- **Capability Review**: Added `docs/articles/library-capability-review.md`, comparing the library with `Microsoft.Extensions.FileSystemGlobbing`, `DotNet.Glob`, gitignore parsers, BCL enumeration, and `fd`/`ripgrep`, with a prioritized backlog and an audit of the published claims.
- **Capability Review Refresh**: Updated the review to the v2.0.0 state: metadata/filters, `.gitignore` ingestion, single-path matching, directory results, `IFileProvider`, brace expansion, sync cancellation, JSON, analyzers, AOT, and MSBuild/SARIF are marked shipped; the claims audit is closed; Phase 2 statuses now reflect delivery by H-02/H-03/H-04; and both release checklists require the docs/claims/API checks.
- **Documentation Refresh**: Rebuilt the documentation landing page and onboarding flow, added common recipes and error-handling guides, fixed stale API examples in quick-start/runtime/observability pages, added Runtime and Contributing navigation, and linked the published documentation from the root README.
- **Native Enumeration Decision**: Rejected platform-specific enumeration for v2.0.0. .NET already wraps `FindFirstFileExW`/`getdents64`/`readdir`; the addressable ceiling is a fraction of the 7.29 ms BCL baseline, while three OS-specific implementations would add unsafe code, semantic drift risk, and a permanent verification matrix. The measurements and revisit criteria are recorded in `docs/performance/native-enumeration-research.md`.

### Fixed

- **Diff-Aware Roots Through Symlinks**: Git change detection now rebases via `git rev-parse --show-prefix` instead of comparing path strings, so `--changed-since` works when the query root reaches the repository through a symlink (for example macOS `/var` → `/private/var`) or points at a subdirectory.
- **CommandLine Regex Patterns**: `PatternBuilder.Build` now adds the required `r:` dialect prefix to patterns supplied through the `--regex` option, so regex patterns work without a manual prefix.
- **Escaped GitIgnore Leading Characters**: An escaped leading `!` (`\!name`) is now treated as a literal instead of negation, matching Git's escape semantics.
- **CLI Ignore-File Fallback**: `dotnet-filequery --ignore-files` no longer lets the implicit `!**` include-all fallback override the loaded ignore rules.
- **Character-Class Case Folding**: `IgnoreCase()` now folds literals and ranges inside character classes, so `[a-z]` matches `A`–`Z` as expected. POSIX categories keep their category semantics.
- **Regex Match Timeouts**: A `RegexMatchTimeoutException` during matching is now surfaced as a `PatternException` with the offending pattern instead of leaking an infrastructure exception.
- **Malformed Pattern Diagnostics**: Untyped patterns that fail structural validation now report the specific reason and position instead of the generic "No compiler registered for pattern type 'Unknown'" message.
- **Empty Pattern Analysis**: `PatternAnalysisResult.Empty()` now reports `IsEmpty: true`, matching its contract.
- **Directory-Only Negation Ordering**: A later explicit rule now overrides an earlier directory-only negation, matching Git's last-match-wins model.
- **Match Explanation Attribution**: `Explain` now attributes GitIgnore negations, Glob/Regex re-inclusions, and GitIgnore exclusions to the rule that actually decided the outcome.
- **Pattern Order Preservation**: Builder and merger storage now preserves declaration order deterministically instead of relying on `HashSet` enumeration order.
- **Retry Robustness**: Retry policies no longer overflow `int.MaxValue` attempts, and a retried directory enumeration skips already-yielded entries instead of duplicating them.
- **Regex Cache Bound**: The shared regex cache now has an upper bound, preventing unbounded retention in long-running hosts that supply dynamic patterns.
- **Git Ignore Repository Rules**: `.git/info/exclude` rules that target an ancestor of a deeper query root now exclude the query root instead of being silently dropped.
- **Cross-Dialect Re-inclusion**: Glob/Regex fallbacks can now re-include files below a GitIgnore-excluded directory when they could match a descendant, while non-matching fallbacks preserve the exclusion.
- **CLI Explicit Include-All**: An explicitly supplied `!**` pattern is no longer mistaken for the implicit fallback when `--ignore-files` is active.
- **Forced-Dialect CLI Validation**: `--ignore-files` with a forced non-GitIgnore `--dialect` now fails during argument validation instead of later during query construction.
- **Regex Literal Braces**: Untyped `r:` patterns with literal or quantifier braces are no longer misclassified as malformed by the glob structural validator.
- **Empty-Set Root Outcome**: An empty pattern set now reports `Exclude` (not `NoMatch`) for the empty path, consistent with the resolver and documentation.
- **File Provider Retry Parity**: The `IFileProvider` adapter now uses the same retry clamping and no-duplicate replay behavior as the physical file system.
- **Native AOT Coverage**: The AOT smoke host now exercises Regex and GitIgnore matching in addition to Glob, covering the trimmed execution paths.
- **Traversal Frontier Memory**: The BFS/DFS frontier now compacts consumed slots before growing, avoiding unnecessary array doubling.

### Removed

- **In-repo assertions project**: removed `src/Jeninnet.Testing.Assertions` and its 182-test suite; test projects now use the standalone `Jeninnet.Assertions` 1.0.0 package. Product packages have no dependency on it.

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

> **Historical correction:** The three capabilities below were announced in the 1.3.0 entry but
> were not backed by shipped code at the time. They were delivered in v2.0.0 by H-02 (telemetry),
> H-03 (Roslyn analyzers), and H-04 (Native AOT validation). See the v2.0.0 entry.

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
