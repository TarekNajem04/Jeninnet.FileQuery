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

**Deterministic.** The same patterns applied to the same directory always produce the same result. Within each dialect, rules are evaluated top to bottom and the last matching rule wins; across dialects, the fixed order is GitIgnore → Glob → Regex with the first Include winning.

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
| `Jeninnet.FileQuery.FileProviders`       | Query `Microsoft.Extensions.FileProviders.IFileProvider` sources.               |
| `Jeninnet.FileQuery.Tooling`             | Pattern classification, validation, and tokenization without execution.        |
| `Jeninnet.FileQuery.Tool`                | `dotnet-filequery` global tool.                                                 |
| `Jeninnet.FileQuery.Build`               | MSBuild `FileQueryPatterns` validation and SARIF export.                        |
| `Jeninnet.FileQuery.Analyzers`           | Roslyn `JNQ1001` diagnostic and code fix.                                       |

## Target Framework

All runtime/tool packages target `net10.0` and require .NET 10 or later. The Roslyn analyzer and
code-fix package target `netstandard2.0` so they can load in the Roslyn host.

## License

MIT. See the License file in the root directory for the full text.