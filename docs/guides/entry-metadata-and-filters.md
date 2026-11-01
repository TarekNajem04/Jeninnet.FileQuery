# Entry Metadata and Filters

`ExecuteEntries()` and `ExecuteEntriesAsync()` return `FileQueryEntry` values instead of bare
paths. Metadata comes from the same directory enumeration used for matching, so requesting it
does not add per-file system calls.

---

## Reading entries

```csharp
var entries = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .ExecuteEntries();

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.RelativePath} ({entry.Length} bytes)");
    Console.WriteLine($"  full: {entry.FullPath}");
    Console.WriteLine($"  created: {entry.CreationTimeUtc:O}");
    Console.WriteLine($"  modified: {entry.LastWriteTimeUtc:O}");
    Console.WriteLine($"  directory: {entry.IsDirectory}");
}
```

| Property | Meaning |
|---|---|
| `FullPath` | Absolute path reported by the operating system. |
| `RelativePath` | Root-relative path using forward slashes and no trailing slash. |
| `Length` | Size in bytes as reported by the enumeration. |
| `CreationTimeUtc` | Creation time in UTC, or `DateTime.MinValue` when unavailable. |
| `LastWriteTimeUtc` | Last-write time in UTC, or `DateTime.MinValue` when unavailable. |
| `Attributes` | Raw `FileAttributes` flags. |
| `IsDirectory` | Whether the directory attribute is set. |

The path-only `Execute()` API is unchanged and keeps its allocation profile.

To include directories in the result stream, add `IncludeDirectories()`; see
[Directory Results](directory-results.md).

---

## Filters

Filters run before pattern matching and use metadata already present on the enumeration entry.

```csharp
var entries = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.bin"])
    .MinSize(1_000)                                  // inclusive
    .MaxSize(50_000_000)                             // inclusive
    .ModifiedAfter(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))  // exclusive
    .ModifiedBefore(DateTimeOffset.UtcNow)           // exclusive
    .AttributesToSkip(FileAttributes.Hidden | FileAttributes.System)
    .ExecuteEntries();
```

- Size and time filters apply to files only; directory entries do not represent the aggregate
  size or timestamp of their contents.
- `AttributesToSkip` applies to files and directories. A matching directory is pruned, so its
  subtree is not traversed.
- Invalid ranges (negative sizes, minimum greater than maximum, or `ModifiedAfter` not earlier
  than `ModifiedBefore`) fail when the query is built.

A runnable sample covering size, time, and attribute filters is available in
[`samples/EntryMetadata`](https://github.com/TarekNajem04/Jeninnet.FileQuery/tree/main/samples/EntryMetadata).
