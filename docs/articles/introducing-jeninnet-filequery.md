# Building a Deterministic File Query Engine for .NET

File discovery seems like a trivial problem—until it isn't.

Many tools depend on finding files: build systems, code analyzers, automation scripts, repository scanners, backup tools, and more. At first glance, the task looks simple: walk a directory tree and filter paths with a few patterns.

But once projects grow large, file discovery becomes surprisingly complex.

Pattern rules interact with each other.
Exclusions override inclusions.
Different pattern syntaxes behave differently.
Performance collapses when scanning massive directory trees.

After encountering these problems repeatedly in real-world projects, I decided to design a library that treats file discovery as a **first-class architectural problem**.

The result is **Jeninnet.FileQuery**.

---

# The Problem With Existing Approaches

Most file matching tools fall into one of three categories.

Some rely purely on **glob patterns**.
These are simple and familiar, but limited once rule sets grow complex.

Others use **regular expressions**.
Regex is powerful but difficult to read and maintain when describing filesystem rules.

Some systems use **GitIgnore-style semantics**, which introduce rule ordering and negation. These are expressive but often implemented in ways that are difficult to extend or combine with other pattern systems.

What is usually missing is a **deterministic rule model** and a clean separation between **filesystem traversal** and **pattern evaluation**.

Jeninnet.FileQuery was designed to address these issues.

---

# Deterministic Pattern Evaluation

The core rule of the engine is intentionally simple:

> Patterns are evaluated sequentially, and the last matching rule determines the final result.

If you have used `.gitignore` files, this model will feel familiar.

Example pattern list:

```
**
!*.log
important.log
```

These rules can be read almost like a sentence.

Include everything.
Exclude all `.log` files.
Include `important.log` again.

Because the rules are evaluated in order, the final result is always predictable.

This model allows complex filtering behavior without introducing confusing precedence rules.

---

# A Hybrid Pattern Engine

Different situations call for different pattern languages.

Sometimes glob patterns are perfect.
Sometimes regex is required.
Sometimes GitIgnore-style rules are the most readable.

Rather than forcing developers to choose one syntax, Jeninnet.FileQuery supports **all three simultaneously**.

The engine automatically detects the pattern type and routes it to the appropriate matcher.

For example:

```
**
!*.log
r:^temp_.*\.txt$
```

In this pattern list:

* GitIgnore rules handle inclusion and exclusion
* glob patterns match extensions
* regex handles advanced filtering

All rules run through the same deterministic evaluation pipeline.

---

# The HybridPathMatcher

The core of the engine is the **HybridPathMatcher**.

Instead of one monolithic matcher, the system uses specialized matchers for each pattern language:

* GitIgnore matcher
* Glob matcher
* Regex matcher

Patterns are classified during initialization and routed to the correct matcher.
The HybridPathMatcher coordinates them so the final result behaves as a unified rule system.

This architecture keeps each matcher small, efficient, and easy to reason about.

---

# Traversal Designed for Scale

In many applications, pattern matching is not the bottleneck.
Filesystem traversal is.

Large repositories can contain hundreds of thousands—or even millions—of files.

Jeninnet.FileQuery uses a **streaming traversal model**:

```
filesystem
   ↓
path discovered
   ↓
pattern evaluation
   ↓
result emitted
```

Paths are evaluated as soon as they are discovered rather than being accumulated in memory.

This approach keeps memory usage low and allows the engine to scale to very large directory trees.

---

# A Modern .NET Codebase

The project targets modern .NET development.

Technology stack:

```
.NET 10
C# 14
MSTest
```

The implementation emphasizes:

* performance-aware design
* minimal allocations
* cross-platform filesystem support
* deep XML documentation

Patterns are tokenized during initialization so matchers operate on structured tokens instead of raw strings. This significantly reduces overhead during traversal.

---

# A Small Example

Here is a minimal example using the library.

```csharp
var patterns =
[
    "**",
    "!*.log",
    "important.log"
];

var options = new PatternOptions(patterns);

var runtime = new FileQueryRuntime(options);

foreach (var file in runtime.Query("logs"))
{
    Console.WriteLine(file);
}
```

This query will:

* include everything
* exclude `.log` files
* include `important.log` again

The behavior is deterministic and easy to understand.

---

# Project Structure

The repository is organized into three packages:

| Package                                | Purpose              |
| -------------------------------------- | -------------------- |
| Jeninnet.FileQuery                     | Core engine          |
| Jeninnet.FileQuery.CommandLine         | CLI argument mapping |
| Jeninnet.FileQuery.DependencyInjection | DI integration       |

The core library remains lightweight and dependency-free.

---

# Why I Built This

File discovery is a foundational operation in many systems, yet it is often implemented as an afterthought.

Once rule sets grow complex, the behavior of many existing solutions becomes difficult to reason about.

The goal of Jeninnet.FileQuery is to provide a **deterministic, composable, and scalable file query engine** for modern .NET applications.

Instead of treating file matching as a utility function, the project treats it as a **well-defined architectural component**.

---

# Try It

If this problem sounds familiar to you, the easiest way to understand the project is simply to try it.

Explore the repository:

GitHub:
https://github.com/TarekNajem04/Jeninnet.FileQuery

Install from NuGet:

```
dotnet add package Jeninnet.FileQuery
```

The repository includes:

* sample programs
* architecture documentation
* a detailed technical whitepaper

---

# Feedback Welcome

The project is open source and contributions are welcome.

If you have ideas, questions, or use cases, feel free to open a discussion or issue.

I'm especially interested in hearing how developers handle large-scale filesystem querying in their own tools.
