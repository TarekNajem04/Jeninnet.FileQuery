# Jeninnet.FileQuery

**Jeninnet.FileQuery** is a fast, cross-platform, pattern-driven file enumeration engine for .NET.

It provides a **GitIgnore-compatible**, **Glob**, and **Regex (flat)** pattern engine built for **high performance**, **predictable behavior**, and **large directory trees**.

---

## ✨ Key Features

* 🚀 High-performance file enumeration (millions of files)
* 📂 Recursive directory traversal
* 🧩 Multiple pattern engines:

  * GitIgnore-style patterns
  * Glob patterns
  * Flat regular expressions
* ❌ Include / exclude rules with deterministic precedence
* 🔠 Case-sensitive or insensitive matching
* 🧠 Span-based pattern compilation
* 🔄 Fully synchronous **and** async APIs
* 🛡 Safe traversal (optional ignore inaccessible directories)

---

## Installation

```bash
dotnet add package Jeninnet.FileQuery
```

---

## Quick Start

### Enumerate all `.cs` files recursively

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

---

### Async enumeration

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

## Execution Model

All enumeration is driven by an immutable **query object**.

```csharp
public interface IFileQueryEngine {
    IEnumerable<string> Execute(FileQuery query);
    IAsyncEnumerable<string> ExecuteAsync(
        FileQuery query,
        CancellationToken cancellationToken = default
    );
}
```

### Why `FileQuery`?

* Groups root path and options into a single request
* Enables deterministic execution
* Keeps sync and async APIs symmetric
* Allows future diagnostics, caching, and replay

---

## FileQueryEngineOptions

```csharp
public class FileQueryEngineOptions {
    public string[] Patterns { get; set; } = Array.Empty<string>();
    public bool RecurseSubdirectories { get; set; } = true;
    public int MaxRecursionDepth { get; set; } = -1;
    public bool IgnoreInaccessible { get; set; } = true;
    public bool IgnoreCase { get; set; } = FileQueryEngineOptions.DefaultIgnoreCase;

    public PatternMatchingMode PatternMatchingMode { get; set; }
        = PatternMatchingMode.GitIgnore;

    public PatternInterpretationMode PatternInterpretation { get; set; }
        = PatternInterpretationMode.Auto;
}
```

---

## Pattern Engines

Jeninnet.FileQuery supports **three pattern engines**.

---

## 1️⃣ GitIgnore Mode

Full GitIgnore-style semantics.

### Supported syntax

| Feature           | Example          | Meaning                  |
| ----------------- | ---------------- | ------------------------ |
| Wildcard          | `*`              | Match filename part      |
| Recursive         | `**`             | Match across directories |
| Directory-only    | `bin/`           | Match directories only   |
| Negation          | `!src/app.cs`    | Un-ignore a path         |
| Root anchoring    | `/src/*.cs`      | Match only from root     |
| Character classes | `[a-z]`, `[0-9]` | Character ranges         |
| Escaping          | `\!file.txt`     | Literal character        |

### Example

```
*.cs
!Program.cs
bin/
```

**Meaning**

* Include all `.cs`
* Exclude `Program.cs`
* Exclude the `bin` directory entirely

---

## 2️⃣ Glob Mode

Simpler, filesystem-style matching.

### Syntax

| Syntax  | Meaning                   |
| ------- | ------------------------- |
| `*`     | Any characters except `/` |
| `?`     | Any single character      |
| `**`    | Recursive directories     |
| `[a-z]` | Character classes         |

### Example

```
**/*.txt
src/**/test?.md
```

---

## 3️⃣ Regex Mode (Flat)

Regex patterns are **explicit** and **flat**.

### Rules

* Must start with `r:`
* No glob or GitIgnore semantics
* Pattern applies to the **entire normalized path**
* Uses .NET `Regex`

### Example

```csharp
Patterns = new[] {
    @"r:^src\/.*\.cs$"
}
```

---

## Hybrid Interpretation Mode

Hybrid mode automatically selects the compiler:

