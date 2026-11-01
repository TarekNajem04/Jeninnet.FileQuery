# Brace Expansion

Glob and GitIgnore patterns support shell-style brace alternation:

```csharp
FileQuery.From(root).Where("src/**/*.{cs,vb}").Execute();
```

This is equivalent to querying `src/**/*.cs` and `src/**/*.vb` in one rule.

---

## Rules

- `{a,b}` expands to one pattern per alternative.
- Nested groups form a cartesian product:
  `{src,test}/**/*.{cs,vb}` → `src/**/*.cs`, `src/**/*.vb`, `test/**/*.cs`, `test/**/*.vb`.
- A group without a comma is literal: `{a}` matches the literal text `{a}`.
- Escaped braces are literal: `\{a,b\}.txt` matches a file named `{a,b}.txt`.
- Braces inside a character class are literal: `[{,}]` matches `{`, `,`, or `}`.
- Empty alternatives (`{a,}`) and unmatched braces are validation errors with a position.
- Expansion is bounded to 256 alternatives; larger inputs fail with a clear error.
- Regex patterns are excluded from brace expansion, so quantifiers such as `r:a{2,3}` keep
  their regex meaning.

---

## Matching semantics

Expansion happens once, before tokenization and compilation. Each alternative is compiled as a
normal rule in the same order as written, so GitIgnore last-match-wins and negation still apply.
Explain diagnostics show the expanded rule (for example `*.cs`) rather than the source brace group.

Brace alternation is classified as a Glob feature; patterns that also use GitIgnore-only syntax
(negation, `**`, directory-only markers) keep GitIgnore semantics.

---

## Tooling

`PatternInspector.Inspect` reports `HasBraceAlternation`, and `PatternInspector.Validate` reports
unmatched braces with `Position`/`Segment`:

```csharp
var inspection = PatternInspector.Inspect("*.{cs,vb}");
// HasBraceAlternation: true
// Kind: Glob
```
