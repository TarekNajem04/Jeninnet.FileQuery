# Recursion Depth

Control how deeply the engine traverses below the query root.

---

## Fluent configuration

```csharp
// Unlimited depth (default)
var all = FileQuery.From(root)
    .WithRecursion()
    .Build();

// Root only
var direct = FileQuery.From(root)
    .WithoutRecursion()
    .Build();

// Root + two levels
var shallow = FileQuery.From(root)
    .WithMaxDepth(2)
    .Build();
```

`WithMaxDepth(0)` visits the root only. `WithMaxDepth(-1)` —
`FileQueryOptions.UNLIMITED_RECURSION_DEPTH` — allows unlimited depth.

`WithMaxDepth` throws `ArgumentOutOfRangeException` for values below `-1`.

---

## Depth semantics

| Depth | Directories visited |
|---|---|
| `0` | Root only |
| `1` | Root + immediate subdirectories |
| `2` | Root + two levels of subdirectories |
| `-1` (default) | Unlimited |

Depth-based pruning is cheaper than pattern-based depth limiting: the traversal stops enumerating
directories that exceed the limit instead of entering them and discarding results.

---

## Interaction with directory pruning

`WithMaxDepth` and directory-only patterns (`bin/`) are evaluated independently. A directory at
depth 1 may be pruned by a pattern even when the depth limit would permit entering it.

A directory is pruned when **either** condition is met:

- its depth exceeds `MaxRecursionDepth`, or
- its most recent pattern match is a non-negated exclusion.

---

## Combining with traversal options

```csharp
var query = FileQuery.From(root)
    .WithMaxDepth(3)
    .WithTraversal(new TraversalOptions(
        Strategy: TraversalStrategy.BreadthFirst,
        SymlinkPolicy: SymlinkPolicy.FollowWithCycleDetection))
    .Build();
```

`WithTraversal` exposes:

| Option | Values |
|---|---|
| `Strategy` | `DepthFirst` (default), `BreadthFirst` |
| `SymlinkPolicy` | `Ignore` (default), `Follow`, `FollowWithCycleDetection` |
| `UseAsync` | Enables asynchronous I/O where supported |

Use `FollowWithCycleDetection` for trees that may contain symlink cycles. `Follow` performs no cycle
detection and can traverse indefinitely on a cycle.

---

## Related

- [Traversal model](traversal-model.md)
- [Multiple roots](../guides/multiple-roots.md)
- [Error handling and validation](../guides/error-handling-and-validation.md)
