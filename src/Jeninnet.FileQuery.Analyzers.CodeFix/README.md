# Jeninnet.FileQuery.Analyzers.CodeFix

Code-fix assembly for `JNQ1001` (malformed file-query pattern literal). It is built separately
from the analyzer because code fixes require the Roslyn Workspaces libraries, while the analyzer
itself must remain loadable in the compiler process without them.

This project is **not published as its own package**. The
[`Jeninnet.FileQuery.Analyzers`](../Jeninnet.FileQuery.Analyzers/README.md) package packs both
assemblies under `analyzers/dotnet/cs/`.

## What it does

- Offers **Close the character class with ']'** when a pattern has an unterminated `[`.
- Applies the fix only when appending `]` produces a structurally valid pattern.
- Preserves the original string literal style (regular, verbatim, or raw).
- Does not attempt to repair ambiguous patterns or guess missing content.

```csharp
// Diagnostic
.Where(PatternKind.Glob, ["src/[abc"]);

// Fix result
.Where(PatternKind.Glob, ["src/[abc]"]);
```

## Build and test

```bash
dotnet build src/Jeninnet.FileQuery.Analyzers.CodeFix/Jeninnet.FileQuery.Analyzers.CodeFix.csproj -c Release
dotnet test tests/Jeninnet.FileQuery.Analyzers.Tests/Jeninnet.FileQuery.Analyzers.Tests.csproj -c Release
```

The code-fix tests live in `tests/Jeninnet.FileQuery.Analyzers.Tests` and use the Roslyn testing
host shared by the analyzer suite.

## Design notes

- Targets `netstandard2.0` for Visual Studio and `dotnet build` compatibility.
- Uses `Microsoft.CodeAnalysis.CSharp.Workspaces` only.
- No reflection and no runtime dependency on `Jeninnet.FileQuery`.
- Kept in a separate assembly to satisfy `RS1038` and avoid loading Workspaces into the compiler.

See the [Roslyn analyzer guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/roslyn-analyzers.md)
for the diagnostic contract and severity configuration.
