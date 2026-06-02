# Basic Patterns

This page introduces the Jeninnet.FileQuery pattern language through practical examples. For the complete syntax specification, see [Pattern Language Reference](../specification/pattern-language.md).

---

## The Golden Rule

**Patterns are evaluated sequentially. The last matching rule wins.**

```
**          ← Rule 1: exclude everything
!*.log      ← Rule 2: include all .log files
data.log    ← Rule 3: exclude data.log specifically
```

`data.log` is excluded because Rule 3 is the last rule that matches it.
Every other `.log` file is included because Rule 2 matches them and no later rule overrides that.
Every non-`.log` file is excluded because only Rule 1 matches them.

---

## Wildcards

### `*` — Match within one segment

`*` matches any sequence of characters within a single directory segment. It does not cross directory boundaries.

```
*.txt           → readme.txt, notes.txt
src/*.cs        → src/Program.cs  (not src/utils/Helper.cs)
file?.log       → file1.log, fileA.log  (not file10.log)
```

### `**` — Match across segments

`**` matches zero or more complete path segments, crossing directory boundaries.

```
**/*.cs         → any .cs file at any depth
src/**/*.cs     → any .cs file anywhere under src/
**/bin/**       → anything inside any bin/ directory
```

### `?` — Match exactly one character

```
file?.txt       → file1.txt, fileA.txt  (not file10.txt)
report.?.md     → report.1.md, report.A.md
```

---

## Negation with `!`

A `!` prefix negates the pattern — it re-includes files that a previous rule excluded.

```
**          ← exclude everything
!*.txt      ← include all .txt files
!src/**     ← include everything under src/
```

A negated pattern can only re-include files that were excluded by an earlier rule. A file that was never excluded cannot be "re-included" — it is simply always included.

---

## Root Anchoring with `/`

A leading `/` anchors the pattern to the query root. Without it, the pattern matches at any depth.

```
# Unanchored — matches readme.txt anywhere in the tree
!readme.txt

# Anchored — matches only the readme.txt at the root
!/readme.txt
```

```
# Unanchored — matches any file named config.json
!config.json

# Anchored — matches only /config.json at the root level
!/config.json
```

---

## Directory-Only Patterns

A trailing `/` makes the pattern match directories only. It is the primary mechanism for preventing the traversal engine from descending into certain directories.

```
bin/        ← exclude the bin/ directory and its entire subtree
obj/        ← exclude the obj/ directory and its entire subtree
.git/       ← exclude .git/ directories
```

Combined with negation for selective traversal:

```
**          ← exclude everything
bin/        ← do not traverse bin/
!src/**     ← include files under src/ (traversal allowed because bin/ was stopped, not src/)
```

---

## Character Classes

### Explicit sets and ranges

```
file[abc].txt       → filea.txt, fileb.txt, filec.txt
file[0-9].txt       → file0.txt through file9.txt
[A-Z]*.cs           → any .cs file starting with an uppercase letter
```

### Negated classes

A `!` or `^` immediately after the opening `[` negates the set:

```
file[!0-9].txt      → fileA.txt, fileB.txt (not file1.txt)
[^aeiou]*.md        → .md files not starting with a vowel
```

### POSIX named classes

```
file[[:digit:]].log         → file0.log through file9.log
[[:alpha:]][[:digit:]].txt  → a1.txt, b9.txt, Z0.txt
```

Supported POSIX classes: `digit`, `alpha`, `alnum`, `space`, `blank`, `upper`, `lower`, `print`, `graph`, `punct`, `cntrl`, `xdigit`.

---

## Regular Expression Patterns

Prefix a pattern with `r:` to use full .NET regular expression syntax:

```
r:^src/.*\.cs$          → any .cs file directly under src/
r:^data_\d{4}\.log$     → data_2024.log, data_2025.log
r:^(?!.*\.Test).*\.dll$ → any DLL not containing ".Test"
```

Regular expression patterns match the full normalized path using forward slashes as separators.

---

## Common Recipes

### Collect source files, exclude build output

```
**              exclude everything
!src/**/*.cs    include C# source
bin/            stop traversal into bin/
obj/            stop traversal into obj/
```

### Collect all text files except large ones

```
**              exclude everything
!**/*.txt       include all .txt files
r:.*_large\.txt exclude files ending in _large.txt
```

### Collect files matching a versioned naming convention

```
**
!r:^releases/v\d+\.\d+\.\d+/.*
```

### Collect everything except hidden files and directories

```
.**             exclude dotfiles
.*/             exclude dot-directories
!**             include everything else
```

---

## What Patterns Cannot Do

- Match file content — patterns operate on paths only
- Match file metadata (size, date, attributes)
- Perform arithmetic or conditional logic
- Reference environment variables

For content-based filtering, apply your own predicate to the results returned by the engine.