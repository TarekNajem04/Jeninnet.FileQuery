# Allocation Strategy

Jeninnet.FileQuery is designed around a two-phase allocation model: allocate during initialization, allocate nothing during evaluation.

---

## Two-Phase Model

| Phase | When | Allocations | Acceptable? |
|-------|------|-------------|-------------|
| **Initialization** | `FileQuery.Build()` | Pattern compilation, compiled pattern set, traversal plan | Yes — one-time cost |
| **Evaluation** | `Execute()` per path | **Zero** from matchers | Required |
| **Results** | Each `yield return` | One `string` per matched file | Unavoidable |

---

## Initialization Allocations

During query build, the following objects are allocated once and reused for the lifetime of the query:

- `ClassifiedPatternSet` — list of classified patterns
- `CompiledPattern` per pattern — immutable token list
- `CompiledPatternSet` — main set plus optional GitIgnore, Glob, Regex sub-sets
- `TraversalPlan` — configuration snapshot
- `TraversalFrontier` — `ArrayPool<TraversalFrame>` buffer (returned on dispose)

The initialization cost is approximately 200–500 bytes per pattern for the typical GitIgnore case. When `PatternKind` is supplied explicitly, the canonicalization and classification stages are bypassed, reducing this to roughly 80–120 bytes per pattern.

---

## Hot-Path Zero-Allocation Rules

These rules govern all code in the evaluation hot path:

### Rule 1 — No `foreach` over interface-typed collections

A `foreach` over `ICompiledPatternSet`, `IReadOnlyList<T>`, or any other interface boxes the enumerator struct onto the heap (~40 bytes per call). All hot-path loops use index-based `for` loops.

```csharp
// Prohibited in hot path
foreach (var pattern in patterns) { ... }

// Required
for (var i = 0; i < patterns.Count; i++) {
    var pattern = patterns[i];
    ...
}
```

### Rule 2 — No closures in hot-path LINQ

Lambda expressions that capture local variables create a display-class heap allocation. `CharacterClassMatches` previously used `cls.Elements.Any(e => MatchesElement(e, c))` — the lambda captured `c`, producing a display class per call. Replaced with a `for` loop.

### Rule 3 — `PathView` and `PathSegmentEnumerator` are `ref struct`

Both types are stack-allocated. Copying a `PathSegmentEnumerator` for speculative matching is a stack copy — no heap allocation.

### Rule 4 — `TraversalFrontier` uses `ArrayPool<T>`

The frontier buffer is rented from `ArrayPool<TraversalFrame>.Shared` and returned when the `using` block in `TraversalExecutor.Execute` disposes it. No `List<T>` or `Queue<T>` is allocated during traversal.

---

## Measuring Allocations

Use BenchmarkDotNet with `[MemoryDiagnoser]` to verify zero allocations in the hot path:

```csharp
[MemoryDiagnoser]
public class GitIgnoreMatcherBenchmark {
    [Benchmark]
    public bool Match() => _matcher.Match(_patterns, _context) is MatchOutcome.Include;
}
```

Expected result after all fixes:

| Matcher | Allocated |
|---------|-----------|
| `GlobMatcher` | 0 B |
| `RegexMatcher` | 0 B |
| `GitIgnoreMatcher` | 0 B |
| `HybridMatcher` | 0 B |

---

## Architecture Test

The allocation contract is enforced by an architecture test:

```csharp
[TestMethod]
public void Matching_Must_Not_Allocate()
{
    GC.Collect();
    var before = GC.GetAllocatedBytesForCurrentThread();

    matcher.Match(compiledPatternSets, context);

    var after = GC.GetAllocatedBytesForCurrentThread();

    Assert.AreEqual(before, after,
        $"Matching allocated {after - before} bytes — must be 0.");
}
```

This test fails if any change to the matching hot path introduces a heap allocation.

---

## Unavoidable Allocations

The following allocations are unavoidable and are not considered regressions:

- **Result path strings** — each matched file path is a `string` returned to the caller. For a query returning 10,000 files, this is approximately 500 KB assuming an average path length of 50 characters.
- **First regex compilation** — the first call to a regex pattern compiles a `Regex` object. Subsequent calls with the same `(text, case sensitivity)` key return the cached instance.
- **Query initialization** — pattern compilation allocates once per query build.