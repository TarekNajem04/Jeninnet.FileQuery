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

## Measured Dataset Profile (Phase 3 Probe)

Allocation targets are sized against a probe of a real generated dataset
(50,000 sampled files under a typical root, depth ≤ 3):

| Metric | Measured |
|--------|----------|
| Average full path length | 114 chars |
| Average relative path length | 39 chars |
| Average file name length | 17 chars |
| Average depth | 2.4 levels (max 3) |
| Directories in dataset | 4,096 |

Per-entry allocation cost at these lengths:

| Operation | Size |
|-----------|------|
| `new string(fullPath)` (114 chars) | 256 B |
| `new string(relativePath)` (39 chars) | 104 B |
| `new string(fileName)` (17 chars) | 56 B |
| `string.Concat(relativePath, fileName)` | 136 B |

Budget at one million entries (1,004,096):

| Strategy | Total |
|----------|-------|
| Relative string per entry + full path for matches | 313.5 MB |
| Relative path composed in a reusable buffer (0 B per non-match) + full path only for matches | 213.9 MB |
| Unavoidable result strings (matches only) at measured 256 B/path | 100 MB per 409,600 matches |

**Decision:** the hot path must compose relative paths in a reusable buffer
(rented `char[]`, extended only when a file actually matches) instead of
allocating a relative string per entry. Result strings — one `string` per
matched file — remain the only unavoidable allocation.

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

- **Result path strings** — each matched file path is a `string` returned to the caller. At the measured average of 114 chars (~256 B per string), 10,000 matches cost approximately 2.5 MB. This is proportional to result count, not dataset size.
- **First regex compilation** — the first call to a regex pattern compiles a `Regex` object. Subsequent calls with the same `(text, case sensitivity)` key return the cached instance.
- **Query initialization** — pattern compilation allocates once per query build.