| Pattern           | Engine    |
| ----------------- | --------- |
| `!bin/`           | GitIgnore |
| `/src/*.cs`       | GitIgnore |
| `*.txt`           | Glob      |
| `src/**/test?.md` | Glob      |
| `r:^data\d+$`     | Regex     |

---

## Pattern Processing Pipeline

```
Raw Pattern Text
        │
        ▼
PatternScanner
(tokenization only)
        │
        ▼
PatternInvariants
(structural + semantic validation)
        │
        ▼
CompiledPattern
(immutable)
        │
        ▼
PathMatcher
```

### Important guarantees

* `PatternScanner` **does not throw** for malformed input
* All errors are reported via **invariants**
* Regex is isolated from glob / GitIgnore logic
* No pattern matching happens during compilation

---

## Architecture Overview

```
┌────────────────────────┐
│     FileQueryEngine    │
│      (public API)      │
└──────────┬─────────────┘
           │ executes
           ▼
┌────────────────────────┐
│   HybridPathMatcher    │─────┐
└────────────────────────┘     │
           ▲                   │
           │ uses              │
     ┌─────┴─────────────┬─────┴───────────┐
     ▼                   ▼                 ▼
GitIgnoreMatcher   GlobMatcher        RegexMatcher
(pattern-based)    (pattern-based)    (flat regex)
     │                   │                 │
     └──────────┬────────┴─────────────────┘
                ▼
         CompiledPattern[]
                ▼
          PatternToken[]
```

---

## Performance Notes

* Uses `Directory.EnumerateFileSystemEntries`
* Avoids `FileInfo` / `DirectoryInfo`
* Span-based tokenization
* Regex only used in explicit regex mode

Typical throughput (example):

| Operation          | Files/sec |
| ------------------ | --------- |
| Enumeration only   | ~1.8M     |
| Glob matching      | ~1.2M     |
| GitIgnore matching | ~1.1M     |

---

## Testing

The project includes:

* Scanner architecture tests
* Tokenizer tests
* Invariant tests
* Matcher tests (Glob / GitIgnore / Regex)
* Case sensitivity tests
* Async enumeration tests

All tests must pass with:

* No `PatternException` thrown by the scanner
* All invalid syntax caught by invariants

---

## FAQ

### Does `*.*` work?

Yes. Same behavior as GitIgnore.

### Can I un-ignore a file inside an excluded folder?

Yes — same semantics as GitIgnore.

### Is this cross-platform?

Yes:

* `/` normalized internally
* OS-aware case sensitivity
* Override available

### Does it scale?

Yes. Designed for multi-million file trees.

---

## License

MIT License. See `LICENSE`.

Love this direction — those two sections are exactly what makes the engine *understandable* instead of “magic”.
Below are **two appendices** you can paste **directly at the end of the README**.

They are fully aligned with the **current scanner → tokenizer → invariant pipeline** and explain *why things are the way they are*, not just *what*.

---

# Appendix A — Pattern Grammar

This appendix describes the **formal grammar** of patterns accepted by Jeninnet.FileQuery.

The grammar is intentionally **strict**, **unambiguous**, and **engine-specific**.
It is *inspired by* GitIgnore and glob syntax, but not a byte-for-byte clone.

---

## A.1 Lexical Structure

All patterns are processed as **normalized forward-slash paths**.

```
pattern        := [prefix] core-pattern [suffix]
prefix         := '!' | '/' | ε
suffix         := '/' | ε
```

Whitespace surrounding the pattern is ignored.

---

## A.2 Segments

Patterns are split into **segments** by `/`.

```
core-pattern := segment ('/' segment)*
segment      := token+
```

Empty segments are ignored **except** in root-only cases (`/`).

---

## A.3 Tokens (Glob & GitIgnore)

Within each segment, the scanner produces tokens:

```
token :=
    literal
  | '*'
  | '?'
  | '**'
  | character-class
```

---

### Literal

Any character that is not part of another token.

Escaping is supported when enabled:

```
\*  \?  \[  \]  \!
```

---

### Wildcards

