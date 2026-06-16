# Architecture Notice

The matching and traversal architecture was enhanced during the v1.2 release. All documentation in this repository reflects the current 1.2.0 architecture.

## Key Changes in 1.2.0

### Validation Pipeline
Introduced a centralized `FileQueryValidator` for pre-execution configuration and root path validation, ensuring consistent error handling and early detection of misconfigurations.

### Pattern Results
Replaced `PatternException` with `PatternResult<T>` in the compilation flow, providing a safer and more predictable approach to handling compilation errors without relying on exceptions for flow control.

### Refactored Compiler Pipeline
Refined the pattern compilation pipeline and anchor resolution, resolving several SonarCloud quality warnings and improving overall performance and maintainability.

## Key Changes From the Original Design (v1.0)

### PatternScanner responsibility narrowed
`PatternScanner` is now a pure lexer. The `ApplyImplicitRecursiveWildcard` method, which was a semantic transform embedded in the scanner, was removed and replaced by `GitIgnoreImplicitRecursiveInvariant` in the Semantic invariant phase.

### Invariant system unified
`GitIgnoreNegationImplicitRecursiveInvariant` (which handled only negated patterns) was merged into `GitIgnoreImplicitRecursiveInvariant`, which handles all unanchored GitIgnore patterns in one place.

### PatternInput replaces CanonicalPatternInput on the public surface
`FileQueryOptions.PatternInput` is now typed as `PatternInput` — a simple, BCL-typed public record. `CanonicalPatternInput` remains internal.

### Character class system redesigned
`CharacterClassToken` now wraps a `CharacterClass` AST using a discriminated union of `ICharacterClassElement` (CharLiteral, CharRange, PosixClass, CharacterClassParseError). The old parallel-list representation (separate `Characters` and `Ranges` lists) and the embedded `CharacterClassError?` field are removed.

### Zero-allocation hot path
All matcher hot-path loops converted from `foreach` over `ICompiledPatternSet` (which boxed the enumerator) to index-based `for` loops. All matchers now show 0 B allocated in BenchmarkDotNet.

### MatchingConfiguration type changed
`TypedPatterns` changed from `ImmutableDictionary` to `IReadOnlyDictionary`, eliminating an unnecessary dictionary copy in `TraversalPlanBuilder.Build`.

## Current Documentation Status

All documentation files are up to date with the 1.2.0 implementation. The architecture documents under `/docs/architecture/` reflect the current codebase.