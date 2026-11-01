# EntryMetadata Sample

Read file metadata and apply size/time/attribute filters **during the same traversal** that
matches the files.

**What it teaches:** `ExecuteEntries()`, `MinSize`, `MaxSize`, `ModifiedAfter`, `ModifiedBefore`,
`AttributesToSkip`, and `FileQueryEntry`.

## Run

```bash
dotnet run --project samples/EntryMetadata/EntryMetadata.csproj
```

Expected: only `large.dat`. `small.dat` is rejected by `MinSize(100)` and is older than
`ModifiedAfter`; `large.dat` passes both filters.

The output prints, for every entry:

- full and relative paths
- size in bytes
- creation and last-write timestamps
- attributes
- directory state

## Code

```csharp
var entries = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.dat"])
    .MinSize(100)
    .MaxSize(4_096)
    .ModifiedAfter(oldTimestamp)
    .ModifiedBefore(DateTimeOffset.UtcNow.AddDays(1))
    .ExecuteEntries();
```

No `FileInfo` calls are made per match: `Length` and timestamps come from the OS enumeration that
already produced the entry.

## Related

- [Entry metadata and filters](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/entry-metadata-and-filters.md)
- [All samples](../README.md)
