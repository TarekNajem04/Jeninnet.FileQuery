# Recursion Depth

The engine provides precise control over how deeply it traverses the directory tree.

---

## Configuration Properties

Both properties live on `FileQueryOptions`:

```csharp
public sealed record FileQueryOptions
{
    public bool RecurseSubdirectories { get; init; } = true;
    public int  MaxRecursionDepth     { get; init; } = -1;   // -1 = unlimited
}
```

---

## RecurseSubdirectories

When `false`, only the root directory is inspected. No subdirectories are entered regardless of `MaxRecursionDepth`.

```csharp
var options = new FileQueryOptions {
    PatternInput          = new(patterns: ["**", "!*.cs"]),
    RecurseSubdirectories = false   // root only
};
```

---

## MaxRecursionDepth

Controls the maximum depth of directory traversal. Depth is counted in directory levels below the root:

| Depth | Directories visited |
|-------|-------------------|
| `0` | Root only |
| `1` | Root + immediate subdirectories |
| `2` | Root + two levels of subdirectories |
| `-1` (default) | Unlimited |

```csharp
var options = new FileQueryOptions {
    PatternInput          = new(patterns: ["**", "!*.cs"]),
    RecurseSubdirectories = true,
    MaxRecursionDepth     = 2   // root + two levels
};
```

Using `MaxRecursionDepth` is more efficient than pattern-based depth limiting because the traversal engine stops enumerating directories that exceed the limit rather than entering them and discarding results.

---

## Fluent API

The fluent builder exposes `WithRecursion` and `WithoutRecursion` for the most common cases:

```csharp
// Unlimited recursion (default)
FileQuery.From(root).WithRecursion().Where(...).Build();

// Root only
FileQuery.From(root).WithoutRecursion().Where(...).Build();
```

For `MaxRecursionDepth`, use `FileQueryOptions` directly — the fluent builder does not expose a depth parameter to keep the API surface small.

---

## Interaction with Directory Pruning

`MaxRecursionDepth` and directory-only patterns (`bin/`) are evaluated independently. A directory at depth 1 may be pruned by a pattern even when `MaxRecursionDepth` would permit entering it.

The engine prunes a directory when **either** condition is met:
- The directory's depth exceeds `MaxRecursionDepth`, or
- The directory's most recent pattern match is a non-negated exclusion.

---

## Examples

### Only the root directory

```csharp
new FileQueryOptions {
    PatternInput      = new(patterns: ["**", "!*.cs"]),
    MaxRecursionDepth = 0
}
```

### Exactly two levels

```csharp
new FileQueryOptions {
    PatternInput      = new(patterns: ["**", "!*.json"]),
    MaxRecursionDepth = 2
}
```

### Unlimited (default)

```csharp
new FileQueryOptions {
    PatternInput = new(patterns: ["**", "!**/*.log"])
    // MaxRecursionDepth defaults to -1
}
```

---

## Validation

`MaxRecursionDepth` must be ≥ `-1`. Values below `-1` throw `ArgumentOutOfRangeException` during `FileQueryOptions.Validate()`, which is called automatically when building a query.