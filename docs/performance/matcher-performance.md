# Matcher Performance

Jeninnet.FileQuery is designed for high-performance file querying
across large directory trees.

The engine emphasizes:

• zero hot-path allocations in matchers
• low allocations in the compilation pipeline
• predictable execution time
• efficient pattern evaluation

---

## Benchmark Environment

BenchmarkDotNet v0.15.8
Windows 11 (10.0.26200.8039 / 25H2)
Intel Core i7-8850H 2.60 GHz (Coffee Lake), 6 physical / 12 logical cores
.NET SDK 10.0.201 / Runtime 10.0.5 (RyuJIT x86-64-v3)
All benchmarks executed in Release mode with Tiered JIT enabled.

---

## v1.0 Benchmark Results

| Component | Mean | Allocated | Notes |
| ------------------- | --------- | --------- | ---------------------------------------- |
| PatternClassifier | 64 ns | 0 B | Allocation-free classification |
| GlobMatcher | 261 ns | 0 B | Index-based loop, no enumerator boxing |
| RegexMatcher | 85 ns | 0 B | Fixed: was 40 B (enumerator boxing) |
| GitIgnoreMatcher | 771 ns | 0 B | Fixed: was 40 B (enumerator boxing) |
| HybridMatcher | 742 ns | 0 B | Fixed: was 120 B (three enumerators) |
| PatternTokenizer | 4.85 μs | ~9 KB | Fixed: was 12 KB (pipeline bypass) |
| Traversal (QueryFiles) | 2.0 ms | ~30 KB | Fixed: was 35 KB (matcher + dict copy) |

---

## Matcher Allocation History

### Before v1.0 (original design)

| Matcher | Allocated | Root cause |
| --------------- | --------- | -------------------------------------------------- |
| GlobMatcher | 0 B | Already used index-based `for` loop |
| GitIgnoreMatcher | 40 B | `foreach` over `ICompiledPatternSet` interface boxed `IEnumerator<T>` |
| RegexMatcher | 40 B | Same cause; one enumerator per call |
| HybridMatcher | 120 B | Two GitIgnore enumerators (80 B) + one Regex enumerator (40 B) |

### After v1.0 allocation fixes

All three hot-path matchers now use index-based `for` loops.
A `foreach` over an interface-typed collection creates a boxed
`IEnumerator<T>` (approximately 40 B) on every call regardless
of the number of elements. Replacing these loops eliminated all
hot-path allocation from matching.

**Pattern iteration change (GitIgnoreInstructionMatcher, RegexInstructionMatcher):**

```csharp
// Before: 40 B per call (IEnumerator<ICompiledPattern> boxed on heap)
foreach (var pattern in patterns) { ... }

// After: 0 B (no enumerator object created)
for (var i = 0; i < patterns.Count; i++) {
    var pattern = patterns[i];
    ...
}
```

The `SegmentInstructionMatcher.CharacterClassMatches` method was also fixed.
The previous `cls.Elements.Any(element => MatchesElement(element, c))` lambda
captured the local `char c`, causing the compiler to emit a display-class
closure allocation on every call. Replaced with a manual loop with early exit.

---

## Compilation Pipeline Allocation History

### PatternTokenizer benchmark: 12 KB → ~9 KB

When the caller supplies an explicit `PatternKind`, the compilation path now
bypasses the `PatternCanonicalizer` → `PatternClassifier` chain entirely and
constructs a `ClassifiedPatternSet` directly. The eliminated intermediate
objects per-pattern were:

```
CanonicalPatternInput  +  ImmutableDictionary  (~200 B)
HashSet<CanonicalPattern>                       (~80 B)
List<CanonicalPattern>                          (~40 B)
CanonicalPatternSet                             (~32 B)
List<ClassifiedPattern>                         (~40 B)
ClassifiedPatternSet                            (~32 B)
```

Total eliminated: ~424 B × 6 patterns = ~2.5 KB per `Compile(PatternKind, patterns[])` call.

### TraversalBenchmark: 35 KB → ~30 KB

Three sources of reduction:

**Matcher loop fix:** ~2.7 KB (confirmed by benchmark drop from 35.02 to 32.29 KB)

**`CompiledPatternSet` lazy sub-list allocation:** For a pure GitIgnore pattern set,
the previous constructor always pre-allocated three `List<ICompiledPattern>` at
`patterns.Count` capacity (GitIgnore + Glob + Regex), then discarded the unused two.
Replaced with `??=` lazy initialization: each list is only created when the first
pattern of that kind is encountered.

**`ImmutableDictionary` copy eliminated:** `TraversalPlanBuilder.Build` called
`PatternsMerger.Merge(...).ToImmutableDictionary()`, copying an already-materialized
`Dictionary<PatternKind, ImmutableArray<string>>` into a second immutable structure.
Since `MatchingConfiguration` is internal and consumed once per query, the copy
was unnecessary. Replaced with `IReadOnlyDictionary`.

**`CompileSet` LINQ chain replaced:** `typed.Values.SelectMany(p => p.Patterns).ToList()`
allocated a `SelectManyIterator` and an intermediate list. Replaced with a pre-sized
`for` loop.

---

## Hybrid Matcher Performance History

The HybridPathMatcher sub-set architecture was redesigned once between
prototype and v1.0 to eliminate the LINQ grouping performed on every call.

| Version | Mean | Allocated | Change |
| ---------------------------------- | ---------- | --------- | -------------------------------------------- |
| Original (LINQ grouping per call) | ~1.11 μs | ~1.17 KB | Baseline |
| First optimization (filter caching) | ~981 ns | ~816 B | Sub-sets cached after first call |
| v1.0 (pre-compiled sub-sets) | ~809 ns | 144 B | Sub-sets built during compilation, not matching |
| v1.0 + enumerator fix | **742 ns** | **0 B** | Enumerator boxing eliminated |

Result from original to v1.0 final: ~33% faster, ~100% allocation reduction.

---

## Observations

**GitIgnore and Glob matchers** are suitable for large-scale directory traversal.
At 261–771 ns per path evaluation, a directory tree of one million files can be
evaluated in under one second of pure matching time.

**Regex matchers** add flexibility at a small cost. The 85 ns mean includes a
`ConcurrentDictionary` lookup keyed by `(pattern text, case sensitivity)`. The
first call per key compiles the `Regex`; subsequent calls are pure cache hits.

**Pattern compilation** is a one-time initialization cost per query. For applications
that reuse the same pattern set across many root paths, a `PrecompiledQuery` API
is planned for v1.1.

**Traversal** is typically the dominant cost in real workloads. The 2 ms benchmark
covers a real directory tree traversal including `File.GetAttributes()` calls per
entry. Pattern matching contributes a small fraction of this time.

---

## Conclusion

Jeninnet.FileQuery provides a high-performance foundation for file system
querying in scenarios such as:

• build systems
• code analysis tools
• repository scanners
• CLI utilities
• backup and indexing tools

The zero-allocation matching hot path ensures that GC pressure scales only with
the number of file path strings returned to the caller, not with the complexity
of the pattern set.