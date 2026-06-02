# Matching Layer

The matching layer (`Jeninnet.FileQuery.Matching`) evaluates compiled patterns against normalized path strings. It is the performance-critical core of the engine.

## Responsibilities

- Match paths using compiled patterns
- Produce zero heap allocations during evaluation
- Support GitIgnore, Glob, and Regex dialect semantics
- Respect case sensitivity configuration

## Forbidden

- Pattern parsing or raw string inspection
- Filesystem access of any kind
- Mutable state between path evaluations (matchers are stateless)

## Pipeline

```
PatternInput (public)
   ↓
PatternsMerger.Merge()
   ↓
CompiledPatternSetFactory.Create()
   ↓  (via PatternPipeline)
ClassifiedPatternSet
   ↓
PatternCompiler per dialect
   ↓
CompiledPatternSet
   ├── GitIgnoreSubSet
   ├── GlobSubSet
   └── RegexSubSet
   ↓
HybridPathMatcher
   ↓
MatchPrecedenceResolver
   ↓
GitIgnoreInstructionMatcher / GlobInstructionMatcher / RegexInstructionMatcher
```

## Closed-World Contract

The matching layer follows a Bertrand Meyer–style closed-world contract: **parsing and classification happen before matching begins**. The matchers receive only `ICompiledPatternSet` — they never inspect raw pattern strings.

This invariant is enforced by an architecture test:

```csharp
[TestMethod]
public void EngineLayer_Must_Not_Reference_Patterns_Namespace()
{
    // Uses reflection to verify no type in the Engine namespace
    // references any type in the Patterns namespace
}
```

## Zero-Allocation Contract

All hot-path loops use index-based `for` loops. No `foreach` over interface-typed collections. No LINQ closures. No enumerator boxing.

Verified by:

```csharp
[TestMethod]
public void Matching_Must_Not_Allocate()
{
    var before = GC.GetAllocatedBytesForCurrentThread();
    matcher.Match(compiledPatternSets, context);
    var after = GC.GetAllocatedBytesForCurrentThread();
    Assert.AreEqual(before, after);
}
```

## Key Rule

> **Matching must operate only on compiled patterns, never on raw pattern strings.**