# Performance & Traversal Model

Efficient file querying requires more than pattern matching. The true performance cost often comes from **filesystem traversal**, not from the pattern evaluation itself.
`Jeninnet.FileQuery` was designed with this reality in mind.

The architecture separates responsibilities into two independent systems:

```
filesystem traversal
pattern evaluation
```

Traversal discovers paths.
Matchers evaluate those paths.

Because these concerns are isolated, the engine can optimize traversal without affecting the pattern language or matcher architecture.

---

# Filesystem Traversal Strategy

When a query is executed through `FileQueryRuntime`, the runtime begins by walking the directory tree starting from the provided root path.

Conceptually the process looks like this:

```
Root Directory
      │
      ▼
Directory Enumeration
      │
      ▼
Path Normalization
      │
      ▼
Matcher Evaluation
      │
      ▼
Result Stream
```

Traversal is intentionally designed to behave like a **streaming pipeline**. Paths are discovered, evaluated, and emitted incrementally instead of building large in-memory collections.

This allows the engine to scale to directory trees containing **millions of files**.

The asynchronous traversal API is cooperative rather than fully non-blocking at the operating-system level. The default filesystem uses synchronous .NET directory and attribute APIs, checks cancellation between entries, and yields control to async consumers between emitted paths. It intentionally does not wrap each filesystem call in `Task.Run`, because doing so would create avoidable thread-pool pressure on large directory trees.

---

# Streaming Enumeration

One of the key design principles is that traversal should behave as a **lazy enumeration**.

Instead of collecting all paths before filtering them, the runtime evaluates each path as soon as it is discovered.

This produces a pipeline like the following:

```
filesystem
   ↓
path discovered
   ↓
pattern evaluation
   ↓
yield result
```

This streaming behavior provides several advantages.

Memory consumption remains minimal even for large directory trees.
Applications can begin processing results immediately without waiting for traversal to finish.
Traversal can stop early if the consumer stops enumeration.

---

# Recursion Model

Directory traversal supports configurable recursion behavior through the query options.

Two important properties control recursion:

```
RecurseSubdirectories
MaxRecursionDepth
```

`RecurseSubdirectories` determines whether the traversal descends into subdirectories.

`MaxRecursionDepth` limits how deep traversal is allowed to go.

Example:

```csharp
MaxRecursionDepth = 0
```

This configuration restricts the query to the root directory only.

Limiting recursion depth is an important performance tool when scanning large directory trees where deep traversal is unnecessary.

---

# Path Normalization

Before any matcher evaluates a path, the runtime normalizes the path representation.

This normalization ensures consistent behavior across different operating systems.

Responsibilities of this stage include:

* consistent directory separator handling
* removal of redundant segments
* predictable path representation

Normalization ensures that matchers never need to worry about platform-specific path variations.

---

# Pattern Matching Cost Model

Pattern matching itself is designed to be computationally lightweight.

Several architectural decisions contribute to this.

Patterns are tokenized before traversal begins.
Matchers operate on structured tokens instead of raw pattern strings.
Classification determines the matcher implementation ahead of time.

This allows the engine to avoid repeated parsing or interpretation during traversal.

In practice, most of the work occurs during **query initialization**, not during per-path evaluation.

---

# Tokenization and Performance

Tokenization is one of the key performance features of the engine.

Instead of interpreting pattern strings repeatedly, patterns are converted into tokens during initialization.

For example, the pattern:

```
**/*.txt
```

may be represented internally as a sequence of tokens representing recursive wildcards, separators, and literals.

Because matchers operate on tokens rather than strings, pattern evaluation becomes significantly faster and more predictable.

This also reduces memory allocations during traversal.

---

# Allocation Minimization

High-performance file traversal requires careful control of memory allocations.

`Jeninnet.FileQuery` minimizes allocations by:

* tokenizing patterns once during initialization
* streaming traversal results
* avoiding unnecessary string operations during matching

The result is a runtime that can process very large directory trees without generating excessive garbage collection pressure.

---

# Case Sensitivity Strategy

Different operating systems treat path case sensitivity differently.

Windows filesystems are typically case-insensitive.
Linux filesystems are usually case-sensitive.

The engine allows this behavior to be controlled through the query options.

```
CaseSensitivity.Sensitive
CaseSensitivity.Insensitive
```

By making case behavior explicit, the engine ensures that pattern matching remains predictable across platforms.

---

# Matcher Isolation

Matchers are deliberately isolated from traversal logic.

This separation provides two benefits.

Traversal algorithms can evolve independently without affecting pattern semantics.
Matcher implementations remain focused on evaluating patterns rather than interacting with filesystem APIs.

Because of this separation, the engine architecture remains clean and maintainable.

---

# Scalability Considerations

The engine was designed to perform well in scenarios such as:

* scanning build output directories
* analyzing large source code repositories
* processing log archives
* indexing file systems

These scenarios often involve directory trees containing hundreds of thousands or millions of files.

By streaming traversal and minimizing allocations, `Jeninnet.FileQuery` maintains predictable performance even in these environments.

---

# Execution Pipeline Summary

When a query runs, the complete pipeline can be summarized as:

```
Pattern Initialization
      │
      ▼
Pattern Canonicalization
      │
      ▼
Pattern Classification
      │
      ▼
Pattern Tokenization
      │
      ▼
Matcher Construction
      │
      ▼
Filesystem Traversal
      │
      ▼
Path Evaluation
      │
      ▼
Result Enumeration
```

This architecture allows expensive operations to occur once during initialization while keeping per-path evaluation lightweight.

---

# Why Performance Matters

File discovery is a foundational task in many development tools and automation systems.
Slow traversal or inefficient matching can easily become a bottleneck.

By focusing on traversal efficiency, deterministic pattern evaluation, and minimal allocations, `Jeninnet.FileQuery` provides a robust foundation for applications that must operate on large file systems.
