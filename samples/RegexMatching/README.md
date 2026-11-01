# RegexMatching Sample

Use an unanchored Regex to find every matching file at any depth.

**What it teaches:** Regex mode, full-path matching, and the difference from an anchored pattern.

```csharp
.UsingRegex()
.Where("r:.*\\.log$")
```

## Run

```bash
dotnet run --project samples/RegexMatching/RegexMatching.csproj
```

Expected: **3 log files** — `app.log`, `logs/app.log`, and `logs/error.log`.

## Anchored vs unanchored

`r:.*\\.log$` matches at any position, so it finds logs in every directory. `r:^logs/.*\\.log$`
would restrict the search to the `logs/` subtree. See
[RegexAnchoredMatching](../RegexAnchoredMatching/README.md) for the anchored form.

## Related

- [Regex reference](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/regex-patterns.md)
- [All samples](../README.md)
