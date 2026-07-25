# Documentation Directory

Welcome to the **Jeninnet.FileQuery** documentation suite. This directory contains comprehensive articles, guides, reference sheets, and architectural specifications.

---

## Getting Started

*   **[System Specification](./system-specification.md)**: Master architectural constraints and invariant contracts.

*   **[Installation](./getting-started/installation.md)**: Nuget instructions and package dependencies.
*   **[Quick Start Guide](./getting-started/quick-start.md)**: First steps with the fluent builder API.
*   **[Basic Patterns](./getting-started/basic-patterns.md)**: Core matching rules.

---

## Reference & Pattern Dialects

*   **[Pattern Overview](./reference/pattern-overview.md)**: Dialect classification.
*   **[GitIgnore Patterns](./reference/gitignore-patterns.md)**: Rule sets and final inclusion logic.
*   **[Glob Patterns](./reference/glob-patterns.md)**: Shell globs and bracket expression matching.
*   **[Regex Patterns](./reference/regex-patterns.md)**: Working with explicit regular expressions.
*   **[Pattern Semantics](./reference/pattern-semantics.md)**: Comprehensive comparison matrix.
*   **[Pattern Precedence](./reference/pattern-precedence.md)**: Order evaluation rules.
*   **[Pattern Language Specification](./specification/pattern-language.md)**: Formal dialect rules.

---

## Architecture & Internals

*   **[Engine Architecture](./architecture/engine-architecture.md)**: Subsystem layering and orchestration.
*   **[Hybrid Matcher](./architecture/hybrid-matcher.md)**: Pattern composition and routing details.
*   **[Matching Layer](./architecture/matching-layer.md)**: Matcher contracts and execution modes.
*   **[Pattern Modes](./architecture/pattern-modes.md)**: Interpretation mode behaviors.
*   **[Pattern Tokenization](./architecture/pattern-tokenization.md)**: Scanner, readers, and token streams.
*   **[Pattern Classification](./architecture/pattern-classification.md)**: Automatically sorting input strings.
*   **[Matcher Invariants](./architecture/matcher-invariants.md)**: Validating tokens without throwing exceptions.
*   **[Public API and Options](./architecture/public-api-options.md)**: Model configurations and properties.
*   **[Internal Visibility](./architecture/internal-visibility.md)**: Enforcing compilation encapsulation.

---

## Traversal Runtime

*   **[FileQuery Runtime](./runtime/filequery-runtime.md)**: The core execute logic.
*   **[Traversal Model](./runtime/traversal-model.md)**: Queue/stack structures during filesystem walks.
*   **[Recursion Depth Options](./runtime/recursion-depth.md)**: Limiting and guarding folders.
*   **[Case Sensitivity Runtime](./runtime/case-sensitivity.md)**: Casing rules per operating system.
*   **Observability Runtime**: Async progress reporting uses `FileQueryProgress`; audit mode emits `FileQueryDiagnostic`; IO recovery uses `FileQueryErrorRecoveryOptions`.

---

## Performance

*   **[Design for Performance](./performance/design-for-performance.md)**: Core design patterns for high throughput.
*   **[Allocation Strategy](./performance/allocation-strategy.md)**: Keeping hot paths allocation-free.
*   **[Matcher Performance](./performance/matcher-performance.md)**: Internal benchmarks and segment matches.
*   **[Traversal Performance Model](./performance/traversal-model.md)**: High-speed traversal details.
*   **[Benchmarking Guide](./performance/benchmarking.md)**: Recreating measurements.
*   **[Release Baselines](./performance/release-benchmark-baseline.md)**: Documented execution speeds.

---

## Integrations & Extensions

*   **[CommandLine Integration](./guides/command-line-integration.md)**: Using CLI argument structures.
*   **[Dependency Injection Guide](./guides/dependency-injection.md)**: Managing registry configurations.

---

## Contributing & Plans

*   **[Contributing Guide](./contributing/contributing-guide.md)**: Submitting changes.
*   **[Project Structure Guide](./contributing/project-structure.md)**: Solution layouts.
*   **[Testing Guidelines](./contributing/testing.md)**: Ensuring zero-allocation matching checks pass.
*   **[Testing and Release Plan](./contributing/testing-and-release-plan.md)**: Core QA procedures.
*   **[Release Checklist](./contributing/release-checklist.md)**: Pre-flight verifications.
*   **[Refactor Tracking](./architecture-refactor-tracking.md)**: Current roadmap changes.
