# Architecture Notice

The matching and traversal architecture was redesigned during the v1.0 stabilization phase. All documentation in this repository reflects the **final v1.0 architecture**. No obsolete documents remain.

## Key Changes From the Original Design

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

All documentation files are up to date with the v1.0 implementation. The architecture documents under `/docs/architecture/` reflect the current codebase.