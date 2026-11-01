# Case Sensitivity

Pattern matching supports three case-sensitivity modes, controlled by the fluent `IgnoreCase`
method or by `CaseSensitivity` in the options configuration.

---

## Modes

```csharp
public enum CaseSensitivity
{
    PlatformDefault,  // inferred from the OS at runtime
    Sensitive,        // exact character casing required
    Insensitive       // upper and lower case are equivalent
}
```

---

## PlatformDefault (the default)

When `PlatformDefault` is used, the engine resolves the conventional filesystem behavior:

| OS | Resolved mode |
|---|---|
| Linux | `Sensitive` |
| Windows | `Insensitive` |
| macOS | `Insensitive` |

> **Cross-platform recommendation:** set `IgnoreCase(false)` when the same behavior is required on
> every platform. Do not rely on `PlatformDefault` for portable output.

---

## Setting case sensitivity

```csharp
var insensitive = FileQuery.From(root)
    .Where("**", "!**/*.TXT")
    .IgnoreCase()          // Insensitive
    .Build();

var sensitive = FileQuery.From(root)
    .Where("**", "!**/*.TXT")
    .IgnoreCase(false)     // Sensitive
    .Build();
```

`IgnoreCase()` makes `!**/*.TXT` match `file.txt`, `File.TXT`, and `FILE.Txt` equally.

---

## What is folded

- Literal segments and character-class **literals** (`[a]`).
- Character-class **ranges** (`[a-z]` matches `A`–`Z`).
- Negated classes fold before negation (`[!a-z]` excludes both cases).

POSIX categories keep their category semantics: `[:upper:]` remains uppercase-only and
`[:lower:]` remains lowercase-only even when `IgnoreCase()` is enabled.

```csharp
var files = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/[a-z].txt"])
    .IgnoreCase()          // also matches A.txt, Z.txt
    .Execute();
```

---

## Effect on each matcher

**GitIgnore and Glob**

Case sensitivity is passed as a `StringComparison` to segment comparisons.
`Insensitive` maps to `StringComparison.OrdinalIgnoreCase`; `Sensitive` maps to
`StringComparison.Ordinal`. Character classes fold literals and ranges as described above.

**Regex**

When `Insensitive` is set, `RegexOptions.IgnoreCase` is added to the compiled `Regex`. The cache key
includes `CaseSensitivity`, so sensitive and insensitive calls to the same expression use separate
compiled instances.

---

## Related

- [Quick start](../getting-started/quick-start.md)
- [Glob patterns](../reference/glob-patterns.md)
- [Regex patterns](../reference/regex-patterns.md)
- [Pattern precedence](../reference/pattern-precedence.md)
