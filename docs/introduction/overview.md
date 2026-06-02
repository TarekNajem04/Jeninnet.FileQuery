# Overview

**Jeninnet.FileQuery** is a high-performance file system querying library for .NET 10. It provides a deterministic, composable, and allocation-efficient engine for discovering files based on pattern rules.

## What It Solves

File discovery is a foundational task in build systems, code analyzers, backup tools, repository scanners, and automation pipelines. Despite appearing simple, it becomes complex once rule sets grow beyond a few patterns:

- Pattern languages behave inconsistently across libraries
- Rule ordering is ambiguous when exclusions and inclusions interact
- Different pattern syntaxes (glob, regex, GitIgnore) cannot easily coexist
- Traversal performance collapses on large directory trees

Jeninnet.FileQuery addresses all of these by treating file discovery as a first-class architectural concern.

## Core Principles

**Deterministic.** The same patterns applied to the same directory always produce the same result. There is no internal priority system to reason about — patterns are a list, evaluated top to bottom, and the last matching rule wins.

**Composable.** GitIgnore, Glob, and Regex patterns can appear in the same rule set. The engine classifies each pattern and routes it to the appropriate matcher automatically.

**Zero-allocation hot path.** Pattern evaluation produces no heap garbage per file. GC pressure scales only with the number of file path strings returned to the caller.

**Streaming.** Results are emitted as paths are discovered. No full-tree buffering occurs, so queries on trees containing millions of files remain memory-efficient.

**Cross-platform.** Path normalization handles Windows backslashes, forward slashes, UNC paths (`\\server\share`), and platform default case sensitivity transparently.

## Packages

| Package                                  | Purpose                                                                         |
| ---------------------------------------- | ------------------------------------------------------------------------------- |
| `Jeninnet.FileQuery`                     | Core engine. No external dependencies.                                          |
| `Jeninnet.FileQuery.CommandLine`         | CLI argument mapping via `System.CommandLine`.                                  |
| `Jeninnet.FileQuery.DependencyInjection` | `IFileQueryEngine` registration for `Microsoft.Extensions.DependencyInjection`. |

## Target Framework

All packages target `net10.0`. Requires .NET 10 or later.

## License

MIT. See the License file in the root directory for the full text.