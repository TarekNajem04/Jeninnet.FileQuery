# Pattern Classifier

High-Precision Syntactic Classifier for GitIgnore, Glob, Flat, and Unknown Patterns

---

## Overview

`PatternClassifier` is a syntactic classifier that parses pattern strings (such as: `*.txt`, `!src/`, `/bin/`, `[a-z].md`) and determines their type according to three categories:

* **GitIgnore**

* **Glob**

* **Flat**

* **Unknown**

> GitIgnore is treated as a superset of Glob syntax.

> Any pattern that is valid for both is considered a GitIgnore.

The classifier relies solely on syntactic parsing without any matching engine.

---

## Features

* Detects **GitIgnore-exclusive syntax**
*Detects **Glob-exclusive syntax**
* Fully supports **escaping rules**
* Detects **malformed POSIX ranges**
* Detects Windows paths (like `src\main\*.cs`)
* Custom user-defined **Flat syntax** via `"r:"` prefix
* Strict and deep syntactic validation
* Excellent XML documentation for IDE autocompletion

---

## Pattern Types

### 1. **GitIgnore**

Represents any pattern that conforms to the `.gitignore` rules, including:

* negation: `!pattern`
* root-anchoring: `/pattern`
* directory-only: `pattern/`
* comments: `# comment`
* escaping: `\!`, `\#`, `\*`, `\?`, `\[`, `\]`
* glob: `*`, `?`, `**`, `[ranges]`

> GitIgnore includes full glob syntax.

---

### 2. **Glob**

Represents patterns that fall under POSIX Glob but are **not valid** as GitIgnore.

Examples:

* Windows-style patterns without valid escapes

`src\main\*.cs`

* Stray bracket allowed in some glob engines:

`file].txt`

---

### 3. **Flat**

A custom pattern that starts with:

```
r:pattern
```

Interpreted outside the context of GitIgnore/Glob.

Example:

```
r:images/*.png
```

---

### 4. **Unknown**

Represents **grammatically incorrect** patterns:

Examples:

* bracket not closed: `[abc`
* empty set: `[]`
* invalid range: `[a-]`
* invalid double-range: `[--x]`

---

## How to Use

### Example

```csharp
var type = PatternClassifier.Classify("*.txt");

// type == PatternKind.GitIgnore
```

### Detecting all supported categories

```csharp
void Test(string p)
{
Console.WriteLine($"{p} → {PatternClassifier.Classify(p)}");
}

Test("*.txt");
Test("!build/");
Test("[abc]");
Test("r:raw-pattern");
Test("[unclosed");
```

---

## Pattern Recognition Reference

The following is a list of all the patterns that the classifier can recognize and classify:

---

## GitIgnore Patterns

### ✔ Patterns starting with `!`

```
!*.log
!src/**
```

### ✔ Patterns starting with `/`

```
/bin/
/assets/styles.css
```

### ✔ Patterns ending with `/`

```
logs/
static/
```

### ✔ Comments `#`

```
# this is a comment
```

### ✔ Escaped characters. characters

```
\!important.txt
\#not-comment.md
\[literal].txt
```

### ✔ GitIgnore glob syntax

```
*.txt
*.{jpg,png}
src/**/*.cs
a?b
docs/[a-z]*.md
```

---

## Glob Patterns (GitIgnore-incompatible)

### ✔ Windows-style paths (non-escaped)

```
src\main\*.cs
assets\images\?.png
```

### ✔ Stray closing bracket

```
file].txt
```

---

## Flat Patterns

### ✔ Prefixed with `r:`

```
r:^src/.*

```

---

## Unknown Patterns (Invalid)

### ❌ Missing closing bracket

```
[a-z
[file
[123
```

### ❌ Empty bracket

```
[]
```

### ❌ Invalid ranges

```
[a-]
[--x]
[-a]
```

### ❌ Mutually contradictory syntax

```
(just a single backslash)
[*]
```

---

## Classification Rules Summary

```
GitIgnore > Glob > Flat > Unknown
```

**GitIgnore wins** Whenever pattern can be interpreted as GitIgnore.

---

## Example Table

| Pattern | Classification | Reason |
| --------------- | -------------- | ------------------------------------- |
| `*.txt` | GitIgnore | Valid in both → GitIgnore wins |
| `!cache/` | GitIgnore | GitIgnore-only negation |
| `/bin/` | GitIgnore | Root and directory-only |
| `src/**/*.cs` | GitIgnore | Glob syntax fully supported |
| `src\main\*.cs` | Glob | Windows path not allowed in GitIgnore |
| `[abc]` | GitIgnore | Valid POSIX class |
| `[a-z` | Unknown | Malformed range |
| `r:assets/png` | Flat | Flat prefix |
| `foo` | GitIgnore | Literal valid in both |

---