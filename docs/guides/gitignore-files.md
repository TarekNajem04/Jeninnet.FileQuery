# Git Ignore Files

`UseGitIgnoreFiles()` loads the ignore files Git would use for a query root:

- the root `.gitignore`;
- nested `.gitignore` files (parent before child);
- `.git/info/exclude` when the query root is inside a Git working tree;
- an optional global excludes file.

Rules are converted into root-relative GitIgnore patterns and evaluated by the normal matching
pipeline. No Git executable is required.

---

## Library usage

```csharp
var query = FileQuery.From(repositoryRoot)
    .UseGitIgnoreFiles()
    .Build();

foreach (var file in FileQueryRuntime.Create().Execute(query))
{
    Console.WriteLine(file);
}
```

Optional settings:

```csharp
var options = new GitIgnoreOptions
{
    IncludeNestedFiles = true,
    IncludeInfoExclude = true,
    GlobalExcludesFile = Path.Combine(home, ".config", "git", "ignore")
};

var query = FileQuery.From(root).UseGitIgnoreFiles(options);
```

Relative global-ignore paths are resolved against the query root. The loader does not read Git
configuration; pass the resolved `core.excludesFile` value explicitly when you need the user's
global ignore file.

---

## Precedence

Precedence follows Git's last-match-wins model:

1. global excludes;
2. `.git/info/exclude`;
3. root `.gitignore`;
4. nested `.gitignore` files, parents before children;
5. the query's own explicit patterns.

Because the query's patterns come last, an explicit `Where(...)` rule can override an ignore file.
A rule in a deeper ignore file overrides a parent rule. Directory-only rules (`build/`) prune the
subtree, and `!` negation re-includes entries when a later rule matches.

A nested ignore file is scoped to its directory. For example, `*.tmp` in `src/.gitignore` affects
`src/a.tmp` and `src/deep/b.tmp`, but not `other/c.tmp`.

---

## CLI opt-in

```bash
dotnet-filequery --root ./repo --ignore-files
dotnet-filequery --root ./repo --ignore-files --global-ignore ~/.config/git/ignore
```

`--global-ignore` requires `--ignore-files`. When no patterns are supplied, the CLI removes its
implicit include-all fallback so the loaded ignore rules can exclude files.

---

## Limits

- Ignore files are read when the query plan is built; changes made during enumeration are not
  observed.
- A query rooted inside a repository loads `.git/info/exclude` only where repository-relative
  rules can be rebased onto the query root. Rules outside the root are dropped; unanchored rules
  still apply below it.
- The engine's negation traversal is intentionally more permissive than Git for in-memory
  patterns such as `**` combined with `!file.txt`. File ingestion avoids reading nested ignore
  files inside an ignored directory, but the general matching semantics remain the engine's
  documented last-match-wins behavior.
- There is no `core.excludesFile` discovery; supply the path explicitly.
