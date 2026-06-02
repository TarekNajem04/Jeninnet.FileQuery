# Jeninnet.FileQuery

### Building a Deterministic File Query Engine for .NET

Every non-trivial software system eventually encounters the same deceptively simple task: **finding files**.

Build systems search for source files.
Backup tools scan directories to determine what has changed.
Code analyzers walk entire repositories.
Log processors filter terabytes of archived data.

At first glance, file discovery appears trivial. Modern operating systems provide directory enumeration APIs, and many programming environments include globbing utilities. Yet once a project grows beyond a few directories, developers quickly discover that the problem is far more subtle.

Pattern languages behave inconsistently.
Traversal becomes expensive on large directory trees.
Rule ordering becomes unclear when exclusions and inclusions interact.
Different pattern syntaxes cannot easily be combined.

These frustrations eventually led to the creation of **Jeninnet.FileQuery**, a library designed to solve file discovery as a first-class architectural problem rather than a small utility function.

---

## The Problem With Traditional File Matching

Most libraries approach file matching from one of two directions.

Some rely entirely on **glob patterns**. These are simple and familiar, but quickly become insufficient when more expressive filtering is required.

Others rely on **regular expressions**. While powerful, they are often difficult to read and are poorly suited for hierarchical filesystem rules.

Then there are systems inspired by **GitIgnore semantics**, which introduce rule ordering and negation. These are expressive, but they are rarely implemented in a way that allows them to coexist with other pattern systems.

The deeper problem is not the syntax itself. The real challenge lies in **how these rules are evaluated**.

Once pattern languages mix inclusion rules, exclusion rules, recursion, and directory traversal, small ambiguities quickly lead to unexpected behavior.

The goal behind Jeninnet.FileQuery was therefore not simply to support another pattern syntax. The goal was to design a **deterministic file query engine** capable of handling complex rule sets in a predictable and performant way.

---

## Deterministic Rule Evaluation

One of the first design decisions in the engine was the adoption of a simple and powerful rule model:

**Patterns are evaluated sequentially, and the last matching rule determines the final result.**

This rule is inspired by GitIgnore semantics and provides a surprisingly intuitive way to reason about complex rule sets.

Consider the following pattern list:

```
**
!*.log
data.log
```

The rules can be read almost like a narrative.

First, everything is included.
Then log files are excluded.
Finally, a specific log file is re-included.

Because the rules are evaluated in order, the final inclusion state becomes completely predictable.

This principle may seem simple, but it becomes the foundation upon which the entire engine architecture is built.

---

## Separating Traversal From Matching

Many file filtering libraries combine directory traversal and pattern evaluation into a single algorithm. While this approach works for simple cases, it quickly becomes difficult to optimize.

Jeninnet.FileQuery instead treats traversal and matching as two independent systems.

Traversal is responsible only for discovering filesystem paths.
Matching is responsible only for evaluating those paths against patterns.

This separation provides several benefits.

Traversal algorithms can be optimized without affecting pattern semantics.
Pattern matchers remain small and focused.
The entire engine becomes easier to reason about and extend.

Most importantly, the runtime can operate as a **streaming pipeline**.

Paths are discovered, evaluated, and emitted immediately rather than being collected in memory first. This allows the engine to process very large directory trees with minimal memory overhead.

---

## Multiple Pattern Languages

Once the rule evaluation model was defined, the next challenge was enabling developers to express their rules naturally.

Different situations call for different pattern languages.

GitIgnore rules provide expressive inclusion and exclusion behavior.
Glob patterns offer concise wildcard matching.
Regular expressions allow complex filtering logic.

Rather than forcing developers to choose a single syntax, Jeninnet.FileQuery supports all three simultaneously.

This capability is made possible by the engine’s most important component: the **HybridPathMatcher**.

---

## The Hybrid Matcher

The HybridPathMatcher acts as the coordination layer for the pattern system.

Instead of interpreting all patterns through one algorithm, the engine classifies patterns and delegates them to specialized matchers.

Each matcher understands one pattern language.

GitIgnore patterns are evaluated by a GitIgnore matcher.
Glob patterns are evaluated by a glob matcher.
Regular expressions are handled by a regex matcher.

The HybridPathMatcher ensures that these matchers operate within the same rule evaluation pipeline.

The result is a system where multiple pattern languages can coexist without interfering with each other.

From the perspective of the user, the rules simply behave as a unified list.

---

## From Pattern Strings to Executable Matchers

When a query is executed, patterns do not remain as raw strings.

Instead they pass through a transformation pipeline.

First, patterns are **canonicalized**, ensuring consistent formatting and path separators.
Next, they are **classified** to determine which matcher should interpret them.
Finally, patterns are **tokenized**, converting them into structured representations that matchers can evaluate efficiently.

Tokenization plays an important role in performance. Rather than repeatedly interpreting pattern strings, matchers operate on lightweight tokens representing semantic elements such as wildcards, directory boundaries, or literal segments.

This design moves most of the computational work to query initialization rather than per-path evaluation.

---

## Matcher Invariants

To guarantee consistent behavior across different matcher implementations, the engine enforces a set of strict invariants.

Matchers must treat paths as immutable values.
They must respect the global case sensitivity configuration.
They must evaluate patterns strictly in the order provided.

Most importantly, matchers must remain stateless between path evaluations.

These constraints may appear restrictive, but they ensure that pattern evaluation remains deterministic regardless of pattern complexity or matcher implementation.

---

## Traversal at Scale

Pattern matching is only part of the problem. In real applications the dominant cost often comes from filesystem traversal itself.

Large repositories, build outputs, and log archives can easily contain hundreds of thousands of files.

Jeninnet.FileQuery addresses this challenge through a traversal model built around streaming enumeration.

Paths are discovered incrementally and immediately evaluated against the matcher pipeline. If a path matches the rules, it is emitted to the caller.

Because results are streamed rather than accumulated, the engine can process extremely large directory trees without excessive memory consumption.

Developers may also control traversal depth and recursion behavior, allowing queries to remain efficient even when scanning complex directory structures.

---

## Command Line and Dependency Injection

Although the engine itself focuses purely on file querying, additional packages provide convenient integration layers.

The **CommandLine** package allows command-line applications to map arguments directly into pattern options.
The **DependencyInjection** package allows the runtime to be registered within application service containers.

These integrations keep the core library lightweight while allowing it to integrate naturally into modern .NET applications.

---

## Why Architecture Matters

At first glance, file matching may appear to be a small problem. Yet once a system grows large enough, file discovery often becomes a foundational component.

Build tools rely on it to discover source files.
Automation pipelines use it to locate artifacts.
Analysis tools depend on it to traverse massive codebases.

When the behavior of a file matcher becomes unpredictable, the effects ripple across entire systems.

Jeninnet.FileQuery was designed to prevent these issues by providing a clear rule model, a modular matcher architecture, and a traversal pipeline optimized for scale.

The goal is not simply to match files. The goal is to provide developers with a **reliable and understandable file query engine**.

---

## Exploring the Project

Developers interested in the engine can explore the project in several ways.

The repository contains sample applications demonstrating common matching scenarios.
The documentation describes the pattern language and architecture in detail.
The NuGet packages allow the library to be integrated into applications with minimal setup.

The best way to understand the system, however, is simply to try it.

Define a few patterns.
Run a query against a directory.
Observe how the rules behave.

Once the model becomes familiar, it becomes clear that file discovery does not need to be mysterious or unpredictable.

It can be deterministic, expressive, and fast.

That is the idea behind **Jeninnet.FileQuery**.
