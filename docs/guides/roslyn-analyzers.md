# Roslyn Analyzer for Pattern Validation

`Jeninnet.FileQuery.Analyzers` validates compile-time pattern literals passed to
`FileQueryBuilder.Where` and offers safe code fixes. It catches structural mistakes such as an
unclosed character class before the query reaches the pattern compiler.

---

## Installation

```xml
<PackageReference Include="Jeninnet.FileQuery.Analyzers" Version="2.0.0" PrivateAssets="all" />
```

The package contains an analyzer and a code-fix assembly; no runtime dependency is added to your
application.

---

## Rules

| Rule | Severity | Enabled | Description |
|---|---|---|---|
| `JNQ1001` | Warning | Yes | A pattern literal is structurally malformed. |

`JNQ1001` mirrors the structural checks of `PatternInspector.Validate` in the Tooling SDK:
trailing unescaped backslashes, missing or empty bracket expressions, nested brackets, invalid
character ranges, and nonsensical double-dash ranges.

Configure the severity in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.JNQ1001.severity = error
```

---

## Example

```csharp
var query = FileQuery.From(root)
    .UsingGitIgnore()
    .Where(PatternKind.GitIgnore, ["**/*.cs", "!bin/", "src/[a-z.cs"]); // JNQ1001
```

The diagnostic points at the offending literal. When the only problem is a missing closing
bracket and appending `]` produces a valid expression, the code fix **Close the character class
with ']'** rewrites the literal:

```csharp
.Where(PatternKind.GitIgnore, ["src/[a-z].cs"])
```

No fix is offered when the rewrite would be unsafe (for example, a bare `[` at the end of the
literal, which would become the invalid empty expression `[]`).

The analyzer also understands the delimited overload:

```csharp
builder.Where("*.cs;[a-]"); // JNQ1001 on the [a-] part
```

---

## Limits

- Only compile-time constant string literals are analyzed; variables and computed values are
  ignored to avoid false positives.
- Only `FileQueryBuilder.Where` overloads are inspected.
- The analyzer performs the same structural checks as the Tooling SDK, not full semantic
  compilation. A valid literal can still fail later if it uses an unsupported dialect feature.
- The analyzer targets `netstandard2.0` so it can load in the compiler process. It therefore
  cannot reference the `net10.0` Tooling SDK assembly directly and carries an equivalent copy of
  the structural rules.
