# Design Goals

This document records the architectural decisions made during the design of Jeninnet.FileQuery and the reasoning behind them. It is intended for contributors and developers who want to understand why the library behaves the way it does.

---

## Goal 1 — Deterministic Rule Evaluation

**Decision:** Adopt last-rule-wins semantics. Patterns are evaluated sequentially; the last matching rule determines the final inclusion state.

**Reasoning:** Most file-matching libraries provide no documented guarantee about how conflicts between rules are resolved. The last-rule-wins model is explicit, predictable, and matches the mental model developers already have from `.gitignore` files. It requires no internal priority system and makes the behavior of any rule set derivable by reading it top to bottom.

---

## Goal 2 — Separation of Traversal and Matching

**Decision:** Traversal and matching are independent systems. The traversal component never inspects pattern strings. The matcher never calls filesystem APIs.

**Reasoning:** Combining these concerns produces systems that are difficult to optimize — improving traversal requires touching the matcher, and vice versa. Separation allows each component to evolve independently. Architecture tests enforce this boundary using reflection.

---

## Goal 3 — Multiple Pattern Languages Without Forcing a Choice

**Decision:** Support GitIgnore, Glob, and Regex dialects in the same rule set via the `HybridPathMatcher`.

**Reasoning:** Different problems suit different syntaxes. Glob patterns are concise for extension matching. GitIgnore patterns are natural for hierarchical include/exclude rules. Regex is necessary for complex naming schemes. Forcing a single syntax means developers either use the wrong tool for the job or write a preprocessing layer themselves.

---

## Goal 4 — Zero Allocation in the Hot Path

**Decision:** All matcher hot-path loops use index-based `for` loops over `IReadOnlyList<T>`. No `foreach` over interface-typed collections.

**Reasoning:** A `foreach` over an interface-typed collection boxes the enumerator struct on every call (~40 bytes per evaluation). At one million files per minute, this produces 40 MB of garbage per minute that triggers GC pauses. Index-based loops eliminate this cost entirely.

**Measured impact:** GitIgnoreMatcher dropped from 40 B to 0 B per call. HybridMatcher dropped from 120 B to 0 B per call.

---

## Goal 5 — Compile Once, Match Many

**Decision:** Patterns are compiled into an executable representation before traversal begins. The scanner is a pure lexer; invariants are a separate validation and transform phase.

**Reasoning:** Raw string parsing during traversal would require repeating the same character-by-character scan for every path evaluated. Pre-compilation moves that cost to query initialization. The invariant pipeline also enables early rejection of structurally invalid patterns before any filesystem access occurs.

---

## Goal 6 — The Invariant System as the Single Safety Net

**Decision:** Pattern validation is expressed as a registry of `IPatternInvariant` implementations organized into Lexical, Structural, and Semantic phases.

**Reasoning:** A monolithic validator becomes brittle as new pattern features are added. An invariant registry allows individual rules to be added, removed, or overridden in isolation. Each invariant has a single, testable responsibility.

---

## Goal 7 — No Exceptions From the Scanner

**Decision:** `PatternScanner` must never throw a `PatternException`. Malformed input is represented as error tokens (e.g., `CharacterClassParseError`) and reported by invariants in the Structural phase.

**Reasoning:** Architecture tests verify that the scanner does not throw. If the scanner threw, an invalid pattern embedded in a large list would crash the entire query rather than producing a meaningful validation error. Error tokens allow the invariant system to collect and report all problems before execution begins.

---

## Non-Goals

**File content inspection.** The engine works with paths only. Content-based filtering is outside scope.

**Parallel traversal.** The v1.0 traversal model is single-threaded. A parallel executor using `Channel<string>` is planned for v1.1.

**Mutable filesystem operations.** The engine is strictly read-only.

**Pattern caching across queries.** Each `FileQuery` compiles its own pattern set. A `PrecompiledQuery` API that allows one compilation to serve multiple root paths is planned for v1.1.