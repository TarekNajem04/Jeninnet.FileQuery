# DirectoryOnlyMatching Sample

Match a **directory itself**, not just the files inside it. In GitIgnore syntax a trailing `/`
marks a directory-only rule:

```csharp
.Where("**", "!logs/")
```

**What it teaches:** directory-only rules, exclusion (`**`), and re-inclusion of a directory.

## Run

```bash
dotnet run --project samples/DirectoryOnlyMatching/DirectoryOnlyMatching.csproj
```

Expected: the two files under `logs/` — `logs/app.log` and `logs/error.log` — because re-including
the directory restores access to its contents.

## Why it matters

Directory-only patterns are how real ignore files prune whole subtrees (`bin/`, `obj/`, `node_modules/`)
without matching files that merely contain the same name. The traversal uses the same rule to decide
whether to descend.

## Related

- [Directory results](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/directory-results.md)
- [GitIgnore patterns](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/gitignore-patterns.md)
- [All samples](../README.md)
