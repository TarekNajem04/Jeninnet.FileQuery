# PatternLanguage Sample

See how ordered GitIgnore rules combine exclusion, negation, and re-exclusion.

**What it teaches:** last-match-wins evaluation and the difference between a positive rule and a
negated re-inclusion.

```csharp
.Where(
    "**",          // exclude every file
    "!**/*.cs",    // re-include every C# file
    "**/cli/**")   // exclude the cli subtree again
```

## Run

```bash
dotnet run --project samples/PatternLanguage/PatternLanguage.csproj
```

Expected: **3 files** outside `cli/`:

- `src/FileQuery.cs`
- `src/Program.cs`
- `src/test/helpers.cs`

## Why the order matters

GitIgnore semantics are order-sensitive. Each later matching rule changes the outcome of the
previous one. The engine keeps the same model at compile time, so the result is deterministic rather
than dependent on filesystem enumeration order.

## Related

- [Pattern precedence](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-precedence.md)
- [Pattern semantics](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-semantics.md)
- [All samples](../README.md)
