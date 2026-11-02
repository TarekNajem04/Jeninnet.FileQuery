# NegationAndRecursive Sample

Exclude everything, then re-include every log file at any depth.

**What it teaches:** GitIgnore negation (`!`) combined with the recursive wildcard (`**`).

```csharp
.Where("**", "!**/*.log")
```

## Run

```bash
dotnet run --project samples/NegationAndRecursive/NegationAndRecursive.csproj
```

Expected: **3 files** — `app.log`, `logs/app.log`, and `logs/error.log`.

## Why it works

`**` excludes all files. The negated pattern `!**/*.log` re-includes matching files in the root and
in every nested directory. Negation is not a separate pass: the compiled GitIgnore matcher applies
last-match-wins while the tree is walked.

## Related

- [GitIgnore patterns](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/gitignore-patterns.md)
- [Pattern precedence](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-precedence.md)
- [All samples](../README.md)
