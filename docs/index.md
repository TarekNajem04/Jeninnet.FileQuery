# Jeninnet.FileQuery Documentation

**Deterministic file discovery for .NET 10.** One compiled pipeline for GitIgnore, Glob, and Regex
patterns, with metadata results, directory output, `.gitignore` ingestion, cancellation, progress,
and diagnostics.

Start with a five-minute query, then follow the path that matches your use case.

---

## Start here

| I want to… | Read |
|---|---|
| Install the package and run my first query | [Installation](getting-started/installation.md) → [Quick start](getting-started/quick-start.md) |
| Learn the pattern language | [Basic patterns](getting-started/basic-patterns.md) → [Pattern overview](reference/pattern-overview.md) |
| Copy a ready-made query | [Common recipes](getting-started/common-recipes.md) |
| Understand files, sizes, and timestamps | [Entry metadata and filters](guides/entry-metadata-and-filters.md) |
| Respect repository ignore files | [Git ignore files](guides/gitignore-files.md) |
| Query an ASP.NET Core `IFileProvider` | [File provider adapter](guides/file-provider-adapter.md) |
| Use the terminal tool | [Global tool](guides/global-tool.md) → [CLI usage](guides/cli-usage.md) |
| Validate patterns in CI | [Roslyn analyzer](guides/roslyn-analyzers.md) → [MSBuild validation](guides/msbuild-validation.md) |
| Understand an unexpected include/exclude | [Observability and diagnostics](guides/observability-and-diagnostics.md) |
| Handle errors and cancellation | [Error handling and validation](guides/error-handling-and-validation.md) |

---

## What the library gives you

- **Three dialects, one pipeline.** GitIgnore, Glob, and Regex rules share deterministic precedence.
- **One pass.** Matching, metadata filters, directory results, and diagnostics happen during the
  same traversal.
- **Predictable output.** The same tree, patterns, and options produce the same order every time.
- **Production controls.** Sync/async execution, cancellation, progress, error recovery, and
  OpenTelemetry-compatible instrumentation.
- **Ecosystem packages.** CLI, DI, `IFileProvider`, Tooling SDK, Roslyn analyzer, MSBuild/SARIF, and
  a global tool.
- **AOT-ready.** No reflection on the query path; a Native AOT smoke host runs in CI.

## Packages

| Package | Use it for |
|---|---|
| `Jeninnet.FileQuery` | Core engine |
| `Jeninnet.FileQuery.CommandLine` | `System.CommandLine` pattern options |
| `Jeninnet.FileQuery.DependencyInjection` | `AddFileQuery()` composition |
| `Jeninnet.FileQuery.FileProviders` | `IFileProvider` sources |
| `Jeninnet.FileQuery.Tooling` | Pattern inspection without execution |
| `Jeninnet.FileQuery.Analyzers` | Design-time `JNQ1001` diagnostics |
| `Jeninnet.FileQuery.Build` | MSBuild validation and SARIF |
| `Jeninnet.FileQuery.Tool` | `dotnet-filequery` global tool |

---

## How it works

```text
builder
  → pattern classification
  → structural validation
  → immutable compiled pattern set
  → traversal plan
  → single-pass enumeration
      → metadata filters
      → compiled matcher
      → diagnostics / progress
      → path or FileQueryEntry
```

Read [Engine architecture](architecture/engine-architecture.md) for the full design, or
[Traversal model](runtime/traversal-model.md) for DFS/BFS, symlinks, and depth control.

---

## Explore by area

### Getting started
- [Installation](getting-started/installation.md)
- [Quick start](getting-started/quick-start.md)
- [Basic patterns](getting-started/basic-patterns.md)
- [Common recipes](getting-started/common-recipes.md)

### Guides
- [Git ignore files](guides/gitignore-files.md)
- [Entry metadata and filters](guides/entry-metadata-and-filters.md)
- [Directory results](guides/directory-results.md)
- [Multiple roots](guides/multiple-roots.md)
- [In-memory matching](guides/in-memory-matching.md)
- [File provider adapter](guides/file-provider-adapter.md)
- [Brace expansion](guides/brace-expansion.md)
- [Diff-aware queries](guides/diff-aware-queries.md)
- [Dependency injection](guides/dependency-injection.md)
- [Observability and diagnostics](guides/observability-and-diagnostics.md)
- [Error handling and validation](guides/error-handling-and-validation.md)
- [Global tool](guides/global-tool.md)
- [CLI usage](guides/cli-usage.md)
- [JSON output](guides/json-output.md)
- [Roslyn analyzer](guides/roslyn-analyzers.md)
- [MSBuild validation and SARIF](guides/msbuild-validation.md)
- [Tooling SDK](guides/tooling-sdk.md)
- [Native AOT](guides/native-aot.md)
- [Ecosystem integrations](guides/integrations.md)
- [Jeninnet FileQuery for VS Code](guides/ide-extension.md)

### Reference
- [Pattern overview](reference/pattern-overview.md)
- [GitIgnore patterns](reference/gitignore-patterns.md)
- [Glob patterns](reference/glob-patterns.md)
- [Regex patterns](reference/regex-patterns.md)
- [Pattern precedence](reference/pattern-precedence.md)
- [Pattern semantics](reference/pattern-semantics.md)
- [Pattern language specification](specification/pattern-language.md)

### Runtime and architecture
- [FileQuery runtime](runtime/filequery-runtime.md)
- [Traversal model](runtime/traversal-model.md)
- [Case sensitivity](runtime/case-sensitivity.md)
- [Recursion depth](runtime/recursion-depth.md)
- [Engine architecture](architecture/engine-architecture.md)
- [Hybrid matcher](architecture/hybrid-matcher.md)
- [Matcher invariants](architecture/matcher-invariants.md)

### Articles and performance
- [Introducing Jeninnet.FileQuery](articles/introducing-jeninnet-filequery.md)
- [Library capability review](articles/library-capability-review.md)
- [Performance documentation](performance/benchmarking.md)
- [Cross-platform baseline](performance/cross-platform-baseline.md)
- [Runtime evolution](performance/runtime-evolution.md)

---

## Samples and tests

- [Samples](https://github.com/TarekNajem04/Jeninnet.FileQuery/tree/main/samples) — 13 runnable
  examples from hello-world to DI + evaluation.
- [Test suite](https://github.com/TarekNajem04/Jeninnet.FileQuery/tree/main/tests) — 1,130 tests
  across seven projects.
- [Root README](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/README.md)

## Contributing

- [Contributing guide](contributing/contributing-guide.md)
- [Project structure](contributing/project-structure.md)
- [Testing](contributing/testing.md)
- [Release checklist](contributing/release-checklist.md)

## License

MIT. See [LICENSE](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE).
