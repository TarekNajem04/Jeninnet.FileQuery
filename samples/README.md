# Samples

Runnable, self-contained examples for `Jeninnet.FileQuery`. Every sample creates a deterministic
temporary tree, prints the query and expected result, then cleans up.

Start with **BasicMatching**, then pick the concept you need.

| Sample | Concept | Pattern |
|---|---|---|
| [BasicMatching](./BasicMatching/README.md) | Hello world: no patterns, every file | — |
| [PatternLanguage](./PatternLanguage/README.md) | Ordered GitIgnore rules and re-exclusion | `**`, `!**/*.cs`, `**/cli/**` |
| [NegationAndRecursive](./NegationAndRecursive/README.md) | Recursive negation | `**`, `!**/*.log` |
| [RecursiveTraversal](./RecursiveTraversal/README.md) | Re-include a subtree | `!src/**/*.cs` |
| [NestedGlobMatching](./NestedGlobMatching/README.md) | Multiple recursive Glob patterns | `**/*.cs`, `**/*.md` |
| [DirectoryOnlyMatching](./DirectoryOnlyMatching/README.md) | Directory-only rules with a trailing `/` | `!logs/` |
| [HybridMatcher](./HybridMatcher/README.md) | GitIgnore + Regex in one query | `**`, `!**/*.cs`, `r:...` |
| [RegexMatching](./RegexMatching/README.md) | Unanchored Regex | `r:.*\.log$` |
| [RegexAnchoredMatching](./RegexAnchoredMatching/README.md) | Root-anchored Regex | `r:^src/.*\.cs$` |
| [POSIXClassMatching](./POSIXClassMatching/README.md) | POSIX named classes | `**/*[[:digit:]].*` |
| [EntryMetadata](./EntryMetadata/README.md) | Metadata results and filters | `ExecuteEntries` + size/time |
| [LogFiltering](./LogFiltering/README.md) | Async streaming and cancellation | `ExecuteAsync` |
| [AdvancedUsage](./AdvancedUsage/README.md) | Generic Host + DI + CommandLine | full application |

## Run any sample

```bash
dotnet run --project samples/<SampleName>/<SampleName>.csproj
```

Samples share the deterministic fixture helpers in
[SamplesUtilities](./SamplesUtilities/README.md), so expected result counts are stable across
operating systems.

## Prerequisites

- .NET 10 SDK
- No external services, databases, or network access

## Related

- [Root README](../README.md)
- [Quick start](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/getting-started/quick-start.md)
- [Pattern reference](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/pattern-overview.md)
