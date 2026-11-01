# Jeninnet.FileQuery.Tooling

Build-tooling SDK for [Jeninnet.FileQuery](../Jeninnet.FileQuery/README.md). It exposes the
engine's pattern classification, structural validation, and tokenization without executing a
query, so build systems, linters, and IDE plugins can reuse the same semantics as the engine.

The package is a stable facade: it returns only SDK-defined models, so the engine internals remain
free to change.

## Installation

```bash
dotnet add package Jeninnet.FileQuery.Tooling
```

## Quick start

```csharp
using Jeninnet.FileQuery.Tooling;

var inspection = PatternInspector.Inspect("!src/**/*.cs");

Console.WriteLine(inspection.Kind);                 // GitIgnore
Console.WriteLine(inspection.IsNegated);            // True
Console.WriteLine(inspection.SegmentCount);         // 3
Console.WriteLine(inspection.Validation.IsValid);   // True

foreach (var segment in inspection.Segments)
{
    foreach (var token in segment.Tokens)
    {
        Console.WriteLine($"{token.Kind}: {token.Value}");
    }
}
```

## API

| Member | Result |
|---|---|
| `PatternInspector.Classify(string)` | `PatternKind` (`GitIgnore`, `Glob`, `Regex`, or `Unknown` when malformed) |
| `PatternInspector.Validate(string)` | `PatternValidation` (`IsValid`, `Error`) |
| `PatternInspector.Inspect(string)` | `PatternInspection` using the automatically classified dialect |
| `PatternInspector.Inspect(string, PatternKind)` | `PatternInspection` using a forced dialect |

`PatternInspection` exposes the structural flags (`IsNegated`, `IsRootAnchored`, `IsDirectoryOnly`,
`HasRecursiveWildcard`, `HasCharacterClass`, `HasEscapedCharacters`, ...), the validation result, and
the tokenized segments.

| Model | Purpose |
|---|---|
| `PatternInspection` | Complete inspection result |
| `PatternSegmentInfo` | One slash-separated segment and its tokens |
| `PatternTokenInfo` | One atomic token (`Kind`, `Value`) |
| `PatternTokenKind` | `Literal`, `Wildcard`, `RecursiveWildcard`, `SingleCharacterWildcard`, `CharacterClass`, `RegularExpression`, `EscapedCharacter`, `Unknown` |
| `PatternValidation` | Structural validation result |

## Token notes

- Character classes are rendered back from the parsed elements, e.g. `[!a-z0-9]` or
  `[[:digit:]]`.
- Escaped characters are currently resolved to literal tokens (`\*` becomes the literal `*`);
  use `HasEscapedCharacters` to detect that escaping was present.
- A forced `Regex` dialect expects the `r:` prefix. Without it the text is tokenized as literals;
  the command-line tool adds the prefix automatically, this SDK reports the pattern as supplied.

## Scope

This package is read-only inspection. Matching files, traversal, and query execution belong to the
core engine. The surface is intentionally small; new members are additive and follow SemVer.

## License

MIT — see [`LICENSE`](../../LICENSE).
