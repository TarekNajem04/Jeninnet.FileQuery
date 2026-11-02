# Jeninnet.FileQuery.Tool.Tests

End-to-end tests for the `dotnet-filequery` global tool: command-line parsing, output modes, exit
codes, diff-aware Git queries, and the `--explain`/`--json` surfaces.

**55 tests.**

## What is covered

| Area | Examples |
|---|---|
| Option parsing | `--root`, `--dialect`, `--no-recursive`, `--depth`, `--ignore-case` |
| Output modes | absolute paths, `--relative`, `--null`, `--count`, `--json`, `--explain` |
| Conflict rules | incompatible combinations return exit code `2` |
| Execution failures | missing root, malformed pattern, IO failure return exit code `1` |
| Git-aware queries | `--changed-since`, `--changed-since-date`, `--no-untracked` |
| Pattern composition | CLI options combined with positional patterns |

The Git tests create temporary repositories through `TemporaryGitRepository`, so they do not touch
the developer's repository or require a remote.

## Run

```bash
dotnet test tests/Jeninnet.FileQuery.Tool.Tests/Jeninnet.FileQuery.Tool.Tests.csproj -c Release

# Git-only
dotnet test tests/Jeninnet.FileQuery.Tool.Tests/Jeninnet.FileQuery.Tool.Tests.csproj -c Release \
  --filter "FullyQualifiedName~Git"
```

## Design notes

- The tool is invoked through its real command-line entry point, not mocked.
- Tests assert both exit codes and exact stdout shape.
- Temporary repositories use deterministic file names and local commits.
- No network access is required.

## Related

- [Global tool guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/global-tool.md)
- [Tool package](../../src/Jeninnet.FileQuery.Tool/README.md)
- [All tests](../README.md)
