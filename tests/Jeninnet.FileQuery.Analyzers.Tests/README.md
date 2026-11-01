# Jeninnet.FileQuery.Analyzers.Tests

Tests for the Roslyn analyzer and code fix behind `JNQ1001`.

**7 tests.**

## What is covered

- Malformed character classes and trailing escapes in `FileQueryBuilder.Where` literals.
- Regular, verbatim, raw, and delimited string literals.
- Collection/array initializers of constant patterns.
- False-positive guards for computed or non-constant values.
- The **Close the character class** code fix.
- Diagnostic location and severity behavior.

## Run

```bash
dotnet test tests/Jeninnet.FileQuery.Analyzers.Tests/Jeninnet.FileQuery.Analyzers.Tests.csproj -c Release
```

## Test host

`AnalyzerTestHost` builds an in-memory Roslyn compilation, runs the analyzer, and applies code
fixes. The tests never invoke the compiler on the repository itself, so they are fast and
deterministic.

## Design notes

- Analyzer and code fix target `netstandard2.0`.
- The test project references both analyzer assemblies and the core project.
- Assertions use the external `Jeninnet.Assertions` package.
- No MSBuild or IDE instance is required.

## Related

- [Roslyn analyzer guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/roslyn-analyzers.md)
- [Analyzer package](../../src/Jeninnet.FileQuery.Analyzers/README.md)
- [Code-fix assembly](../../src/Jeninnet.FileQuery.Analyzers.CodeFix/README.md)
- [All tests](../README.md)
