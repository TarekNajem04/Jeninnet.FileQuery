# BasicMatching Sample

The smallest useful `Jeninnet.FileQuery` program: build a query with no patterns and stream every
file in a deterministic demo tree.

**What it teaches:** builder → `Build()` → `Execute()`; the default query includes every file.

## Run

```bash
dotnet run --project samples/BasicMatching/BasicMatching.csproj
```

Expected: **12 files**, including nested files under `assets/`, `docs/`, `logs/`, and `src/`.

## Code

```csharp
var query = FileQuery.From(root).Build();

foreach (var path in FileQueryRuntime.Create().Execute(query))
{
    Console.WriteLine(path);
}
```

The sample creates and deletes its own fixture tree through
[`SampleUtils`](../SamplesUtilities/README.md), so it is safe to run repeatedly.

## Next

- [DirectoryOnlyMatching](../DirectoryOnlyMatching/README.md) — match directories with `logs/`.
- [PatternLanguage](../PatternLanguage/README.md) — include/exclude ordering.
- [All samples](../README.md)
- [Quick start](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/getting-started/quick-start.md)
