# Regex Patterns

Regular expression patterns give you the full power of the .NET `System.Text.RegularExpressions.Regex` engine for advanced path matching.

---

## Syntax

Prefix any pattern with `r:` to declare it as a regular expression:

```
r:^src/.*\.cs$
r:^data_\d{4}\.log$
r:^(?!.*\.Test).*\.dll$
```

The expression after `r:` is matched against the **full normalized path** using forward slashes as separators. The entire path must match (the expression is not automatically anchored — add `^` and `$` if you need to anchor).

---

## Path Format

Patterns are evaluated against root-relative paths with forward slashes:

```
src/engine/FileQueryEngine.cs
docs/README.md
bin/Release/net10.0/Jeninnet.FileQuery.dll
```

On Windows, drive letters and UNC prefixes are stripped by the root-relative calculation.

---

## Case Sensitivity

By default, regex matching respects the case-sensitivity setting on the `FileQueryOptions`:

```csharp
var options = new FileQueryOptions {
    PatternInput      = new(patterns: ["r:^src/.*\\.CS$"]),
    CaseSensitivity   = CaseSensitivity.Insensitive  // matches .cs, .CS, .Cs
};
```

The `RegexOptions.IgnoreCase` flag is applied automatically when `CaseSensitivity.Insensitive` is set.

---

## Caching

Compiled `Regex` instances are cached by `(pattern text, case sensitivity)`. Two calls with the same expression and the same case sensitivity setting share one compiled `Regex` — no repeated JIT compilation occurs.

---

## Common Examples

```
# Match any file whose name contains a version number (e.g. library-1.2.3.dll)
r:\d+\.\d+\.\d+

# Match files in a release directory named with a date (YYYY-MM-DD)
r:^releases/\d{4}-\d{2}-\d{2}/

# Match .cs files that are NOT test files
r:^(?!.*\.Tests\.).*\.cs$

# Match log files created on a specific date
r:^logs/2024-04-01_.*\.log$

# Match any file whose path does not contain "temp"
r:^(?!.*temp).*$
```

---

## Selecting Regex Mode

For a query that uses only Regex patterns:

```csharp
var query = FileQuery.From(root)
                     .UsingRegex()
                     .Where("r:^src/.*\\.cs$")
                     .Build();
```

In `Hybrid` mode (the default), any pattern prefixed with `r:` is automatically classified as Regex and routed to the `RegexInstructionMatcher`.

---

## Limitations

- Regex patterns do not support GitIgnore negation (`!`). In Hybrid mode, combine a Regex inclusion pattern with a GitIgnore exclusion pattern.
- Regex patterns do not respect directory-only semantics — they always match against the full path string.
- Very complex regex patterns with catastrophic backtracking can significantly slow down large directory traversals. Keep expressions simple.