# Build Tooling SDK

`Jeninnet.FileQuery.Tooling` exposes the pattern pipeline without executing a query. Build systems,
linters, and IDE plugins can use it to validate patterns at design time or to explain how a
pattern is structured, with exactly the same classification rules the engine uses at runtime.

The SDK is a facade: it returns only SDK-defined models, so internal token types can evolve
without breaking consumers.

---

## Installation

```bash
dotnet add package Jeninnet.FileQuery.Tooling
```

---

## Quick start

```csharp
using Jeninnet.FileQuery.Tooling;

var inspection = PatternInspector.Inspect("!src/**/*.cs");

if (!inspection.Validation.IsValid)
{
    Console.Error.WriteLine(inspection.Validation.Error);
    return;
}

Console.WriteLine(inspection.Kind);          // GitIgnore
Console.WriteLine(inspection.SegmentCount);  // 3

foreach (var segment in inspection.Segments)
{
    Console.WriteLine(string.Join(" | ", segment.Tokens.Select(t => $"{t.Kind}: {t.Value}")));
}
```

Output:

```text
GitIgnore
3
Literal: src
RecursiveWildcard: **
Wildcard: * | Literal: .cs
```

---

## Error positions

`PatternValidation` reports a zero-based `Position` and slash-separated `Segment` when the
structural validator can locate the error:

```csharp
var validation = PatternInspector.Validate("src/[abc");
// IsValid:  false
// Error:    "Missing closing bracket ']' for an opening bracket '['."
// Position: 4
// Segment:  1
```

The same data is carried by `PatternException.Position` and `PatternException.Segment` when a
pattern fails during compilation (for example, `..` traversal or a mixed `**` segment). The
properties are `null` when no position is available, so existing exception handling remains
source-compatible.

---

## API

| Member | Description |
|---|---|
| `PatternInspector.Classify(string)` | Returns `PatternKind.GitIgnore`, `Glob`, `Regex`, or `Unknown` for malformed input. |
| `PatternInspector.Validate(string)` | Returns `PatternValidation` with `IsValid`, the structural `Error` message, and `Position`/`Segment` when known. |
| `PatternInspector.Inspect(string)` | Full inspection using the automatically classified dialect. |
| `PatternInspector.Inspect(string, PatternKind)` | Full inspection using a forced dialect (`Unknown` is rejected). |

`PatternInspection` contains:

| Property | Meaning |
|---|---|
| `Kind` | The dialect used for tokenization |
| `IsRegex` | Whether the source carried the `r:` prefix |
| `IsNegated`, `IsRootAnchored`, `IsDirectoryOnly` | GitIgnore structural flags |
| `HasGitIgnoreSyntax`, `HasWildcard`, `HasRecursiveWildcard`, `HasCharacterClass`, `HasEscapedCharacters`, `HasBraceAlternation` | Structural signals |
| `SegmentCount`, `Segments` | Tokenized slash-separated segments |
| `Validation` | Structural validation result |

---

## Example: validate patterns in a build task

```csharp
using Jeninnet.FileQuery.Tooling;

public static bool ValidatePatterns(IEnumerable<string> patterns, out string? error)
{
    foreach (var pattern in patterns)
    {
        var validation = PatternInspector.Validate(pattern);

        if (!validation.IsValid)
        {
            error = $"'{pattern}': {validation.Error}";
            return false;
        }
    }

    error = null;
    return true;
}
```

Because inspection never touches the filesystem, it is safe to run during evaluation, in an
analyzer, or in a language server.

---

## Token model

| Kind | Value |
|---|---|
| `Literal` | The literal text; escaped characters are resolved into literals (`\*` → `*`). |
| `Wildcard` | `*` |
| `RecursiveWildcard` | `**` |
| `SingleCharacterWildcard` | `?` |
| `CharacterClass` | Rendered from the parsed elements, e.g. `[!a-z0-9]`, `[[:digit:]]` |
| `RegularExpression` | The expression without the `r:` prefix |
| `EscapedCharacter` | Reserved for tokenizers that preserve the distinction |
| `Unknown` | A token this SDK version does not recognize |

`HasEscapedCharacters` reports whether escaping was used even though the token is a literal.

---

## Compatibility

- The package follows SemVer and is versioned with the other Jeninnet.FileQuery packages.
- The surface is additive: new members and token kinds are added without breaking existing code.
- Forced `Regex` inspection expects the `r:` prefix; the SDK reports the pattern exactly as
  supplied. The command-line tool adds the prefix for convenience.
- No reflection, no filesystem access, no execution.

---

## See also

- [Command-Line Integration](cli-usage.md) — embedding pattern options in your own CLI.
- [Pattern precedence](../reference/pattern-precedence.md) — how the engine evaluates the dialects.
