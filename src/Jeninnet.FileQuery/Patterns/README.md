# Patterns Compiler Subsystem

This folder contains the **Patterns Compiler** subsystem for [Jeninnet.FileQuery](../README.md).

It is responsible for parsing raw string inputs, checking them against structural invariants, and outputting immutable, compiled pattern structures.

---

## 🏗️ Architectural Location

The patterns layer acts as the **front gate** of the query engine:

```
[Raw Pattern Input] ──► [Patterns Compiler Layer] ──► [Matching Layer]
```

---

## 🎯 Primary Responsibilities

1.  **Lexical Analysis**: [PatternScanner](./Tokenization/PatternScanner.cs) reads raw characters (utilizing `ReadOnlySpan<char>`) and generates token sequences without raising exceptions.
2.  **Structural Validation**: Independent checks implement [IPatternInvariant](./Invariants/Definition/IPatternInvariant.cs) to verify semantic rules (e.g., character class range bounds, recursive wildcard boundary checks, regex validity).
3.  **Compilation**: Converts validated token collections into compiled matcher representations (e.g., [CompiledPattern](./Compiled/CompiledPattern.cs)).
4.  **Diagnostic Metadata**: Preserves source pattern text and source order on compiled patterns so optional runtime audit mode can explain responsible match decisions.

---

## 🚫 Architectural Constraints

*   **No File System Access**: The compiler must never interact with storage or OS directories.
*   **No Matching Engine Dependency**: Contains zero execution logic; its outputs are consumed by matchers in the matching namespace.
*   **No Thread Contention**: All generated patterns are immutable and thread-safe.

---

## 🧬 Sub-Components

*   **[Syntax Profiling](./Syntax/PatternSyntaxProfile.cs)**: Identifies dialetical structures (Glob, GitIgnore, and Regex).
*   **[Invariants Subsystem](./Invariants/Definition/IPatternInvariant.cs)**: Houses structural, semantic, and lexical validation routines.
*   **[Tokenization Subsystem](./Tokenization/IPatternTokenizer.cs)**: Readers for literals, escape symbols, ranges, wildcards, and POSIX class expressions.
