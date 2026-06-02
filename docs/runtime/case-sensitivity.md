# Case Sensitivity

Pattern matching in Jeninnet.FileQuery supports three case sensitivity modes, controlled by `FileQueryOptions.CaseSensitivity`.

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

When `CaseSensitivity.PlatformDefault` is used, the engine detects the operating system at runtime and applies the conventional behaviour:

| OS | Resolved mode |
|----|--------------|
| Linux | `Sensitive` |
| Windows | `Insensitive` |
| macOS | `Insensitive` |

This matches the default behaviour of the underlying filesystem on each platform. A pattern written on Windows will behave consistently when the same code runs on Linux — as long as the explicit `Sensitive` or `Insensitive` mode is set.

> **Cross-platform recommendation:** For applications that must behave identically on all platforms, set `CaseSensitivity.Sensitive` explicitly. Do not rely on `PlatformDefault` if portability matters.

---

## Setting Case Sensitivity

### Via the fluent API

```csharp
var query = FileQuery.From(root)
                     .Where("**", "!*.TXT")
                     .IgnoreCase()           // sets Insensitive
                     .Build();
```

```csharp
var query = FileQuery.From(root)
                     .Where("**", "!*.TXT")
                     .IgnoreCase(false)      // sets Sensitive
                     .Build();
```

### Via FileQueryOptions

```csharp
var options = new FileQueryOptions {
    PatternInput    = new(patterns: ["**", "!*.txt"]),
    CaseSensitivity = CaseSensitivity.Insensitive
};
```

---

## Effect on Each Matcher

**GitIgnoreMatcher and GlobMatcher**

Case sensitivity is passed as a `StringComparison` value to all segment comparisons. `CaseSensitivity.Insensitive` maps to `StringComparison.OrdinalIgnoreCase`; `Sensitive` maps to `StringComparison.Ordinal`.

**RegexInstructionMatcher**

When `Insensitive` is set, `RegexOptions.IgnoreCase` is added to the compiled `Regex`. The cache key includes the `CaseSensitivity` value, so a sensitive and an insensitive call to the same pattern each receive their own compiled `Regex` instance.

---

## Examples

```csharp
// Insensitive: matches FILE.TXT, file.txt, File.Txt
var options = new FileQueryOptions {
    PatternInput    = new(patterns: ["**", "!file.txt"]),
    CaseSensitivity = CaseSensitivity.Insensitive
};
```

```csharp
// Sensitive: only matches exactly "file.txt"
var options = new FileQueryOptions {
    PatternInput    = new(patterns: ["**", "!file.txt"]),
    CaseSensitivity = CaseSensitivity.Sensitive
};
```

```csharp
// Platform default: Insensitive on Windows/macOS, Sensitive on Linux
var options = new FileQueryOptions {
    PatternInput    = new(patterns: ["**", "!file.txt"]),
    CaseSensitivity = CaseSensitivity.PlatformDefault
};
```

---

## Path Normalization and Case

Path normalization (`PathUtilities.Normalize`) does not change the case of path characters — it only converts separators. Drive letter prefixes are uppercased on Windows (`c:\` → `C:/`), but directory and file name characters are left as-is. Case folding is the exclusive responsibility of the matcher, controlled by the `CaseSensitivity` setting.