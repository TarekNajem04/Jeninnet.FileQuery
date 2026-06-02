# GitIgnore Patterns

GitIgnore is the default pattern dialect in Jeninnet.FileQuery. It is the most expressive dialect and the recommended choice for hierarchical filesystem rules.

---

## Syntax Reference

| Syntax | Description |
|--------|-------------|
| `**` | Recursive wildcard — matches zero or more path segments |
| `*` | Single-segment wildcard — matches within one directory level |
| `?` | Single-character wildcard |
| `!pattern` | Negation — re-includes files excluded by an earlier rule |
| `/pattern` | Root-anchored — matches only at the query root |
| `pattern/` | Directory-only — matches directories, not files |
| `[abc]` | Character set |
| `[a-z]` | Character range |
| `[!abc]` | Negated character set |
| `[[:digit:]]` | POSIX named class |
| `\*` | Escaped wildcard — literal asterisk |

---

## Unanchored vs Anchored Patterns

By default, a GitIgnore pattern is **unanchored** — it can match at any depth. This is implemented internally by prepending an implicit `**` segment to patterns that do not start with `/` or already begin with `**`.

```
# Unanchored: matches readme.txt anywhere in the tree
!readme.txt
# Equivalent to: !**/readme.txt

# Anchored: matches only the readme.txt at the root
!/readme.txt
```

Use root anchoring when you want to match a specific file or directory at the top level without accidentally matching identically named files deeper in the tree.

---

## Directory Pruning

A directory-only pattern (`pattern/`) does two things:

1. It matches the directory itself (excluding it from results).
2. It prevents the traversal engine from descending into that directory.

This is more efficient than a recursive exclusion like `bin/**`, because the traversal stops at the directory boundary rather than enumerating and discarding every file inside it.

```
bin/    ← traversal stops here; no files under bin/ are enumerated
obj/    ← traversal stops here
.git/   ← traversal stops here
```

### Re-including files inside a pruned directory

To include specific files inside a pruned directory, add a negated rule after the prune rule. The engine re-opens traversal into the directory when it sees a negated inclusion rule.

```
build/              ← prune build/
!build/release/**   ← but include everything under build/release/
```

---

## Last-Rule-Wins Semantics

Every pattern updates the inclusion state of the current path. When all patterns have been evaluated, the final state determines whether the file is returned.

```
**              state → Exclude (for all files)
!*.cs           state → Include (for .cs files)
src/tests/**    state → Exclude (for files under src/tests/)
!src/tests/*.cs state → Include (for .cs files directly in src/tests/)
```

A `.cs` file in `src/tests/` matches rules 1, 2, 3, and 4. The last match is rule 4, so it is included.
A `.cs` file in `src/tests/unit/` matches rules 1, 2, and 3. The last match is rule 3, so it is excluded.

---

## Escaping Special Characters

Prefix a special character with `\` to treat it as a literal:

```
\!important.txt     → matches the file literally named !important.txt
\#comment.md        → matches #comment.md
\[bracket\].txt     → matches [bracket].txt
```

Escaping is only recognized for `!`, `#`, `[`, `]`, `*`, `?`, and `\`.

---

## Character Classes in Detail

### Explicit sets

```
[abc]       matches a, b, or c
[!abc]      matches any character except a, b, c
[A-Z]       matches any uppercase ASCII letter
[0-9a-f]    matches any hex digit
```

### POSIX classes

POSIX classes are specified using `[:name:]` inside a bracket expression:

```
[[:digit:]]     matches 0–9
[[:alpha:]]     matches a–z, A–Z
[[:alnum:]]     matches 0–9, a–z, A–Z
[[:upper:]]     matches A–Z
[[:lower:]]     matches a–z
[[:space:]]     matches whitespace
[[:xdigit:]]    matches 0–9, a–f, A–F
```

Combining POSIX classes with literals:

```
[[:digit:]_]    matches any digit or underscore
[[:alpha:]0-9]  matches any letter or digit
```

---

## Common Mistakes

### Mistake: forgetting `**` before a directory-only exclusion

```
# Wrong: this only matches the directory at the root
obj/

# Correct: matches obj/ directories at any depth
**/obj/
```

### Mistake: negating without a prior exclusion

```
!*.txt      ← no effect — nothing to re-include because nothing was excluded yet
```

Always pair a negation with a prior exclusion:

```
**          ← exclude everything first
!*.txt      ← then re-include .txt files
```

### Mistake: using anchoring when unanchored matching is intended

```
/README.md  ← matches only the root README.md, not docs/README.md
README.md   ← matches README.md at any depth
```