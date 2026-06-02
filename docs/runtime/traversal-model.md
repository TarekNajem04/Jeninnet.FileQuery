# Traversal Model

The traversal layer is responsible for discovering filesystem paths. It is entirely independent of the pattern matching layer — the traversal component never inspects pattern strings.

---

## Traversal Strategies

Two strategies are available, configured through `TraversalOptions`:

```csharp
var options = new FileQueryOptions {
    Traversal = new TraversalOptions(
        Strategy: TraversalStrategy.DepthFirst   // default
    )
};
```

### DepthFirst (default)

Uses a stack (LIFO). Visits all descendants of a directory before moving to the next sibling. Results from deeply nested files appear before shallower siblings in adjacent directories.

```
root/
  a/
    a1.txt   ← appears first
    a2.txt   ← appears second
  b/
    b1.txt   ← appears third
```

Depth-first has better cache locality for trees where the majority of files are in leaf directories. It is the correct choice for most workloads.

### BreadthFirst

Uses a queue (FIFO). Visits all entries at the current depth before descending. Files closer to the root appear earlier in the result stream.

```
root/
  a.txt      ← appears first
  b/
    b1.txt   ← appears later
```

Use breadth-first when shallow results matter more than deep ones — for example, when looking for project-root files like `README.md` or `*.sln`.

---

## Streaming Pipeline

Traversal produces a streaming result. No internal buffer accumulates all matching paths before the first result is returned. The sequence is:

```
Frontier.Pop/Dequeue()
   ↓
FileSystem.Enumerate(directory)
   ↓  (for each entry)
PathUtilities.BuildRelativePath(root, entry)
   ↓
HybridPathMatcher.Match(patterns, context)
   ↓
TraversalEvaluator.Evaluate(outcome, pathKind, depth)
   ↓
If file and ShouldYield → yield path
If directory and ShouldTraverse → push to frontier
```

The frontier is implemented as a pooled `ArrayPool<TraversalFrame>` buffer that doubles in capacity when full. No `List<T>` or `Queue<T>` allocations occur during normal traversal.

---

## Symlink Policy

Symbolic links and reparse points are handled according to `TraversalOptions.SymlinkPolicy`:

```csharp
public enum SymlinkPolicy
{
    Ignore,                   // default — reparse points are skipped
    Follow,                   // follow symlinks
    FollowWithCycleDetection  // follow symlinks, prevent infinite loops
}
```

The default `Ignore` is the safe choice. `Follow` is appropriate for filesystems where symlinks are used as first-class directory structures. `FollowWithCycleDetection` adds a `HashSet<string>` to detect circular references at the cost of additional allocation per traversal.

---

## Inaccessible Directories

```csharp
var options = new FileQueryOptions {
    IgnoreInaccessible = true   // default — skip locked / denied directories
};
```

When `true`, directories that throw `UnauthorizedAccessException`, `IOException`, or `DirectoryNotFoundException` are silently skipped and traversal continues. When `false`, the exception propagates to the caller.

---

## TraversalPlan

`TraversalPlanBuilder.Build(query)` produces an immutable `TraversalPlan` that encapsulates everything the executor needs:

```
TraversalPlan
├── RootDirectory       (absolute, normalized, trailing separator stripped)
├── FileSystem          (IFileSystem — the real filesystem or a test double)
├── Traversal           (TraversalConfiguration — depth, strategy, symlinks)
├── Matching            (MatchingConfiguration — patterns, case sensitivity)
├── Matcher             (IPathMatcher — HybridPathMatcher or dialect-specific)
├── CompiledPatterns    (ICompiledPatternSet — pre-built sub-sets)
└── Evaluator           (ITraversalEvaluator — decision logic)
```

The plan is built once per `Execute` call. It captures the resolved case sensitivity (platform default → Sensitive or Insensitive) and the compiled pattern set so that repeated calls to the same query options do not recompile patterns.

---

## Path Normalization

Before pattern matching, each discovered path is converted to a root-relative forward-slash form:

```
C:\repo\src\engine\FileQueryEngine.cs
  → root: C:\repo
  → relative: src/engine/FileQueryEngine.cs
```

Directories append a trailing slash:

```
C:\repo\src\engine\
  → src/engine/
```

The trailing slash on directory paths is what allows directory-only patterns (`bin/`) to match directories precisely without also matching files named `bin`.

---

## Async Traversal

`ExecuteAsync` returns an `IAsyncEnumerable<string>` and streams results without
materializing the full traversal. Cancellation is checked before traversal
starts, between directory frames, and while entries are enumerated.

The underlying .NET directory enumeration APIs are synchronous. The async path
therefore provides cooperative asynchronous consumption and cancellation
checkpoints; it does not imply parallel traversal or fully non-blocking
filesystem I/O. Attribute retrieval may be offloaded per entry, but directory
discovery still follows the configured depth-first or breadth-first traversal
order.

Consumers that need higher throughput across independent roots should run
separate queries explicitly and bound their own concurrency.
