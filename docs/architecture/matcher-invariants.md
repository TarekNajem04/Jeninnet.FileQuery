# Matcher Invariants

The invariant system is the safety net that validates patterns before compilation produces an executable matcher. It is organized as a registry of `IPatternInvariant` implementations, each with a single testable responsibility.

---

## Phases

Invariants are executed in three sequential phases:

| Phase | When | Responsibility |
|-------|------|----------------|
| **Lexical** | Before scanning | Raw text validation |
| **Structural** | After scanning | Token stream validation |
| **Semantic** | After structural | Dialect transforms and cross-token meaning |

A failure in any phase raises a `PatternException` and halts compilation for that pattern.

---

## Lexical Invariants

Run against `context.Pattern.Text` before the scanner tokenizes the pattern.

### `EmptyPatternInvariant`
Rejects null or whitespace-only patterns.

### `LiteralNormalizationInvariant`
Rejects patterns containing ASCII control characters (bytes 0x00–0x1F). These cannot appear in valid filesystem paths and indicate malformed input.

### `RegexSyntaxInvariant`
Applies only to `PatternKind.Regex`. Strips the `r:` prefix and attempts to compile the remaining expression with `new Regex(expression, ...)`. If compilation throws `ArgumentException`, the pattern is rejected with a descriptive error message.

> **Note:** The prefix is stripped before validation. This ensures the invariant validates the expression that the matcher will actually use — not the raw string including `r:`.

---

## Structural Invariants

Run against `context.Tokens` after `PatternScanner.Scan`.

### `CharacterClassStructureInvariant`
Inspects every `CharacterClassToken` for `CharacterClassParseError` sentinel elements. The parser never throws — it records errors as sentinels inside the element list. This invariant converts those sentinels into `PatternException` failures.

### `CharacterClassRangeInvariant`
Inspects every `CharRange` element and verifies that `Start <= End`. Inverted ranges like `z-a` are rejected with a descriptive message including the Unicode code points of both endpoints.

### `CurrentDirectoryInvariant`
Rejects patterns containing a segment that is the single literal `.`. A `.` segment is never a valid path component in a pattern.

### `ParentTraversalInvariant`
Rejects patterns containing a `..` segment. Parent directory traversal in patterns is a security concern and is unconditionally rejected regardless of context.

### `RecursiveWildcardInSegmentInvariant`
Rejects segments that contain a `RecursiveWildcardToken` alongside other tokens. Mixed segments like `**a`, `a**`, and `a**b` are structurally invalid in both Glob and GitIgnore dialects. Applies to all pattern kinds.

### `RecursiveWildcardRedundancyInvariant`
Rejects adjacent `**` segments (`**/**`). This is redundant — a single `**` already matches zero or more segments.

---

## Semantic Invariants

Run after structural validation. May modify `context.Tokens`.

### `GitIgnoreImplicitRecursiveInvariant` *(GitIgnore only)*
Prepends an implicit `**` segment to all non-root-anchored GitIgnore patterns that do not already begin with `**`. This implements the GitIgnore rule that unanchored patterns match at any depth.

> **History:** This transform was previously split between `PatternScanner.ApplyImplicitRecursiveWildcard` (scanner layer — wrong) and `GitIgnoreNegationImplicitRecursiveInvariant` (handled only negated patterns). Both were replaced by this single invariant in the Semantic phase, which is the correct layer for dialect-specific token stream transforms.

### `GitIgnorePatternInvariant` *(GitIgnore only)*
Validates:
- A directory-only pattern must have at least one segment.
- A root-anchored pattern with an empty body (bare `/`) is rejected — it has no segment to match.

### `GlobPatternInvariant` *(Glob only)*
Verifies that `**` appears as a standalone segment in Glob patterns. Mixed segments like `**a` are caught by `RecursiveWildcardInSegmentInvariant` earlier; this invariant provides additional Glob-specific context in the error message.

### `RecursiveWildcardIsolationInvariant`
Verifies that no single segment contains more than one `RecursiveWildcardToken`. Applies to all pattern kinds.

---

## Adding a Custom Invariant

Implement `IPatternInvariant` and add it to the `PatternInvariantRegistry` via `PatternPipeline.CreateDefault()`:

```csharp
internal sealed class MaxSegmentCountInvariant : IPatternInvariant {
    public PatternInvariantPhase Phase   => PatternInvariantPhase.Structural;
    public PatternKind?          AppliesTo => null; // all kinds

    public PatternInvariantResult Validate(PatternCompilationContext context) {
        if (context.Tokens!.Count > 16) {
            return PatternInvariantResult.Fail(
                "Patterns with more than 16 segments are not supported.");
        }
        return PatternInvariantResult.Success;
    }
}
```

Register it by constructing a custom `PatternPipeline` and calling `CompiledPatternFactory.Configure(pipeline)` once at startup.

---

## Invariant Contract

- An invariant that returns `PatternInvariantResult.Success` must not modify `context.Tokens`.
- Only Semantic invariants may modify `context.Tokens` — and only by prepending, appending, or wrapping segments. They must not change the relative order of existing segments.
- Invariants are executed in registry order within each phase. The `PatternInvariantRegistry` groups them by phase at construction time.