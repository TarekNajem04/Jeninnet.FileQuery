# Jeninnet.FileQuery

![GitHub Actions CI Workflow Status](https://img.shields.io/github/actions/workflow/status/TarekNajem04/Jeninnet.FileQuery/ci.yml)
![GitHub release](https://img.shields.io/github/v/release/TarekNajem04/Jeninnet.FileQuery)
![GitHub contributors](https://img.shields.io/github/contributors/TarekNajem04/Jeninnet.FileQuery)
![GitHub forks](https://img.shields.io/github/forks/TarekNajem04/Jeninnet.FileQuery)
![GitHub last commit](https://img.shields.io/github/last-commit/TarekNajem04/Jeninnet.FileQuery)
![GitHub Issues](https://img.shields.io/github/issues/TarekNajem04/Jeninnet.FileQuery)
[![GitHub stars](https://img.shields.io/github/stars/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/stargazers)
[![GitHub license](https://img.shields.io/github/license/TarekNajem04/Jeninnet.FileQuery)](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)

![NuGet Version](https://img.shields.io/nuget/v/Jeninnet.FileQuery)
[![NuGet downloads](https://img.shields.io/nuget/dt/Jeninnet.FileQuery)](https://www.nuget.org/packages/Jeninnet.FileQuery/)

Absolutely — here is the **entire unified README**, rewritten **fully in English**, **cleanly structured**, and placed **inside a single fenced code block** exactly as you requested.

**Jeninnet.FileQuery** is a high‑performance, deterministic, pattern‑driven file enumeration engine for .NET.

It supports GitIgnore, Glob, and flat Regex (`r:`) patterns, powered by a strict tokenization + invariant validation pipeline designed for correctness, scalability, and predictable behavior across multi‑million file trees.

---

# ✨ Overview

Jeninnet.FileQuery is not just a file filter — it is a **pattern execution engine** with:

- Deterministic evaluation  
- Zero‑ambiguity parsing rules  
- Strict separation between scanning, validation, and execution  
- Scalable traversal for extremely large directory trees  
- Zero‑allocation hot paths using `ReadOnlySpan<char>`  

---

# 🚀 Key Features

- Ultra‑fast file enumeration (1M–2M files/sec)
- GitIgnore‑compatible pattern engine
- Glob pattern support
- Flat Regex mode (`r:` prefix)
- Hybrid auto‑detection
- Deterministic include/exclude precedence
- Sync + async APIs
- Safe traversal (ignore inaccessible directories)
- Full diagnostics + invariant validation
- Immutable compiled pattern sets

---

# 📦 Installation

```bash
dotnet add package Jeninnet.FileQuery
```

---

# ⚡ Quick Start

## Enumerate all `.cs` files

```csharp
var engine = new FileQueryEngine();

var query = new FileQuery(
    rootPath: "C:/projects",
    options: new FileQueryEngineOptions {
        Patterns = new[] { "**/*.cs" }
    }
);

IEnumerable<string> results = engine.Execute(query);
```

## Async enumeration

```csharp
await foreach (var file in engine.ExecuteAsync(
    new FileQuery(
        rootPath: "/src",
        options: new FileQueryEngineOptions {
            Patterns = new[] { "**/*.md", "!bin/" }
        }
    ),
    cancellationToken
)) {
    Console.WriteLine(file);
}
```

---

# 🧩 Pattern Engines

## 1️⃣ GitIgnore Mode

Supports full GitIgnore semantics:

| Feature           | Example          | Meaning                  |
| ----------------- | ---------------- | ------------------------ |
| Wildcard          | `*`              | Match filename part      |
| Recursive         | `**`             | Match across directories |
| Directory-only    | `bin/`           | Match directories only   |
| Negation          | `!src/app.cs`    | Un-ignore a path         |
| Root anchoring    | `/src/*.cs`      | Match only from root     |
| Character classes | `[a-z]`          | Character ranges         |

Example:

```
*.cs
!Program.cs
bin/
```

---

## 2️⃣ Glob Mode

Filesystem-style matching:

| Syntax  | Meaning                   |
| ------- | ------------------------- |
| `*`     | Any characters except `/` |
| `?`     | Any single character      |
| `**`    | Recursive directories     |
| `[a-z]` | Character classes         |

Example:

```
**/*.txt
src/**/test?.md
```

---

## 3️⃣ Regex Mode (Flat)

Rules:

- Must start with `r:`
- No glob semantics
- Applies to the full normalized path
- Uses .NET Regex

Example:

```
r:^src\/.*\.cs$
```

---

## 4️⃣ Hybrid Mode

Automatic engine selection:

| Pattern           | Engine    |
| ----------------- | --------- |
| `!bin/`           | GitIgnore |
| `/src/*.cs`       | GitIgnore |
| `*.txt`           | Glob      |
| `src/**/test?.md` | Glob      |
| `r:^data\d+$`     | Regex     |

---

# 📊 Pattern Engine Comparison

| Feature | GitIgnore | Glob | Regex (Flat) |
|--------|-----------|-------|--------------|
| Recursive `**` | ✔ | ✔ | ✘ |
| `?` wildcard | ✔ | ✔ | ✘ |
| Character classes | ✔ | ✔ | ✔ |
| Negation `!` | ✔ | ✘ | ✘ |
| Root anchoring `/` | ✔ | ✘ | ✘ |
| Needs prefix | No | No | Yes (`r:`) |
| Complexity | Medium | Low | High |
| Best for | Git-like filtering | General file matching | Advanced constraints |

---

# 🧪 Advanced Examples

## 1️⃣ Exclude a folder but include a specific file inside it

```
bin/
!bin/tools/keep.dll
```

## 2️⃣ Match Markdown files everywhere except in `docs/`

```
**/*.md
!docs/**
```

## 3️⃣ Match JSON files starting with a lowercase letter

```
data/[a-z]*.json
```

## 4️⃣ Regex: match `.cs` files ending with a number

```
r:^src\/.*\d+\.cs$
```

## 5️⃣ Combine GitIgnore + Glob + Regex

```
!bin/
src/**/test?.cs
r:^tools\/.*\.ps1$
```

## 6️⃣ Limit recursion depth

```csharp
MaxRecursionDepth = 2
```

## 7️⃣ Ignore access errors

```csharp
IgnoreInaccessible = true
```

## 8️⃣ Match files inside alphabetic subfolders

```
src/[a-z]*/**/*.cs
```

## 9️⃣ Regex: match multiple extensions

```
r:^.*\.(cs|md|json)$
```

## 🔟 Match files inside numeric folders only

```
r:^logs\/\d+\/.*\.txt$
```

## 1️⃣1️⃣ Complex exclusion rules

```
**/*.cs
!**/obj/**
!**/bin/**
!**/temp/**
```

---

# ⚙️ FileQueryEngineOptions

| Option                | Default   | Description            |
| --------------------- | --------- | ---------------------- |
| Patterns              | empty     | Pattern list           |
| RecurseSubdirectories | true      | Enable recursion       |
| MaxRecursionDepth     | -1        | Unlimited              |
| IgnoreInaccessible    | true      | Skip permission errors |
| IgnoreCase            | platform  | Case sensitivity mode  |
| PatternMatchingMode   | GitIgnore | Default engine         |
| PatternInterpretation | Auto      | Hybrid detection       |

---

# 🧠 Architecture

```
Raw Pattern Text
        │
        ▼
PatternScanner (Tokenization only)
        │
        ▼
Invariant Validation Layer
        │
        ▼
CompiledPatternSet (Immutable)
        │
        ▼
HybridPathMatcher
     │
     ├── GitIgnoreMatcher
     ├── GlobMatcher
     └── RegexMatcher
```

---

# 📘 Appendix A — Pattern Grammar (Extended & Complete)

This appendix defines the **formal grammar** for all pattern engines supported by Jeninnet.FileQuery.

---

## A.1 General Structure

```
pattern        ::= gitignore | glob | regex
```

---

## A.2 GitIgnore Grammar

```
gitignore      ::= [negation] [root] segment ('/' segment)* [dironly]

negation       ::= '!' | ε
root           ::= '/' | ε
dironly        ::= '/' | ε
segment        ::= token+
```

### Tokens

```
token          ::= literal
                 | '*'
                 | '?'
                 | '**'
                 | charclass
```

### Character Classes

```
charclass      ::= '[' ['!'|'^'] classitem+ ']'
classitem      ::= literal | literal '-' literal
```

### GitIgnore Rules

- `**` may match across directories  
- `bin/` matches directories only  
- `/foo` anchors to root  
- `!pattern` negates  
- `.` and `..` forbidden  
- Escaping supported  

---

## A.3 Glob Grammar

```
glob           ::= [negation] segment ('/' segment)*
negation       ::= '!' | ε
segment        ::= token+
```

### Tokens

```
token          ::= literal | '*' | '?' | '**' | charclass
```

### Glob Rules

- `*` matches any chars except `/`
- `?` matches one char
- `**` matches directories
- No root anchoring
- No directory-only suffix

---

## A.4 Regex Grammar

```
regex          ::= 'r:' regextext
regextext      ::= .+
```

Rules:

- Must start with `r:`
- Applies to full normalized path
- Uses .NET Regex
- No glob semantics

---

## A.5 Hybrid Interpretation

```
if starts with "r:" → Regex
else if contains GitIgnore semantics → GitIgnore
else → Glob
```

---

## A.6 Token Grammar (Shared)

```
literal         ::= any character except: '*', '?', '[', ']', '/', '!'
escaped         ::= '\' literal
token           ::= literal | escaped | '*' | '?' | '**' | charclass
```

---

## A.7 Invalid Patterns

| Pattern | Reason |
|---------|--------|
| `a**b`  | `**` must be isolated |
| `[z-a]` | invalid range |
| `../foo` | forbidden |
| `./foo` | forbidden |
| `**a` | invalid |

---

## A.8 Grammar Summary

```
pattern        ::= gitignore | glob | regex

gitignore      ::= [negation] [root] segment ('/' segment)* [dironly]
glob           ::= [negation] segment ('/' segment)*
regex          ::= 'r:' regextext

segment        ::= token+
token          ::= literal | '*' | '?' | '**' | charclass
charclass      ::= '[' ['!'|'^'] classitem+ ']'
classitem      ::= literal | literal '-' literal
```

---

# ❓ FAQ (Extended)

### Does `*.*` work?
Yes.

### Can I un-ignore a file inside an ignored folder?
Yes.

### Is Regex mode using .NET Regex?
Yes.

### Can Hybrid Mode be disabled?
Yes.

### Can access errors be ignored?
Yes.

### Can multiple pattern types be mixed?
Yes.

### Are absolute paths allowed?
No.

### Are `.` or `..` allowed?
No.

### Can I get diagnostics for invalid patterns?
Yes.

### Does it scale?
Yes — multi‑million file trees.

---

# 📄 License

MIT License.
