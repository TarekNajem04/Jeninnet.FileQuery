# Pattern Language Specification

`Jeninnet.FileQuery`

This document defines the pattern syntax supported by the **Jeninnet.FileQuery** engine.

Patterns are processed through the following pipeline:

```
PatternCanonicalizer
        ↓
PatternClassifier
        ↓
Tokenization (Patterns.Tokenization)
        ↓
Matcher selection
        ↓
HybridPathMatcher
```

The engine supports **three matcher types**:

```
GlobMatcher
GitIgnoreMatcher
RegexPathMatcher
```

The `HybridPathMatcher` automatically selects the appropriate matcher.

---

# Pattern Precedence Model

The engine follows **GitIgnore-style rule precedence**.

Rules are evaluated **in order**.

```
Last rule wins
```

Example:

```csharp
patterns: [
    "**",
    "!*.log",
    "data.log",
    "!data.log"
]
```

Result:

```
data.log → included
```

Because the **last rule overrides previous rules**.

---

# Pattern Types

`PatternClassifier` determines the matcher type.

## Regex Pattern

Regex patterns start with:

```
r:
```

Example:

```
r:^data.*\.log$
```

This pattern is executed using:

```
RegexPathMatcher
```

Example usage:

```csharp
patterns: [
    "**",
    "r:^data.*\\.log$"
]
```

---

## Glob Patterns

Glob patterns support typical filesystem wildcard syntax.

Matcher used:

```
GlobMatcher
```

Supported tokens:

```
*
**
?
[abc]
[a-z]
```

---

### Single Wildcard

```
*
```

Matches zero or more characters **within a path segment**.

Example:

```
*.txt
```

Matches:

```
file.txt
notes.txt
```

---

### Recursive Wildcard

```
**
```

Matches directories recursively.

Example:

```
**/*.cs
```

Matches:

```
Program.cs
src/app/main.cs
tests/unit/file.cs
```

---

### Single Character

```
?
```

Matches exactly one character.

Example:

```
a?.txt
```

Matches:

```
ab.txt
ac.txt
```

Does not match:

```
abc.txt
```

---

# Character Classes

Supported syntax:

```
[abc]
[a-z]
```

Example:

```
file[0-9].txt
```

Matches:

```
file1.txt
file2.txt
file9.txt
```

---

# Negation

Negation uses:

```
!
```

Example:

```
!*.txt
```

This excludes `.txt` files.

Example:

```csharp
patterns: [
    "**",
    "!*.txt"
]
```

Result:

```
all files except .txt
```

---

# Root Anchoring

Patterns starting with `/` are anchored to the root.

Example:

```
/file.txt
```

Matches:

```
root/file.txt
```

But not:

```
sub/file.txt
```

---

# Directory Patterns

Patterns ending with `/` match directories.

Example:

```
logs/
```

Matches:

```
logs/
logs/app/
logs/archive/
```

---

# Recursive Directory Rules

Examples from tests:

```
logs/**
logs/**/*.txt
```

Meaning:

```
logs/**        → everything under logs
logs/**/*.txt  → txt files anywhere under logs
```

---

# Mixed Inclusion Example

Example from tests:

```csharp
patterns: [
    "**",
    "!*.txt",
    "b.txt"
]
```

Evaluation:

```
include everything
exclude txt
re-include b.txt
```

Result:

```
b.txt only
```

---

# Complex Pattern Examples From Tests

The test suite contains advanced examples.

Example:

```
!**/foo/**/bar/*.json
```

Meaning:

```
Exclude JSON files in directories matching:
foo/**/bar
```

Example:

```
!a?c*/data/*.txt
```

Matches:

```
abc1/data/file.txt
axc/data/file.txt
```

---

# Case Sensitivity

Controlled through:

```
FileQueryOptions.CaseSensitivity
```

Example:

```csharp
CaseSensitivity = CaseSensitivity.Insensitive
```

This allows patterns like:

```
!Foo.TXT
```

to match:

```
foo.txt
```

---

# Pattern Canonicalization

Before classification, patterns are normalized by:

```
PatternCanonicalizer
```

Possible operations:

```
path normalization
separator normalization
trim
collapse redundant tokens
```

This guarantees consistent behavior across:

```
Windows
Linux
macOS
```

---

# Pattern Tokenization

Pattern parsing occurs in:

```
Jeninnet.FileQuery.Patterns.Tokenization
```

The tokenizer converts patterns into tokens such as:

```
WildcardToken
RecursiveWildcardToken
LiteralToken
CharacterClassToken
DirectorySeparatorToken
```

These tokens are used to build compiled matchers.

---

# Matcher Architecture

The runtime matcher:

```
HybridPathMatcher
```

Dispatch logic:

```
if pattern starts with "r:" → RegexPathMatcher
else if pattern contains glob tokens → GlobMatcher
else → GitIgnoreMatcher
```

Each matcher produces a **compiled path matcher** used during traversal.

---

# Example Usage

```csharp
var engine = FileQueryRuntime.Create();

var options = new FileQueryOptions
{
    PatternInput = new(
        patterns: [
            "**",
            "!*.log",
            "important.log"
        ]
    )
};

var result = engine.Execute(new(rootPath, options))
                   .ToList();
```

---

# Summary

The pattern language supports:

| Feature              | Supported |
| -------------------- | --------- |
| Glob wildcards       | ✓         |
| Recursive matching   | ✓         |
| Character classes    | ✓         |
| Ranges               | ✓         |
| Regex patterns       | ✓         |
| Negation rules       | ✓         |
| Root anchoring       | ✓         |
| Directory rules      | ✓         |
| Last-rule precedence | ✓         |
