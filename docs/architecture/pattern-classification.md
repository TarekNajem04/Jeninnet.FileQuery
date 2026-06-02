# Pattern Classification

Pattern classification is the process of determining which matcher dialect (`GitIgnore`, `Glob`, or `Regex`) should handle each pattern. It is performed by `PatternClassifier` in the namespace `Jeninnet.FileQuery.Patterns.Classification`.

---

## When Classification Occurs

Classification occurs only when the caller has not explicitly declared the pattern kind. In `PatternInterpretationMode.Hybrid` (the default), every untyped pattern in `PatternInput.Patterns` is classified automatically.

When the caller uses `PatternInput.TypedPatterns` or passes an explicit `PatternKind` to `CompiledPatternFactory.Compile(PatternKind, ...)`, classification is bypassed entirely. This is the recommended path for performance-sensitive code.

---

## Classification Rules

Rules are evaluated in priority order:

1. **Empty after trimming** → `GitIgnore` (a whitespace-only pattern is treated as a no-op GitIgnore entry)
2. **Starts with `r:`** → `Regex` (terminal — no further analysis)
3. **Contains `]` without `[`** (stray closing bracket) → `Glob`
4. **Contains `\` without leading `!` and without an escaped character** (Windows path separator) → `Glob`
5. **Contains a leading `\` followed by an escapable character** → `GitIgnore` (escaped metacharacter)
6. **Contains GitIgnore-specific syntax** (`!`, `#`, `**`, trailing `/`, leading `/`) → `GitIgnore`
7. **Contains wildcards or brackets** (`*`, `?`, `[`) → `GitIgnore`
8. **Fallback** → `GitIgnore`

The `PatternAnalyzer` performs a single-pass scan of the pattern to detect these features without allocating intermediate strings.

---

## Malformed Patterns

Before classification, `PatternValidator.IsMalformed` is called. Patterns that fail validation are classified as `PatternKind.Unknown`. The compilation pipeline has no compiler registered for `Unknown`, so a `PatternException` is raised immediately.

Conditions that produce `Unknown`:
- Trailing unescaped backslash (`\`)
- Opening `[` without a closing `]`
- Empty bracket expression (`[]`)
- Genuinely nested brackets such as `[[a-z]]` (but NOT POSIX class syntax `[[:digit:]]`)
- Invalid range syntax `[a-]`
- Range with missing left operand `[-z]`

> **POSIX class exemption:** The `DetectNestedBrackets` regex uses a negative lookahead `(?!:)` to exclude POSIX class prefixes from the nested-bracket check. Without this, `[[:digit:]]` would incorrectly be classified as malformed.

---

## Classification in Specific Mode

When `PatternInterpretationMode.Specific` is used, all patterns must have an explicit kind declared via `TypedPatterns`. Any untyped pattern encountered in this mode causes a `PatternException` with the message: *"Pattern 'X' requires an explicit PatternKind."*

---

## Bypassing Classification

```csharp
// Bypass: all patterns are GitIgnore, no classifier runs
var compiled = CompiledPatternFactory.Compile(
    PatternKind.GitIgnore,
    new[] { "**", "!*.cs", "bin/" }
);
```

This eliminates the `PatternAnalyzer` scan, the `PatternValidator.IsMalformed` call, and the `HashSet<CanonicalPattern>` deduplication — approximately 400–500 bytes of intermediate allocations per pattern.