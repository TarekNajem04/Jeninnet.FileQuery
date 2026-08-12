# Project Structure

This document describes the layout of the repository and explains the purpose of each directory and project.

---

## Repository Layout

```
Jeninnet.FileQuery/
│
├── src/
│   ├── Jeninnet.FileQuery/                         Core engine
│   ├── Jeninnet.FileQuery.CommandLine/             CLI integration
│   ├── Jeninnet.FileQuery.DependencyInjection/     DI integration
│   ├── Jeninnet.FileQuery.Benchmarks/              BenchmarkDotNet benchmarks
│   └── Jeninnet.Testing.Assertions/                Test assertion helpers
│
├── tests/
│   ├── Jeninnet.FileQuery.Tests/                   Unit and integration tests
│   ├── Jeninnet.AdvancedUsage.Tests/               AdvancedUsage sample tests
│   └── Jeninnet.Testing.Assertions.Tests/          Assertions library tests
│
├── samples/
│   ├── BasicMatching/                              Hello-world sample
│   ├── PatternLanguage/                            GitIgnore pattern demo
│   ├── RecursiveTraversal/                         Deep traversal demo
│   ├── HybridMatcher/                              Mixed pattern dialect demo
│   ├── RegexMatching/                              Regex pattern demo
│   └── AdvancedUsage/                              DI + CLI combined demo
│
├── docs/                                           Documentation source
├── artifacts/                                      Build outputs (gitignored)
├── Directory.Build.props                           Shared MSBuild properties
├── Directory.Build.targets                         Shared MSBuild targets
├── Directory.Packages.props                        Central package version management
├── Jeninnet.FileQuery.slnx                         Solution file
└── global.json                                     .NET SDK version pin
```

---

## Core Engine — `src/Jeninnet.FileQuery`

The core library. No external dependencies.

```
Jeninnet.FileQuery/
├── Composition/
│   └── DefaultEngineBuilder.cs          Wires the engine without DI
├── Engine/
│   └── FileQueryEngine.cs               IFileQueryEngine implementation
├── Enums/                               Public enumerations
├── Extensions/                          Extension methods (CaseSensitivity, MatchResult)
├── Internal/
│   ├── CaseSensitivityResolver.cs       Resolves PlatformDefault at runtime
│   ├── FileSystemEntryInfo.cs           Classifies filesystem entries
│   └── PatternsMerger.cs                Merges typed and untyped patterns
├── IO/
│   ├── FileSystem.cs                    IFileSystem implementation
│   ├── FileSystemEntry.cs               Filesystem entry value type
│   ├── FileSystemGuards.cs              Access exception handling
│   ├── PathSegmentEnumerator.cs         ref struct segment enumerator
│   ├── PathUtilities.cs                 Path normalization
│   └── PathView.cs                      ref struct path wrapper
├── Matching/
│   ├── Compiled/
│   │   ├── GitIgnoreInstructionMatcher.cs
│   │   ├── GlobInstructionMatcher.cs
│   │   ├── HybridPathMatcher.cs
│   │   ├── NullMatcher.cs              Null-object pattern for no-pattern queries
│   │   ├── PathMatcher.cs              Abstract base
│   │   ├── RegexInstructionMatcher.cs
│   │   ├── SegmentInstructionMatcher.cs  Segment-level token matching
│   │   └── SegmentMatchEngine.cs       Shared recursive match logic
│   ├── ICompiledPattern.cs
│   ├── ICompiledPatternSet.cs
│   ├── IPathMatcher.cs
│   ├── IPatternToken.cs
│   ├── MatchingConfiguration.cs
│   ├── MatchPrecedenceResolver.cs      Coordinates GitIgnore/Glob/Regex sub-sets
│   ├── MatchResult.cs
│   └── PathMatchContext.cs
├── Patterns/
│   ├── Analysis/                        PatternAnalyzer — single-pass feature detection
│   ├── Canonical/                       CanonicalPatternInput, CanonicalPatternSet
│   ├── Classification/                  PatternClassifier, ClassifiedPattern
│   ├── Compilation/                     PatternPipeline, compilers per dialect
│   ├── Compiled/                        CompiledPattern, CompiledPatternSet
│   ├── Exceptions/                      PatternException, PatternSyntaxException
│   ├── Invariants/                      All IPatternInvariant implementations
│   ├── Syntax/
│   │   ├── CharacterClasses/            CharacterClass AST (discriminated union)
│   │   ├── PatternSyntaxProfile.cs      Dialect feature flags
│   │   └── PatternToken.cs              All token types
│   ├── Tokenization/                    PatternScanner and per-token tokenizers
│   └── Validation/                      PatternValidator (malformed-input detection)
├── Traversal/
│   ├── Policies/                        SyncTraversalPolicy, AsyncTraversalPolicy
│   ├── TraversalDecisionProvider.cs
│   ├── TraversalEvaluator.cs
│   ├── TraversalExecutor.cs
│   ├── TraversalFrontier.cs             ArrayPool-backed stack/queue
│   ├── TraversalPlan.cs
│   ├── TraversalPlanBuilder.cs
│   └── TraversalOptions.cs
├── FileQuery.cs                         Immutable query descriptor
├── FileQueryBuilder.cs                  Fluent builder
├── FileQueryEngineExtensions.cs         From() extension method
├── FileQueryOptions.cs                  Complete query configuration
├── FileQueryRuntime.cs                  Composition root
├── IFileQueryEngine.cs                  Public engine contract
└── PatternInput.cs                      Public pattern configuration type
```

