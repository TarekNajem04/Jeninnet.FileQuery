Invariant Philosophy
====================

The pattern system enforces correctness in three stages:

1. **Text-level** invariants
   - Scope: raw pattern strings
   - Responsibility: ensure the input is syntactically valid before compilation
   - Examples:
     - Pattern is not null or empty (unless empty is meaningful)
     - Disallow unsupported escape sequences
     - Enforce encoding or character set rules

2. **Structural invariants**
   - Scope: CompiledPattern instances
   - Responsibility: ensure the compiled representation is internally consistent
   - Examples:
     - Segments collection is non-null
     - No null or empty segments
     - No null tokens
   - These invariants are *universal* and independent of pattern type.

3. **Pattern-type invariants**
   - Scope: CompiledPattern + PatternKind
   - Responsibility: enforce semantics specific to a pattern dialect
   - Examples:
     - In Glob, '**' must be a standalone segment
     - In GitIgnore, certain combinations of '!' and '/' may be disallowed
   - Implemented via IPatternTypeInvariant and resolved by PatternInvariantRegistry.

**`PatternException`** is thrown whenever a pattern fails validation, cannot be parsed, or violates structural or semantic invariants.
It represents a domain-level failure and is the primary exception type that callers should catch when compiling patterns.

Design Goals
------------

- Make invariants explicit and testable.
- Keep structural rules centralized and reusable.
- Isolate pattern-type-specific rules to avoid cross-contamination.
- Ensure that any CompiledPattern returned by PatternCompilerBase has passed all relevant invariant checks,
  so matchers can assume a valid structure and focus solely on evaluation logic.
