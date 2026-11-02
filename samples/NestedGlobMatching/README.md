# NestedGlobMatching Sample

Select several extensions at any depth with multiple recursive Glob patterns.

**What it teaches:** `**` (recursive wildcard), multiple include patterns, and deterministic
result ordering.

```csharp
.Where("**/*.cs", "**/*.md")
```

## Run

```bash
dotnet run --project samples/NestedGlobMatching/NestedGlobMatching.csproj
```

Expected: **6 files** — the 4 `.cs` files plus `ReadMe.md` and `docs/guide.md`.

## When to use this

Use multiple Glob patterns when each pattern describes an independent family of files. Use GitIgnore
negation instead when the intent is "everything except …" with exclusions and re-inclusions.

## Related

- [Glob reference](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/glob-patterns.md)
- [Pattern overview](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-overview.md)
- [All samples](../README.md)
