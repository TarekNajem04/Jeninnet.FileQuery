# Pattern Precedence

Understanding precedence is essential for writing correct rule sets. This page explains exactly how the engine evaluates patterns and resolves conflicts.

---

## The Evaluation Model

There is no precedence hierarchy between pattern types. There is no concept of "GitIgnore rules override Glob rules" or "specific patterns override general ones". There is only **sequential evaluation with last-rule-wins**.

```
Rule 1 evaluated → current state updated
Rule 2 evaluated → current state updated
Rule 3 evaluated → current state updated
...
Final state → file included or excluded
```

The current state after all rules have been evaluated determines whether the file appears in the results.

---

## Worked Examples

### Example 1 — Simple last-rule-wins

```
**           → state: Exclude
!*.cs        → state: Include  (for .cs files)
src/gen/**   → state: Exclude  (for files under src/gen/)
```

A file `src/gen/Generated.cs`:
- Rule 1 matches → Exclude
- Rule 2 matches (`*.cs`) → Include
- Rule 3 matches (`src/gen/**`) → Exclude

Final state: **Excluded**.

---

### Example 2 — Re-inclusion after deep exclusion

```
**              → Exclude
src/gen/**      → Exclude (no change — already excluded)
!src/gen/keep/  → Include (directory-only inclusion — re-opens traversal)
```

A file `src/gen/keep/Registry.cs`:
- Rule 1 → Exclude
- Rule 2 → Exclude (no change)
- Rule 3 matches the parent directory `src/gen/keep/` → Include

Final state: **Included**.

---

### Example 3 — Multiple pattern types in Hybrid mode

```
**                      → GitIgnore: Exclude
!**/*.cs                → GitIgnore: Include (.cs files)
r:^src/.*\.generated\.cs$  → Regex: Exclude (generated .cs files)
!src/Main.generated.cs  → GitIgnore: Include (specific override)
```

A file `src/Main.generated.cs`:
- Rule 1 → Exclude
- Rule 2 → Include
- Rule 3 matches the regex → Exclude
- Rule 4 matches → Include

Final state: **Included**.

In Hybrid mode, GitIgnore and Regex patterns share the same ordered evaluation list. There is no separation by type at evaluation time.

---

## Directory Pruning and File Re-inclusion

Directory-only patterns affect traversal, not just inclusion state. When a directory is pruned, the engine does not enumerate files inside it — unless a subsequent negated pattern causes re-inclusion of the directory.

```
bin/            ← prune bin/ (traversal stops)
!bin/release/   ← re-open bin/release/ (traversal resumes)
```

The engine evaluates directory patterns against directory paths as they are encountered during traversal. A directory is pruned when its most recent match is a non-negated directory-only pattern.

---

## GitIgnore Sub-set, Glob Sub-set, Regex Sub-set

Internally, `HybridPathMatcher` evaluates three sub-sets in order:

1. GitIgnore patterns
2. Glob patterns
3. Regex patterns

Within each sub-set, patterns are evaluated in the order they were declared. The outcome of each sub-set can be overridden by the next.

This means that in practice, Regex patterns act as the final override layer, followed by Glob, followed by GitIgnore at the base.

If you want a GitIgnore rule to override a Regex rule, declare the GitIgnore rule after the Regex rule in the input list — it will be included in a later evaluation position within the GitIgnore sub-set.

---

## Summary Rules

1. All patterns are evaluated in declaration order.
2. The last matching pattern determines the outcome.
3. Negated patterns (`!`) restore the Include state.
4. Non-negated patterns set the Exclude state (or re-exclude if already excluded).
5. Directory-only patterns (`/`) affect traversal AND state.
6. In Hybrid mode, the GitIgnore → Glob → Regex sub-set order determines which sub-set can override which, but within each sub-set, declaration order still governs.