---

## Tests — `tests/Jeninnet.FileQuery.Tests`

```
Jeninnet.FileQuery.Tests/
├── Architecture/          Layer boundary, construction authority, allocation tests
├── Correctness/           Determinism, precedence, directory-only behaviour
├── Core/
│   ├── FileCollectorAsync/   Async enumeration tests (many sub-categories)
│   └── FileCollectorSync/    Sync enumeration tests (many sub-categories)
├── Infrastructure/        TestMatcher, TestPattern, TestPath helpers
├── Integration/           End-to-end file enumeration tests
├── Invariants/            PatternInvariant validation tests
├── Matchers/              GitIgnore, Glob, Regex, Hybrid matcher tests
├── PatternEngine/         Compiler and token tests
├── Patterns/              Analysis, Syntax, Validation tests
├── Regression/            Tests for fixed bugs
└── Shared/                TestEnvironment, TestPathUtils, PatternHelpers
```

---

## Benchmarks — `benchmarks/Jeninnet.FileQuery.Benchmarks`

One benchmark class per component:

| File | Measures |
|------|---------|
| `GitIgnoreMatcherBenchmark` | GitIgnore rule evaluation speed and allocation |
| `GlobMatcherBenchmark` | Glob rule evaluation |
| `RegexMatcherBenchmark` | Regex rule evaluation and cache performance |
| `HybridMatcherBenchmark` | Combined matcher pipeline |
| `PatternClassifierBenchmark` | Classification throughput |
| `PatternTokenizerBenchmark` | Tokenization cost per pattern |
| `TraversalBenchmark` | End-to-end directory traversal |
| `CharacterClassMatcherBenchmark` | Character class element evaluation |
| `PatternCompilationColdStartBenchmark` | One-time compilation cost |
| `TraversalStrategyBenchmark` | DFS vs BFS comparison |
| `RegexMatcherCacheBenchmark` | Cache hit vs cold-start regex |
| `PatternPipelineAllocationBenchmark` | Sub-set lazy allocation verification |

---

## Central Package Management

All NuGet package versions are declared in `Directory.Packages.props`. Individual `.csproj` files reference packages without version numbers. To update a package, change the version in one place.

---

## Shared Build Properties

`Directory.Build.props` sets properties inherited by all projects:

- `TargetFramework`: `net10.0`
- `LangVersion`: `14.0`
- `Nullable`: `enable`
- `ImplicitUsings`: `enable`
- `TreatWarningsAsErrors`: configurable per build
- `GenerateDocumentationFile`: `true` (produces `.xml` for docfx)
- `AnalysisLevel`: `latest` with `AnalysisMode`: `recommended`