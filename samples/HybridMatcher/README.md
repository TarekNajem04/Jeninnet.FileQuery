# HybridMatcher Sample

Mix GitIgnore-style globs, negation, and a Regex rule in one query.

**What it teaches:** Hybrid classification, per-dialect matching, and precedence.

```csharp
.Where(
    "**",                 // GitIgnore-style exclusion
    "!**/*.cs",           // negation re-includes C# files
    "r:.*\\.(png|jpg)$")  // regex re-includes images
```

## Run

```bash
dotnet run --project samples/HybridMatcher/HybridMatcher.csproj
```

Expected: **6 files** — the 4 `.cs` files plus the 2 images under `assets/`.

## Why it works

The engine compiles each pattern into its dialect-specific matcher and resolves them with
deterministic precedence: GitIgnore → Glob → Regex. A GitIgnore inclusion is final; Glob and Regex
can re-include paths excluded by GitIgnore but cannot exclude a GitIgnore inclusion.

## Related

- [Pattern precedence](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-precedence.md)
- [Pattern overview](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-overview.md)
- [All samples](../README.md)
