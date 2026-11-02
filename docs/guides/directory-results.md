# Directory Results

Queries return files only by default. `IncludeDirectories()` opts into yielding matching
directories from the same traversal, without a second walk.

---

## Library usage

```csharp
var entries = FileQuery.From(root)
    .UsingGitIgnore()
    .Where(PatternKind.GitIgnore, ["**", "!bin/"])
    .IncludeDirectories()
    .ExecuteEntries();

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.RelativePath} (directory: {entry.IsDirectory})");
}
```

`Execute()` also returns directory full paths when the option is enabled. The default behavior is
unchanged: omit `IncludeDirectories()` and only files are returned.

---

## Directory-only patterns and negation

Directory results use the same match outcome that decides whether traversal descends, so the
existing semantics apply directly:

- `bin/` matches the directory `bin` and excludes its subtree.
- `!keep/` re-includes the `keep` directory; files below it are restored by the existing
  directory-inclusion override.
- Last-match-wins applies across parent and nested `.gitignore` rules.
- A directory excluded by a rule is not yielded, even when the traversal continues because a
  negated rule could re-include content below it.
- `IncludeDirectories(false)` restores the file-only behavior explicitly.

For example, `.Where(PatternKind.GitIgnore, ["**", "!keep/"]).IncludeDirectories()` returns only
`keep` and the files below it; other directories are pruned.

---

## Pruning and filters

- Directory pruning is unchanged: an excluded directory is not traversed unless a negated rule
  could match content below it.
- `AttributesToSkip` applies to directories and prunes matching subtrees.
- Size and modification-time filters apply to files only; directory entries are not rejected
  because a directory's own size/timestamp does not describe its contents.
- `RestrictToPaths` still restricts traversal; a directory is yielded only when it is an ancestor
  of an allowed path.
