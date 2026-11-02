# RegexAnchoredMatching Sample

Anchor a Regex pattern to the query root so only a specific subtree matches.

**What it teaches:** the `r:` dialect prefix and `^` anchoring in Regex mode.

```csharp
.UsingRegex()
.Where("r:^src/.*\\.cs$")
```

## Run

```bash
dotnet run --project samples/RegexAnchoredMatching/RegexAnchoredMatching.csproj
```

Expected: **4 C# files** under `src/` — and nothing outside it.

## Why the anchor matters

Regex patterns match the **full normalized relative path**. `^src/` requires the path to begin at
the query root; without the anchor, a pattern can match at any position and include lookalike
directories such as `other/src/`.

## Related

- [Regex reference](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/regex-patterns.md)
- [All samples](../README.md)
