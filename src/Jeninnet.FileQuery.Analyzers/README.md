# Jeninnet.FileQuery.Analyzers

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery.Analyzers)
![NuGet Downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery.Analyzers?label=Jeninnet.FileQuery.Analyzers)

Design-time validation for `Jeninnet.FileQuery` pattern literals. Catch malformed GitIgnore,
Glob, and Regex patterns while you type instead of discovering them at runtime.

```xml
<PackageReference Include="Jeninnet.FileQuery.Analyzers" Version="2.0.0" PrivateAssets="all" />
```

The package adds the analyzer and its code fix under `analyzers/dotnet/cs`; it has no runtime
reference and does not ship the engine.

---

## What it reports

`JNQ1001` — malformed file-query pattern literal:

```csharp
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;

var query = FileQuery.From(root)
    .Where(PatternKind.GitIgnore, ["src/[abc"]); // JNQ1001: missing closing bracket
```

The analyzer inspects constant strings passed to `FileQueryBuilder.Where`, including:

- plain string literals;
- verbatim and raw string literals;
- collection expressions and array initializers of constant strings;
- delimited pattern strings where the malformed segment can be identified.

It intentionally ignores computed values, interpolated strings, and patterns that cannot be
determined at compile time.

## Code fix

When the only problem is a missing closing `]`, the code fix offers **Close the character class
with ']'**. It only appends the bracket when the resulting literal is structurally valid, so it
never "fixes" a pattern by guessing intent.

```csharp
// Before
.Where(PatternKind.Glob, ["src/[abc"]);

// After the code fix
.Where(PatternKind.Glob, ["src/[abc]"]);
```

## Package layout

```text
analyzers/dotnet/cs/
├── Jeninnet.FileQuery.Analyzers.dll
└── Jeninnet.FileQuery.Analyzers.CodeFix.dll
```

The code-fix assembly is separate because Roslyn code-fix providers require the Workspaces
libraries, which must not be loaded into the compiler process that runs the analyzer.

## Severity and suppression

The diagnostic is a **warning** by default and respects the normal `.editorconfig` severity
configuration:

```ini
dotnet_diagnostic.JNQ1001.severity = error
```

Use the standard `#pragma warning disable JNQ1001` or `<NoWarn>JNQ1001</NoWarn>` when a dynamic
pattern is intentionally supplied as a constant that the analyzer cannot prove safe.

## Limits

- The analyzer mirrors the structural rules in the Tooling SDK, not the full runtime compiler.
  Semantic invariants such as parent traversal (`..`) or `**` placement are still enforced by
  `PatternException` at build/execute time.
- Only compile-time constant patterns are analyzed.
- The analyzer targets `netstandard2.0` for Roslyn compatibility.

## Related

- [Roslyn analyzer guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/roslyn-analyzers.md)
- [MSBuild validation and SARIF](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/msbuild-validation.md)
- [Core package](../Jeninnet.FileQuery/README.md)
