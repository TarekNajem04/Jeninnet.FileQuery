# Jeninnet.FileQuery.Tooling.Tests

Tests for the Tooling SDK: pattern classification, structural validation, inspection flags,
tokenization, and SARIF projection.

**22 tests.**

## What is covered

- `PatternInspector.Classify` across GitIgnore, Glob, Regex, and malformed input.
- `PatternInspector.Validate` reasons and positions.
- `PatternInspector.Inspect` flags (`IsNegated`, `IsRootAnchored`, `IsDirectoryOnly`,
  `HasRecursiveWildcard`, `HasCharacterClass`, `HasEscapedCharacters`, `HasBraceAlternation`).
- Segment/token projection.
- `SarifExporter` documents, locations, empty results, and URI escaping.

## Run

```bash
dotnet test tests/Jeninnet.FileQuery.Tooling.Tests/Jeninnet.FileQuery.Tooling.Tests.csproj -c Release
```

## Design notes

- The SDK returns its own models; tests lock that contract so engine internals can change.
- No filesystem traversal is performed by the SDK or its tests.
- Assertions use the external `Jeninnet.Assertions` package.
- SARIF assertions parse the produced JSON rather than comparing strings.

## Related

- [Tooling SDK guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/tooling-sdk.md)
- [Tooling package](../../src/Jeninnet.FileQuery.Tooling/README.md)
- [All tests](../README.md)
