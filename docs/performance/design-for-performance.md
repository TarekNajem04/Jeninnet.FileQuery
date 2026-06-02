# Design for Performance

This document describes the architectural and implementation decisions made specifically to achieve high performance in Jeninnet.FileQuery.

---

## Compile Once, Match Many

The most important performance decision is the separation between compilation and evaluation. Patterns are compiled into an executable representation exactly once per query. During traversal, the matcher operates on pre-built token lists and sub-set indexes — no string parsing occurs per path.

```
Query initialization  →  PatternPipeline.Compile()      (paid once)
Per-path evaluation   →  HybridPathMatcher.Match()      (paid per file)
```

For a query that evaluates one million files against ten patterns, the compilation cost is paid once and the matching cost is paid one million times. Minimizing per-path cost has far more impact than minimizing compilation cost.

---

## Pre-Built Sub-Sets

`CompiledPatternSet` partitions patterns into three sub-sets during construction:

```csharp
// Built once at query initialization
var gitIgnoreSubSet = patterns where PatternKind == GitIgnore;
var globSubSet      = patterns where PatternKind == Glob;
var regexSubSet     = patterns where PatternKind == Regex;
```

`HybridPathMatcher.Match` checks each sub-set with a single null check before delegating:

```csharp
if (instructions.GitIgnoreSubSet is not null)
    result = GitIgnoreMatcher.Match(instructions.GitIgnoreSubSet, context);
```

No grouping, no LINQ, no dictionary lookup at match time.

**Lazy allocation:** A sub-set list is only allocated when the first pattern of that kind is encountered during `CompiledPatternSet` construction. A pure GitIgnore pattern set allocates no Glob or Regex sub-list objects.

---

## Index-Based Loops Everywhere

Every loop in the matching hot path uses `for` with an index rather than `foreach` over an interface. This eliminates one heap-allocated `IEnumerator<T>` box per loop per call.

Impact measured by benchmarks:

| Loop style | Allocation per call |
|-----------|-------------------|
| `foreach (var p in patterns)` where `patterns : ICompiledPatternSet` | ~40 B |
| `for (var i = 0; i < patterns.Count; i++)` | 0 B |

---

## `ref struct` Path Representation

`PathView` and `PathSegmentEnumerator` are both `ref struct`. Segment enumeration is stack-allocated. Copying a `PathSegmentEnumerator` for speculative backtracking is a zero-cost value copy — no heap involvement.

---

## `ArrayPool<T>` for Traversal Frontier

`TraversalFrontier` rents a `TraversalFrame[]` array from `ArrayPool<TraversalFrame>.Shared`. The array doubles in capacity when full (copying from `_head` to `0` to compact the buffer). The array is returned to the pool when the `using` block in `TraversalExecutor.Execute` completes.

This means the traversal frontier produces no GC pressure regardless of tree depth or breadth.

---

## Compilation Pipeline Bypass

When the caller supplies an explicit `PatternKind`, the compilation path skips `PatternCanonicalizer` and `PatternClassifier` entirely:

```csharp
// Fast path — bypasses classification
CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);

// Standard path — runs classifier
CompiledPatternFactory.Compile(classifiedPatternSet);
```

Eliminated intermediate allocations in the fast path (per pattern):
- `Dictionary<PatternKind, IEnumerable<string>>` — 72 B
- `CanonicalPatternInput` + `ImmutableDictionary` — ~200 B
- `HashSet<CanonicalPattern>` — 80 B
- `List<CanonicalPattern>` + `CanonicalPatternSet` — 72 B
- `List<ClassifiedPattern>` + `ClassifiedPatternSet` — 72 B

Total eliminated: ~496 B × N patterns per query build.

---

## `ImmutableDictionary` Copy Eliminated

`TraversalPlanBuilder` previously called `PatternsMerger.Merge(...).ToImmutableDictionary()`, which copied an already-materialized `Dictionary` into a second immutable structure. `MatchingConfiguration.TypedPatterns` was changed to `IReadOnlyDictionary` — the dictionary from `PatternsMerger` is passed directly, eliminating the copy.

---

## Segment Token List Initial Capacity

`TokenizeSegment` initializes each per-segment `List<IPatternToken>` with capacity 3:

```csharp
var tokens = new List<IPatternToken>(SegmentInitialTokenCapacity); // 3
```

Most segments contain 1–3 tokens (e.g., `*.cs` → `[WildcardToken, LiteralToken(".cs")]`). Initial capacity 3 avoids the first internal array resize for the vast majority of segments.

---

## Character Class Matching — No Closures

`CharacterClassMatches` previously used `cls.Elements.Any(element => MatchesElement(element, c))`. The lambda captured the local `char c`, causing the compiler to emit a display-class allocation per call. Replaced with a `for` loop with early exit:

```csharp
for (var i = 0; i < elements.Count; i++) {
    if (MatchesElement(elements[i], c)) {
        inSet = true;
        break;
    }
}
```

---

## What Is NOT Optimized (and Why)

**Regex compilation** uses `RegexOptions.Compiled`. This is intentionally slower at startup than `RegexOptions.None` but significantly faster at match time. For queries that evaluate thousands of files, the amortized cost of compilation is negligible compared to the per-file savings.

**Path string allocation** is unavoidable. Each matched path is a `string` object returned to the caller. This is the dominant allocation in any realistic workload and cannot be eliminated without changing the public API contract.

**`FileSystem.Enumerate`** calls `File.GetAttributes(path)` for each filesystem entry. This is a system call — it cannot be avoided if directory/file classification is needed. The cost is proportional to the number of entries, not to the number of patterns.