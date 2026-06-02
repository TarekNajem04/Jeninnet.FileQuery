# Pattern Tokenization

Tokenization converts a raw pattern string into a structured sequence of `IPatternToken` objects. This is performed by `PatternScanner` in the namespace `Jeninnet.FileQuery.Patterns.Tokenization`.

---

## Token Types

| Token | Syntax | Meaning |
|-------|--------|---------|
| `LiteralToken(string Text)` | `foo`, `.cs` | Match the exact text |
| `WildcardToken` | `*` | Match any sequence within one segment |
| `RecursiveWildcardToken` | `**` | Match zero or more path segments |
| `SingleCharToken` | `?` | Match exactly one character |
| `CharacterClassToken(CharacterClass)` | `[abc]`, `[0-9]` | Match one character from the set |
| `RegularExpressionToken(string Pattern)` | `r:^src/.*` | Full regex against the path |
| `EscapeToken(char Escaped)` | `\*`, `\!` | Literal interpretation of the escaped character |

---

## The Character Class Token

`CharacterClassToken` wraps a `CharacterClass` AST node. The AST node contains an ordered list of `ICharacterClassElement` entries:

| Element | Meaning |
|---------|---------|
| `CharLiteral(char Value)` | Match the literal character |
| `CharRange(char Start, char End)` | Match any character in the inclusive range |
| `PosixClass(string Name)` | Match a POSIX named class (e.g., `digit`, `alpha`) |
| `CharacterClassParseError(string Message)` | Compile-time parse error sentinel |

The `CharacterClassParser` never throws. Malformed classes produce a `CharacterClassParseError` sentinel, which `CharacterClassStructureInvariant` detects and converts to a `PatternException`.

---

## Scanning Process

`PatternScanner.Scan` runs in five steps:

1. **Whole-pattern tokenizers** — tried first. `RegexPatternTokenizer` recognizes the `r:` prefix and produces a single `RegularExpressionToken` segment without further segment splitting.

2. **Structural analysis** — identifies the pattern's structural markers: leading `!` (negation), leading `/` (root anchor), trailing `/` (directory-only). These set fields on `PatternContext` but are not emitted as tokens.

3. **Segment splitting** — divides the effective body on `/` separators to produce a list of `(start, length)` spans.

4. **Per-segment tokenization** — each span is passed through the tokenizer chain:
   - `EscapeTokenizer` — highest priority; handles `\x` sequences
   - `RecursiveWildcardTokenizer` — recognizes `**`
   - `WildcardTokenizer` — recognizes `*`
   - `SingleCharWildcardTokenizer` — recognizes `?`
   - `CharacterClassTokenizer` — recognizes `[...]` and delegates to `CharacterClassParser`
   - `LiteralTokenizer` — fallback; always succeeds

5. **Context and token list stored** on `PatternCompilationContext` for the invariant phases.

---

## Scanner Responsibility Boundary

The scanner is a **pure lexer**. It has no knowledge of pattern semantics or dialects. It does not:

- Apply the implicit `**` prefix for unanchored GitIgnore patterns (that is `GitIgnoreImplicitRecursiveInvariant`'s responsibility)
- Validate whether a `**` segment is isolated (that is `RecursiveWildcardInSegmentInvariant`'s responsibility)
- Detect inverted character class ranges (that is `CharacterClassRangeInvariant`'s responsibility)

This separation ensures the scanner remains stable as new dialects and invariants are added.

---

## Performance Notes

- The outer segment list is pre-sized using the segment count from step 3, avoiding `List<T>` resizing.
- Each per-segment token list is initialized with capacity 3, which is the right size for the majority of segments (e.g., `*.cs` → `[Wildcard, Literal(".cs")]`).
- The tokenizer chain is a static array. No allocation occurs from selecting the next tokenizer.