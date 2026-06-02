# Hybrid Matcher Architecture

The pattern system in `Jeninnet.FileQuery` is implemented through a **hybrid matcher architecture** designed to support multiple pattern languages without sacrificing deterministic behavior.

Instead of forcing all patterns into a single syntax, the engine allows each pattern language to be handled by a specialized matcher.

These matchers are coordinated through a central component known as the **HybridPathMatcher**.

The HybridPathMatcher acts as the orchestration layer responsible for delegating pattern evaluation to the correct matcher while preserving rule ordering and evaluation semantics.

---

# Architectural Motivation

Traditional file filtering libraries typically implement one of two approaches.

The first approach converts all patterns into a single internal representation. While simple, this approach often leads to semantic mismatches between pattern languages.

The second approach uses independent matchers but leaves the responsibility of coordination to the caller, which introduces complexity and inconsistent behavior.

`Jeninnet.FileQuery` takes a different approach by introducing a **hybrid matcher pipeline**.

Each pattern language has its own matcher implementation, while a coordinating layer ensures that rule ordering and inclusion semantics remain consistent.

This architecture allows developers to mix pattern languages while still maintaining a predictable rule evaluation model.

---

# HybridPathMatcher

The **HybridPathMatcher** is responsible for coordinating all matcher implementations.

Its responsibilities include:

• receiving classified patterns
• constructing matcher pipelines
• delegating path evaluation
• maintaining ordered rule evaluation

Conceptually the architecture looks like this:

```
                    HybridPathMatcher
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
  GitIgnoreMatcher     GlobMatcher      RegexPathMatcher
```

Each matcher specializes in interpreting patterns belonging to its pattern language.

The HybridPathMatcher ensures that pattern ordering and inclusion semantics are respected across the entire matcher pipeline.

---

# Pattern Routing

Patterns are routed to matchers through the **PatternClassifier**.

The classifier analyzes the raw pattern and determines its pattern type.

For example:

```
*.cs           → Glob
**/*.txt       → GitIgnore
r:^data_.*     → Regex
```

The classifier ensures that each pattern is processed by the matcher designed for that syntax.

This separation allows each matcher implementation to remain simple and focused.

---

# Matcher Pipeline Construction

When the engine initializes pattern matching, it builds a matcher pipeline.

The process follows several stages.

First, patterns are canonicalized so that path separators and formatting are normalized.

Second, patterns are classified to determine their pattern type.

Third, patterns are grouped by matcher type.

Finally, the HybridPathMatcher constructs the matcher pipeline.

Conceptually the pipeline looks like this:

```
PatternInput
      │
      ▼
PatternCanonicalizer
      │
      ▼
PatternClassifier
      │
      ▼
Matcher Construction
      │
      ▼
HybridPathMatcher
```

Once this pipeline is built, it can evaluate any number of paths efficiently.

---

# Ordered Rule Evaluation

One of the most important guarantees provided by the engine is **deterministic rule evaluation**.

Patterns are evaluated in the exact order provided by the user.

Each rule updates the inclusion state of the current path.

For example:

```
**
!*.log
data.log
```

Evaluation proceeds as follows.

First the engine includes all files.

Second it excludes files matching the `.log` extension.

Third it explicitly includes `data.log`.

The final inclusion state determines whether the file is returned by the query.

This rule model ensures that behavior remains predictable even when complex rule sets are used.

---

# Matcher Invariants

The matcher system operates under a set of strict **invariants** defined in:

```
Jeninnet.FileQuery.Patterns.Invariants
```

These invariants guarantee that matcher implementations behave consistently.

They also prevent subtle bugs that often occur in pattern matching systems.

Matchers must follow these core principles.

Paths must be treated as immutable values during evaluation.

Matchers must not modify the path being evaluated.

All normalization must occur before matching begins.

Matchers must respect the global case sensitivity mode provided by the runtime.

Pattern evaluation must occur in the exact order defined by the pattern list.

Matchers must not maintain mutable state between path evaluations.

These guarantees ensure that the matcher system behaves consistently regardless of pattern complexity.

---

# Pattern Tokenization

Pattern matching performance depends heavily on avoiding repeated parsing of pattern strings.

To achieve this, `Jeninnet.FileQuery` tokenizes patterns before matching begins.

Tokenization is implemented in the namespace:

```
Jeninnet.FileQuery.Patterns.Tokenization
```

During tokenization, pattern strings are converted into a sequence of tokens representing pattern semantics.

For example, the pattern:

```
**/*.txt
```

may be represented internally as tokens such as:

```
RecursiveWildcard
PathSeparator
Wildcard
Literal("txt")
```

This structured representation allows the matcher to evaluate patterns efficiently without repeatedly interpreting raw strings.

Tokenization also allows the engine to validate patterns and enforce matcher invariants.

---

# Matcher Execution

Once the matcher pipeline has been constructed, the engine evaluates each file path encountered during filesystem traversal.

For each path, the HybridPathMatcher evaluates patterns sequentially.

The matcher pipeline determines whether each pattern matches the current path.

If a pattern matches, it updates the inclusion state.

Once all patterns have been evaluated, the final inclusion state determines whether the path is included in the result set.

This process is designed to minimize allocations and unnecessary computation during traversal.

---

# Interaction With Filesystem Traversal

Matchers are deliberately isolated from filesystem traversal logic.

Traversal is responsible for discovering paths.

Matchers are responsible for evaluating patterns against those paths.

This separation ensures that improvements in traversal performance do not require changes to matcher implementations.

It also allows the matcher system to remain independent of the underlying filesystem APIs.

---

# Advantages of the Hybrid Matcher Design

The hybrid matcher architecture provides several important advantages.

Multiple pattern languages can coexist within the same rule set.

Each matcher implementation can remain small and focused.

Pattern semantics remain deterministic.

Pattern matching can be optimized independently of traversal.

The system remains extensible, allowing additional pattern languages to be introduced in the future.

---

# Extensibility

The architecture was designed with extensibility in mind.

New pattern languages can be integrated by implementing a new matcher and extending the classification system.

For example, a future extension might introduce:

```
Ant-style patterns
custom DSL patterns
domain-specific file filters
```

Because the HybridPathMatcher coordinates all matchers, these extensions can be added without modifying the existing matcher implementations.

---

# Why This Architecture Matters

File matching seems simple at first glance, but becomes complex when multiple pattern languages, ordered rules, and filesystem traversal are combined.

The hybrid matcher architecture solves this complexity by separating responsibilities across well-defined components.

The result is a system that is both powerful and predictable.

Developers can express complex file filtering rules while remaining confident that pattern evaluation will behave exactly as expected.