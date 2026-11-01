# Jeninnet.FileQuery.Tool

`dotnet-filequery` is a terminal file discovery tool built on the
[Jeninnet.FileQuery](../Jeninnet.FileQuery/README.md) engine. It matches GitIgnore, Glob, and
Regex patterns and streams the results to standard output so they compose with the rest of your
shell tooling.

## Installation

```bash
dotnet tool install --global Jeninnet.FileQuery.Tool
```

Update an existing installation:

```bash
dotnet tool update --global Jeninnet.FileQuery.Tool
```

## Usage

```text
dotnet-filequery [<patterns>...] [options]
```

When no pattern is supplied, every file under the root is returned. Patterns use the engine's
GitIgnore-style rule order: a rule ignores matching paths, and a leading `!` re-includes them.
Start with `**` to ignore everything and re-include what you want:

```bash
# Every C# file in the current directory tree
dotnet-filequery "**;!*.cs"

# Markdown files under ./docs
dotnet-filequery --root ./docs --glob "**/*.md"

# Regex dialect, only the count
dotnet-filequery --dialect regex "\.Tests\.cs$" --count

# Paths relative to the root, one level deep
dotnet-filequery --relative --depth 1 "**;!*.json"

# NUL-separated output for xargs
dotnet-filequery "**;!*.log" --null | xargs -0 wc -l
```

## Options

| Option | Default | Description |
|---|---|---|
| `<patterns>` | every file | Positional patterns; each value may hold multiple `;`-separated patterns. |
| `-p`, `--patterns <value>` | | Same as the positional argument, for scripts that prefer an option. |
| `--gitignore <value>` | | GitIgnore-style patterns applied in order. |
| `--glob <value>` | | Glob-style patterns applied in order. |
| `--regex <value>` | | Regular-expression patterns; the `r:` prefix is optional. |
| `-r`, `--root <path>` | current directory | Root directory to search. |
| `--dialect <name>` | `hybrid` | Force one dialect: `hybrid`, `gitignore`, `glob`, or `regex`. |
| `--no-recursive` | off | Search only the root directory. |
| `--depth <n>` | unlimited | Maximum recursion depth (`0` searches the root only). |
| `--ignore-case` | platform default | Match without case sensitivity. |
| `--symlinks <policy>` | `ignore` | `ignore`, `follow`, or `follow-with-cycle-detection`. |
| `--relative` | off | Emit paths relative to the root. |
| `-0`, `--null` | off | Separate paths with a NUL character instead of a newline. |
| `--count` | off | Write only the number of matching files. |

Patterns supplied through `--gitignore`, `--glob`, or `--regex` are typed explicitly and cannot be
combined with a forced `--dialect`.

## Output and exit codes

- Paths are absolute by default, one per line, in deterministic traversal order.
- `--relative` makes them relative to the root; `--null` separates them with `\0`.
- `--count` writes a single number.
- Exit code `0` means the query completed (even when nothing matched), `1` means execution failed,
  and `2` means the command line was invalid.
- Usage and error messages go to standard error, so piping standard output stays clean.

## Related packages

- [Jeninnet.FileQuery](../Jeninnet.FileQuery/README.md) — the matching engine.
- [Jeninnet.FileQuery.CommandLine](../Jeninnet.FileQuery.CommandLine/README.md) — the
  `System.CommandLine` integration used by this tool.

## License

MIT — see [`LICENSE`](../../LICENSE).