| Token | Meaning                                      |
| ----- | -------------------------------------------- |
| `*`   | Matches zero or more characters (except `/`) |
| `?`   | Matches exactly one character                |
| `**`  | Matches zero or more *path segments*         |

---

### Recursive Wildcard Rules

* `**` **must be isolated**
* Valid: `**/foo`, `foo/**`, `**`
* Invalid: `a**`, `**a`, `a**b`

These constraints are enforced by **invariants**, not tokenizers.

---

### Character Classes

```
character-class :=
    '[' ['!' | '^'] class-item+ ']'

class-item :=
    literal
  | literal '-' literal
```

Examples:

```
[a-z]
[0-9]
[abc]
[!aeiou]
```

Invalid examples:

```
[]
[a-]
[-z]
[z-a]
```

> Character classes are tokenized structurally and validated semantically.

---

## A.4 Directory Semantics

| Syntax   | Meaning                  |
| -------- | ------------------------ |
| `foo/`   | Matches directories only |
| `/foo`   | Root-anchored            |
| `./foo`  | ❌ invalid                |
| `../foo` | ❌ invalid                |

Segments equal to `.` or `..` are **not permitted**.

---

## A.5 Regex Patterns (Flat Mode)

Regex patterns are **explicit** and bypass glob grammar.

```
regex-pattern := 'r:' regex-text
```

Rules:

* Must start with `r:`
* No tokenization
* No glob or GitIgnore semantics
* Applied to the full normalized path

Example:

```
r:^src\/.*\.cs$
```

---

## A.6 Grammar Summary (EBNF)

```
pattern        ::= regex | glob
regex          ::= 'r:' .+
glob           ::= ['!'] ['/'] segment ('/' segment)* ['/']

segment        ::= token+
token          ::= literal | '*' | '?' | '**' | charclass
charclass      ::= '[' ['!'|'^'] classitem+ ']'
classitem      ::= literal | literal '-' literal
```

---

# Appendix B — Why Invariants?

Invariants are **not validation helpers**.
They are a **core architectural boundary**.

---

## B.1 The Core Design Rule

> **The scanner never decides whether a pattern is valid.**

Instead:

1. The scanner **always tokenizes**
2. Invariants **interpret meaning**
3. Matchers **assume correctness**

This separation is deliberate.

---

## B.2 Why Not Validate During Tokenization?

Because tokenization answers only one question:

> “What symbols are present?”

Validation answers a different question:

> “What do these symbols *mean together*?”

Example:

```
[a-z]     ✅ valid
[z-a]     ❌ invalid (semantic)
```

Both tokenize identically.
Only **semantic analysis** can detect the error.

---

## B.3 Invariant Phases

Invariants run in **ordered phases**:

### 1️⃣ Lexical Invariants

* Literal normalization
* Escape correctness

### 2️⃣ Structural Invariants

* Recursive wildcard isolation
* Character class structure
* Directory traversal (`.` / `..`)
* Regex syntax validity

### 3️⃣ Semantic Invariants

* GitIgnore negation rules
* Implicit recursive wildcards
* Mode-specific constraints

Each invariant:

* Reads the compiled context
* Returns success or a structured failure
* Never mutates tokens

---

## B.4 Benefits of the Invariant Model

### ✅ Deterministic behavior

The same pattern always fails in the same phase.

### ✅ Better diagnostics

Errors include **exact location and meaning**, not tokenizer noise.

### ✅ Testability

Each invariant can be tested independently.

### ✅ Extensibility

New rules = new invariant
No scanner changes required.

---

## B.5 Scanner Contract (Critical)

The scanner guarantees:

* ❌ No `PatternException` for malformed input
* ❌ No semantic decisions
* ❌ No pattern matching

It produces **tokens only**.

> If a scanner throws, it is a bug.

---

## B.6 Why This Matters at Scale

At millions of paths:

* Validation must happen **once**
* Matching must assume **zero invalid states**
* Runtime checks must be eliminated

Invariants make this possible.

## Links

- [Full documentation](https://github.com/TarekNajem04/Jeninnet.FileQuery/docs)
- [Changelog](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/CHANGELOG.md)
- [License: MIT](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/LICENSE)