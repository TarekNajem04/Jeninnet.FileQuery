# Diff-Aware Queries

Incremental tools should process only what changed. A diff-aware query takes the files reported
by a version-control diff and restricts the engine to that set: every other file is skipped before
pattern matching, and directories that cannot contain a changed file are pruned.

The engine itself is version-control agnostic. `FileQueryBuilder.RestrictToPaths` accepts any
list of root-relative paths; the `dotnet-filequery` tool fills that list from Git.

---

## Library API

```csharp
using Jeninnet.FileQuery;

// Paths are relative to the query root and use forward slashes.
string[] changed =
[
    "src/Jeninnet.FileQuery/Engine/FileQueryEngine.cs",
    "docs/guides/diff-aware-queries.md",
];

var files = FileQuery.From(repositoryRoot)
    .RestrictToPaths(changed)
    .Where("!**")                  // include every listed file
    .Execute();

foreach (var file in files)
{
    Console.WriteLine(file);
}
```

Rules:

- Paths are file paths relative to the query root. Absolute paths, `..` segments, and empty
  entries are rejected when the query is built.
- `\` separators and `.` segments are normalized, so paths produced on Windows work unchanged.
- An empty list matches no files. Do not call `RestrictToPaths` when the query should be
  unrestricted.
- The allow-list is combined with the pattern set: a listed file must also satisfy the patterns.
- Directory pruning is automatic; a query never walks a subtree that cannot contain a listed file.
- Case handling follows the query's case-sensitivity setting (`IgnoreCase()`).

Because the allow-list is a plain collection, you can feed it from any source: Git, Mercurial,
a build system's incremental graph, or a cache manifest.

---

## Command-line tool

`dotnet-filequery` reads the change set from Git and passes it to `RestrictToPaths`:

```bash
# Files changed since a commit, branch, or tag (includes untracked files)
dotnet-filequery --changed-since HEAD~5

# Files changed since a timestamp (uses the last commit before it)
dotnet-filequery --changed-since-date 2026-10-01T00:00:00Z

# Ignore untracked files
dotnet-filequery --changed-since origin/main --no-untracked

# Combine with patterns and count the result
dotnet-filequery --changed-since HEAD --count "**;!*.cs"
```

| Option | Description |
|---|---|
| `--changed-since <revision>` | Baseline commit, tag, or branch. The working tree (staged and unstaged) is compared against it. |
| `--changed-since-date <timestamp>` | ISO 8601 timestamp; the last commit before it becomes the baseline. |
| `--no-untracked` | Do not report untracked files as added. |

The two baselines are mutually exclusive. When neither is supplied, the query is not
diff-aware.

### What is included

| Git status | Reported as | Returned by the query |
|---|---|---|
| Added | Added | Yes |
| Modified | Modified | Yes |
| Renamed | Renamed (with the previous path) | Yes, at the new path |
| Copied | Copied | Yes |
| Type changed | Type changed | Yes |
| Deleted | Deleted | No — the path no longer exists |

Untracked files are reported as added unless `--no-untracked` is supplied.

### Repository layout

The query root may be the repository root or any directory inside it. Change paths are rebased
onto the root, so `--root ./src` only returns changed files under `src/`. A root outside the
repository is rejected with an execution error.

### Failure modes

| Situation | Exit code |
|---|---|
| Git is not installed | `1` |
| The root is not inside a Git repository | `1` |
| The revision or timestamp baseline cannot be resolved | `1` |
| Both baselines are supplied | `2` |
| The timestamp is not a valid ISO 8601 value | `2` |

---

## See also

- [Global Tool](global-tool.md) — the full command-line reference.
- [Pattern precedence](../reference/pattern-precedence.md) — how the allow-list interacts with
  pattern precedence.
