# Global Tool

`dotnet-filequery` is a .NET global tool that exposes the Jeninnet.FileQuery engine in the
terminal. It reads patterns, walks a directory tree, and writes matching paths to standard
output so they can be piped into other commands.

The tool is a thin shell over the library: pattern classification, compilation, and traversal all
come from [Jeninnet.FileQuery](../introduction/overview.md).

---

## Installation

```bash
dotnet tool install --global Jeninnet.FileQuery.Tool
```

Verify the installation:

```bash
dotnet-filequery --version
dotnet-filequery --help
```

---

## Basic usage

```bash
dotnet-filequery "**;!*.cs"
```

Patterns follow the engine's GitIgnore-style rule order: each rule ignores matching paths, and a
leading `!` re-includes them. Starting with `**` ignores everything, then `!*.cs` re-includes the
C# files. When no pattern is supplied, every file is returned.

---

## Pattern sources

| Option | Dialect | Notes |
|---|---|---|
| `<patterns>` | Hybrid | Positional; classified automatically. |
| `--patterns` / `-p` | Hybrid | Same behaviour as the positional argument. |
| `--gitignore` | GitIgnore | Applied in order. |
| `--glob` | Glob | Applied in order. |
| `--regex` | Regex | The `r:` prefix is optional. |

Multiple patterns are separated with `;`:

```bash
dotnet-filequery --glob "**/*.md;**/*.yml"
```

GitIgnore patterns are ignore rules: a matching rule excludes the path, and a leading `!`
re-includes it. Glob and Regex patterns are inclusion rules: only matching files are returned.
Pick one dialect when a query is meant to be a simple filter, or rely on Hybrid precedence when
mixing them.

A forced dialect applies to every pattern and cannot be combined with the typed options:

```bash
dotnet-filequery --dialect regex "\.Tests\.cs$"
dotnet-filequery --dialect glob "**/*.json"
```

---

## Traversal options

| Option | Description |
|---|---|
| `--root` / `-r` | Root directory; defaults to the current directory. |
| `--no-recursive` | Search only the root directory. |
| `--depth <n>` | Maximum depth; `0` searches the root only. |
| `--ignore-case` | Case-insensitive matching. |
| `--symlinks <policy>` | `ignore` (default), `follow`, or `follow-with-cycle-detection`. |
| `--changed-since <revision>` | Only files changed since a commit, tag, or branch. |
| `--changed-since-date <timestamp>` | Only files changed since an ISO 8601 timestamp. |
| `--no-untracked` | Exclude untracked files from a change-set query. |
| `--explain` | Write per-file include/exclude reasons instead of matching paths. |
| `--ignore-files` | Load root/nested `.gitignore`, `.git/info/exclude`, and optional global rules. |
| `--global-ignore <path>` | Additional global ignore file; requires `--ignore-files`. |
| `--json` | Write one JSON object per line with path and metadata; combines with `--explain` and diff-aware queries. |

`--no-recursive` and `--depth` are mutually exclusive.

### Git ignore files

```bash
dotnet-filequery --root . --ignore-files
dotnet-filequery --root . --ignore-files --global-ignore ~/.config/git/ignore "**/*.cs"
```

Loaded ignore rules come before positional or `--patterns` rules, so explicit patterns win. When
`--ignore-files` is used without patterns, the tool omits its implicit include-all fallback so the
ignore files can exclude files. See [Git Ignore Files](gitignore-files.md) for precedence details.

### JSON output

```bash
dotnet-filequery --root . --json "**/*.cs"
dotnet-filequery --root . --json --explain "**;!*.cs"
```

`--json` writes newline-delimited objects containing absolute and relative paths, size,
timestamps, attributes, and directory state. It cannot be combined with `--null` or `--count`, and
`--relative` is unnecessary because both path forms are present. See [JSON Output](json-output.md)
for the schema.

---

## Output and pipelines

By default each match is written as an absolute path on its own line:

```bash
dotnet-filequery "**;!*.cs" | while read -r file; do echo "$file"; done
```

Use `--relative` when you want paths relative to the root:

```bash
dotnet-filequery --relative --depth 1 "**;!*.json"
```

Use `--null` with tools that support NUL-separated input, which is safe for paths containing
spaces or newlines:

```bash
dotnet-filequery "**;!*.log" --null | xargs -0 wc -l
```

Use `--count` to print only the number of matches:

```bash
dotnet-filequery --dialect regex "\.Tests\.cs$" --count
```

Use `--explain` to see why every file was included or excluded. The output is tab-separated:

```text
include	src/Engine/FileQueryEngine.cs	Include by GitIgnore pattern.
exclude	bin/Debug/app.dll	Exclude by GitIgnore pattern.
```

`--explain` cannot be combined with `--count` or `--null`. The VS Code extension uses this mode
to render its include/exclude view.

---

## Exit codes

| Code | Meaning |
|---|---|
| `0` | The query completed, whether or not anything matched. |
| `1` | Execution failed (missing root, malformed pattern, IO error). |
| `2` | The command line was invalid. |

Errors are written to standard error, so they never contaminate piped output.

---

## Diff-aware queries

`--changed-since` and `--changed-since-date` restrict the query to the files Git reports between
the baseline and the working tree. The root may be a repository subdirectory; change paths are
rebased automatically, and deleted files are never returned.

```bash
dotnet-filequery --changed-since HEAD~5
dotnet-filequery --changed-since origin/main "**;!*.cs" --count
dotnet-filequery --changed-since-date 2026-10-01T00:00:00Z --no-untracked
```

See [Diff-Aware Queries](diff-aware-queries.md) for the full semantics.

---

## Examples

```bash
# All Markdown files under the documentation folder
dotnet-filequery --root docs --glob "**/*.md"

# Every C# file except tests, relative to the repository root
dotnet-filequery --root . --regex "\.cs$" --relative

# Files changed in the last level of the tree
dotnet-filequery --depth 1

# Ignore build output and dependency folders
dotnet-filequery --gitignore "bin/;obj/;node_modules/"
```

---

## See also

- [Command-Line Integration](cli-usage.md) — embedding the pattern options in your own CLI.
- The `Jeninnet.FileQuery.Tool` package README documents the same options for NuGet consumers.
