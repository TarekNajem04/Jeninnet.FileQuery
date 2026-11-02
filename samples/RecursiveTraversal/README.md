# RecursiveTraversal Sample

Re-include an entire subtree with one recursive negation.

**What it teaches:** `!src/**/*.cs` — a recursive GitIgnore rule that walks every nested folder.

```csharp
.Where("**", "!src/**/*.cs")
```

## Run

```bash
dotnet run --project samples/RecursiveTraversal/RecursiveTraversal.csproj
```

Expected: **4 C# files** under `src/`, including `src/cli/Program.cs` and `src/test/helpers.cs`.

## Why it works

The recursive wildcard `**` spans zero or more directories. `!src/**/*.cs` therefore re-includes
matching files at any depth below `src/`, without a second traversal or a separate recursive
`Directory.GetFiles` call.

## Related

- [Glob reference](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/glob-patterns.md)
- [Traversal model](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/runtime/traversal-model.md)
- [All samples](../README.md)
