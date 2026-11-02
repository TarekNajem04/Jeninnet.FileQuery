# Documentation Directory

Source for the published documentation at
[tareknajem04.github.io/Jeninnet.FileQuery](https://tareknajem04.github.io/Jeninnet.FileQuery/).
The site is built with DocFX from this directory.

## Start here

- [Documentation home](./index.md)
- [Installation](./getting-started/installation.md)
- [Quick start](./getting-started/quick-start.md)
- [Basic patterns](./getting-started/basic-patterns.md)
- [Common recipes](./getting-started/common-recipes.md)

## Guides

- [Git ignore files](./guides/gitignore-files.md)
- [Entry metadata and filters](./guides/entry-metadata-and-filters.md)
- [Directory results](./guides/directory-results.md)
- [Multiple roots](./guides/multiple-roots.md)
- [In-memory matching](./guides/in-memory-matching.md)
- [File provider adapter](./guides/file-provider-adapter.md)
- [Brace expansion](./guides/brace-expansion.md)
- [Diff-aware queries](./guides/diff-aware-queries.md)
- [Dependency injection](./guides/dependency-injection.md)
- [Observability and diagnostics](./guides/observability-and-diagnostics.md)
- [Error handling and validation](./guides/error-handling-and-validation.md)
- [Global tool](./guides/global-tool.md)
- [CLI usage](./guides/cli-usage.md)
- [JSON output](./guides/json-output.md)
- [Roslyn analyzer](./guides/roslyn-analyzers.md)
- [MSBuild validation and SARIF](./guides/msbuild-validation.md)
- [Tooling SDK](./guides/tooling-sdk.md)
- [Native AOT](./guides/native-aot.md)
- [Ecosystem integrations](./guides/integrations.md)
- [Jeninnet FileQuery for VS Code](./guides/ide-extension.md)

## Reference

- [Pattern overview](./reference/pattern-overview.md)
- [GitIgnore patterns](./reference/gitignore-patterns.md)
- [Glob patterns](./reference/glob-patterns.md)
- [Regex patterns](./reference/regex-patterns.md)
- [Pattern precedence](./reference/pattern-precedence.md)
- [Pattern semantics](./reference/pattern-semantics.md)
- [Pattern language specification](./specification/pattern-language.md)

## Runtime and architecture

- [FileQuery runtime](./runtime/filequery-runtime.md)
- [Traversal model](./runtime/traversal-model.md)
- [Case sensitivity](./runtime/case-sensitivity.md)
- [Recursion depth](./runtime/recursion-depth.md)
- [Engine architecture](./architecture/engine-architecture.md)
- [Hybrid matcher](./architecture/hybrid-matcher.md)
- [Matcher invariants](./architecture/matcher-invariants.md)
- [Public API options](./architecture/public-api-options.md)

## Performance

- [Design for performance](./performance/design-for-performance.md)
- [Allocation strategy](./performance/allocation-strategy.md)
- [Benchmarking](./performance/benchmarking.md)
- [Cross-platform baseline](./performance/cross-platform-baseline.md)
- [Long-running stability](./performance/long-running-stability.md)
- [Runtime evolution](./performance/runtime-evolution.md)
- [Native enumeration research](./performance/native-enumeration-research.md)

## Articles

- [Introducing Jeninnet.FileQuery](./articles/introducing-jeninnet-filequery.md)
- [Library capability review](./articles/library-capability-review.md)

## Contributing

- [Contributing guide](./contributing/contributing-guide.md)
- [Project structure](./contributing/project-structure.md)
- [Testing](./contributing/testing.md)
- [Testing and release plan](./contributing/testing-and-release-plan.md)
- [Release checklist](./contributing/release-checklist.md)

## Building the docs

```bash
dotnet tool restore
dotnet docfx docs/docfx.json --warningsAsErrors
```

The generated site is written to `docs/_site`. The docs workflow also runs:

- `scripts/docs/check-doc-links.ps1`
- `scripts/docs/check-readme-claims.ps1`
- `scripts/docs/check-api-reference.ps1`
