# Jeninnet.FileQuery System Specification

This document serves as the authoritative Software Requirements Specification (SRS) and architecture manifesto for the Jeninnet.FileQuery engine. It aggregates the constitutional constraints, matching semantics, and cross-platform behaviors into a single source of truth. All components MUST adhere strictly to the rules defined herein.

The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED",  "MAY", and "OPTIONAL" in this document are to be interpreted as described in RFC 2119.

---

## 1. Declarative Semantics (The Contract)

### 1.1 Intent-Driven Matching
All file selection MUST be expressed through declarative patterns (GitIgnore, Glob, Regex). Patterns represent the explicit *intent* of the query and MUST be treated as an immutable contract. The engine MUST NOT execute runtime heuristics, implicit overrides, or ad-hoc parsing outside the boundaries of the compiled pattern invariants.

### 1.2 Determinism
Pattern semantics MUST be deterministic and predictable across all supported operating systems (Windows, Linux, macOS).

---

## 2. Execution Pipeline

The execution pipeline MUST strictly separate pattern compilation from filesystem traversal. The engine operates on a Bertrand Meyer-style closed-world contract: parsing and classification happen *before* matching begins.

### 2.1 Phase 1: Tokenization & Invariant Validation
- **Tokenization**: Pattern scanning MUST NOT throw exceptions for malformed input. It MUST operate as a pure lexer.
- **Invariant Enforcement**: Structural and semantic issues MUST be surfaced through the invariant validation phase.
- **Diagnostics**: Invalid patterns MUST yield a `PatternResult<T>` containing rich diagnostic errors rather than throwing unstructured exceptions.

### 2.2 Phase 2: Compilation
- **Zero-Allocation**: Pattern compilers MUST construct execution instructions that produce zero heap allocations during the matching hot path.
- **Stateless Matchers**: Matchers MUST NOT maintain mutable state between path evaluations. They MUST only evaluate `ICompiledPatternSet` instances.
- **Dialect Isolation**: Regex patterns MUST be explicit (using the `r:` prefix) and MUST be processed in isolation from GitIgnore and Glob logic.

### 2.3 Phase 3: Traversal Execution
- **IO Abstraction**: All filesystem interactions MUST go through the `IFileSystem` interface. The traversal engine MUST NOT directly use `System.IO` (e.g., `File`, `Directory`).
- **Traversal Plan**: The engine MUST evaluate patterns against normalized path strings retrieved via the traversal queue or stack.
- **Progress & Observability**: Async enumeration SHOULD support `IProgress<T>` for real-time scan statistics and MUST respect deep `CancellationToken` propagation.

---

## 3. High-Performance & AOT Readiness

The engine is built for cloud-native workloads, high-throughput enumeration, and Native AOT compilation.

### 3.1 Reflection Ban
The engine MUST NOT use reflection in the enumeration or matching pipeline.

### 3.2 Hot Path Constraints
The matching evaluation loop MUST utilize `ReadOnlySpan<char>` and index-based `for` loops. The use of LINQ closures, enumerator boxing (`foreach` over interface-typed collections), and heap-allocated parsing arrays is strictly FORBIDDEN in the hot path.

### 3.3 Case Sensitivity
Case sensitivity MUST follow the native operating system defaults unless explicitly overridden by the `FileQueryOptions` configuration.

---

## 4. Governance

Any modification to pattern semantics, matching rules, invariant logic, or engine behavior MUST undergo architectural review, require a version bump, and ensure zero regressions in performance or cross-platform behavior.
