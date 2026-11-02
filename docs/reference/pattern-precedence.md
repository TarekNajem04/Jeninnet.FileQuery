# Pattern Precedence

Understanding precedence is essential for writing correct rule sets. This page explains the
two levels at which the engine resolves conflicts.

---

## Two-Level Evaluation Model

### 1. Within a dialect: last matching rule wins

Each dialect keeps its rules in declaration order. The last rule in that dialect that matches the
path determines the dialect's result. GitIgnore negation (`!`) is part of this rule order.

### 2. Across dialects: GitIgnore → Glob → Regex, first Include wins

The compiled pattern set has three subsets. The engine evaluates them in this fixed order:
**GitIgnore → Glob → Regex**.

- If the GitIgnore subset returns **Include**, evaluation stops and the path is included.
- Otherwise the Glob subset is evaluated. If it returns **Include**, evaluation stops and the
  path is included.
- Otherwise the Regex subset is evaluated and its result is returned.
- If a subset has no matching rule, it does not override an earlier Exclude.
- If the pattern set is empty, the path is **included**; an empty path is **excluded**.

This is not the same as "the last pattern in the input list wins". A later dialect can override
an earlier **Exclude**, but it cannot override an earlier **Include**.

| GitIgnore result | Glob result | Regex result | Final result |
|---|---|---|---|
| Include | — | — | **Include** |
| Exclude | Include | — | **Include** |
| Exclude | Exclude | Include | **Include** |
| Exclude | Exclude | Exclude | **Exclude** |
| Exclude | Exclude | no match | **Exclude** |
| no match | no match | no match | **Include** |

---

## Worked Examples

### Example 1 — GitIgnore-only last-rule-wins

```text
**
!*.cs
src/gen/**
```

A file `src/gen/Generated.cs`:

1. `**` → Exclude
2. `!*.cs` → Include
3. `src/gen/**` → Exclude

Final result: **Excluded**.

This is normal GitIgnore ordering: all three rules live in the GitIgnore subset.

### Example 2 — Directory pruning and re-inclusion

```text
bin/
!bin/release/
```

`bin/` prunes the directory during traversal. `!bin/release/` re-opens that subtree. Files under
`bin/release/` can then be matched by the remaining rules. The directory decision uses the same
last-match rule inside the GitIgnore subset.

### Example 3 — Mixed dialects

```text
**
!**/*.cs
r:^src/.*\.generated\.cs$
```

A file `src/Main.generated.cs`:

1. GitIgnore `**` → Exclude
2. GitIgnore `!**/*.cs` → Include (for `.cs` files)
3. The GitIgnore subset therefore returns **Include**, so evaluation stops.

Final result: **Included**. The Regex rule cannot exclude a path that GitIgnore already included.

To exclude generated C# files, put that rule in the GitIgnore subset, for example:

```text
**
!**/*.cs
src/**/*.generated.cs
```

### Example 4 — Regex as a fallback Include

```text
**
r:^src/.*\.cs$
```

1. GitIgnore `**` → Exclude
2. Glob has no rules → no override
3. Regex matches `src/Program.cs` → Include

Final result for `src/Program.cs`: **Included**. The Regex subset acts as a fallback include,
not as a final override of an earlier include.

---

## Directory Pruning and File Re-inclusion

Directory-only patterns affect traversal as well as inclusion state. When a directory is pruned,
the engine does not enumerate its files unless a later rule can re-include something below it.

```text
bin/            ← prune bin/ (traversal stops)
!bin/release/   ← re-open bin/release/ (traversal resumes)
```

GitIgnore negation uses the same last-match rule as file matching. Glob and Regex re-inclusion is
also honored across dialects: when an excluded directory could contain a path matched by a Glob
rule, the engine traverses it. Regex rules are conservatively treated as potentially matching
anywhere below an excluded directory. Files that no later rule matches remain excluded.

---

## Summary Rules

1. Within each dialect, rules are evaluated in declaration order and the last matching rule wins.
2. Across dialects, the order is GitIgnore → Glob → Regex.
3. The first subset that returns **Include** wins; later subsets are not consulted.
4. A later subset can override an earlier **Exclude**, but not an earlier **Include**.
5. Negated patterns (`!`) restore the Include state within the GitIgnore subset.
6. Directory-only patterns (`/`) affect traversal and state within the GitIgnore subset.
7. An empty pattern set includes everything; an empty path is excluded.
