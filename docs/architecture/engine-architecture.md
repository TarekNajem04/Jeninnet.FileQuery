# Jeninnet.FileQuery — Engine Architecture

## High-Level Overview

The engine follows a **compile-then-execute architecture**.

Patterns are first **classified and compiled** into efficient matchers.
A **TraversalPlan** is then executed to enumerate files.

Three concerns are strictly separated:

* **pattern compilation** — transforms raw strings into executable matchers
* **filesystem traversal** — discovers paths
* **matching execution** — evaluates paths against compiled patterns

---

## Layer Boundaries

The core package is a library-first engine. Optional presentation and integration
packages depend on it; the core package must not depend on them.

Allowed dependency direction:

```
CommandLine / DependencyInjection
        │
        ▼
Public API
        │
        ▼
Engine / Traversal Plan
        │
        ├── IO abstraction
        ├── Pattern compilation
        └── Matching
```

Boundary rules:

* `Jeninnet.FileQuery` owns the public contracts, engine, traversal, matching,
  pattern compilation, and file-system abstraction.
* `Jeninnet.FileQuery.CommandLine` is presentation/adaptation code only. It may
  parse command-line input into public query contracts, but it must not own
  matching or traversal behavior.
* `Jeninnet.FileQuery.DependencyInjection` is composition code only. It may
  register core services and lifetimes, but it must not introduce runtime
  behavior that differs from `FileQueryRuntime.Create()`.
* `Engine` coordinates execution through `TraversalPlanBuilder` and
  `TraversalExecutor`; it must not parse patterns or touch physical I/O
  directly.
* `Traversal` may depend on matching contracts and the `IO` abstraction, but
  pattern tokenization and compilation must remain in the `Patterns` namespace.
* `Matching` consumes compiled pattern contracts and must remain allocation-free
  on the hot path.
* `IO` centralizes physical file-system interaction and access-error policy.

---

## Execution Pipeline

```
User Code
   │
   ▼
FileQueryRuntime.Create()
   │
   ▼
FileQueryBuilder / FileQuery
   │
   ▼
TraversalPlanBuilder.Build(query)
   │
   ├── PatternsMerger.Merge(PatternInput)
   │
   ├── PatternPipeline
   │      ├─ Phase 1: Lexical invariants
   │      ├─ Phase 2: PatternScanner (Tokenize)
   │      ├─ Phase 3: Structural invariants
   │      └─ Phase 4: Semantic invariants + dialect transforms
   │
   ├── CompiledPatternSet
   │
   └── TraversalPlan
          │
          ▼
   TraversalExecutor
          │
          ▼
   HybridPathMatcher (per path)
          │
          ▼
   Result Stream
```

---

## 1. Runtime Entry Point

**Location:** `src/Jeninnet.FileQuery/FileQueryRuntime.cs`

```csharp
var engine = FileQueryRuntime.Create();

var options = new FileQueryOptions {
    PatternInput = new(patterns: [
        "**",
        "!*.txt"
    ])
};

var result = engine.Execute(new(root, options));
```

`FileQueryRuntime.Create()` is the composition root. It wires the default
engine without exposing any internal components.

---

## 2. Public Pattern Configuration

**Location:** `src/Jeninnet.FileQuery/PatternInput.cs`

`PatternInput` is the public boundary between the caller and the compilation
pipeline. It uses standard BCL types (`IReadOnlyList<string>`,
`IReadOnlyDictionary<PatternKind, IReadOnlyList<string>>`).

```csharp
// Auto-classified (hybrid mode)
var input = new PatternInput(
    patterns: ["**", "!*.log", "r:^data_.*"]
);

// Explicitly typed (bypasses classifier)
var input = new PatternInput(
    typedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
        [PatternKind.GitIgnore] = ["**", "!*.cs"],
        [PatternKind.Regex]     = ["r:^temp_.*\\.log$"]
    }
);
```

When the kind is supplied explicitly, the compilation pipeline bypasses the
`PatternCanonicalizer` and `PatternClassifier` entirely, reducing per-query
allocation by approximately 400–500 B per pattern.

---

## 3. Pattern Compilation Pipeline

**Location:** `src/Jeninnet.FileQuery/Patterns/Compilation/PatternPipeline.cs`

Compilation runs in four sequential phases, each with a distinct responsibility:

### Phase 1 — Lexical invariants
Validates raw pattern text before scanning begins. Examples:
- `EmptyPatternInvariant` — rejects null or whitespace patterns
- `RegexSyntaxInvariant` — compiles the regex expression (without the `r:` prefix) to verify it is valid .NET syntax
- `LiteralNormalizationInvariant` — rejects patterns containing control characters

### Phase 2 — PatternScanner (Lexer + Structural Parser)
`PatternScanner` is a pure lexer. It identifies structural markers (`!`, leading `/`,
trailing `/`) and tokenizes each segment. It does **not** apply dialect-specific
transforms — that is the invariant system's responsibility.

Token types produced:
```
LiteralToken
WildcardToken (*)
RecursiveWildcardToken (**)
SingleCharToken (?)
CharacterClassToken ([...])
  └─ Elements: CharLiteral | CharRange | PosixClass | CharacterClassParseError
RegularExpressionToken (r:...)
EscapeToken
```

### Phase 3 — Structural invariants
Validates the token stream. Examples:
- `CharacterClassStructureInvariant` — detects `CharacterClassParseError` sentinels
- `CharacterClassRangeInvariant` — detects inverted ranges (`z-a`)
- `RecursiveWildcardInSegmentInvariant` — rejects mixed segments (`**a`, `a**`)
- `RecursiveWildcardRedundancyInvariant` — rejects adjacent `**/**`
- `ParentTraversalInvariant` — rejects `..` traversal
- `CurrentDirectoryInvariant` — rejects `.` segments

### Phase 4 — Semantic invariants + dialect transforms
Applies meaning and rewrites the token stream where needed. Examples:
- `GitIgnoreImplicitRecursiveInvariant` — prepends an implicit `**` to all
  unanchored GitIgnore patterns (replaces the removed `PatternScanner.ApplyImplicitRecursiveWildcard`)
- `GitIgnorePatternInvariant` — validates directory-only and root-anchored patterns
- `GlobPatternInvariant` — enforces `**` isolation in glob segments

---

## 4. Character Class System

**Location:** `src/Jeninnet.FileQuery/Patterns/Syntax/CharacterClasses/`

Character classes use a discriminated union of `ICharacterClassElement`:

```
ICharacterClassElement
├─ CharLiteral(char Value)              — single literal character
├─ CharRange(char Start, char End)      — inclusive range, e.g. a-z
├─ PosixClass(string Name)             — POSIX named class, e.g. [:digit:]
└─ CharacterClassParseError(string)    — compile-time parse error sentinel
```

The parser (`CharacterClassParser`) never throws. Structural problems are
recorded as `CharacterClassParseError` sentinels and surfaced by
`CharacterClassStructureInvariant` during Phase 3.

Supported POSIX classes: `digit`, `alpha`, `alnum`, `space`, `blank`,
`upper`, `lower`, `print`, `graph`, `punct`, `cntrl`, `xdigit`.

---

## 5. HybridPathMatcher

**Location:** `src/Jeninnet.FileQuery/Matching/Compiled/HybridPathMatcher.cs`

The HybridPathMatcher coordinates three specialized matchers:

```
HybridPathMatcher
├─ GitIgnoreInstructionMatcher   — directory-aware, last-rule-wins, anchoring
├─ GlobInstructionMatcher        — flat wildcard matching, first-match-wins
└─ RegexInstructionMatcher       — full .NET Regex, cached by (text, case)
```

Pattern routing is performed by `MatchPrecedenceResolver`, which uses pre-built
sub-sets (`CompiledPatternSet.GitIgnoreSubSet`, `.GlobSubSet`, `.RegexSubSet`).
Sub-sets are built once during compilation, not at match time.

**Zero-allocation hot path:** all matchers use index-based `for` loops over
`ICompiledPatternSet`. A `foreach` over an interface-typed collection boxes the
enumerator (~40 B per call). After this fix all three matchers show 0 B allocated
in benchmarks.

---

## 6. Traversal

**Location:** `src/Jeninnet.FileQuery/Traversal/`

`TraversalExecutor` performs streaming depth-first (default) or breadth-first
enumeration. For each discovered entry:

1. Build a root-relative path with `PathUtilities.BuildRelativePath`
2. Evaluate the path with `HybridPathMatcher.Match`
3. Apply the decision from `TraversalEvaluator` (yield file / recurse into directory)

Key options:

```csharp
new FileQueryOptions {
    RecurseSubdirectories = true,                   // default
    MaxRecursionDepth     = -1,                     // -1 = unlimited
    IgnoreInaccessible    = true,                   // skip locked/denied directories
    CaseSensitivity       = PlatformDefault,        // Linux: Sensitive, Windows/macOS: Insensitive
    Traversal             = new TraversalOptions(
        Strategy:     TraversalStrategy.DepthFirst,  // or BreadthFirst
        SymlinkPolicy: SymlinkPolicy.Ignore
    )
}
```

---

## 7. Path Normalization

**Location:** `src/Jeninnet.FileQuery/IO/PathUtilities.cs`

`PathUtilities.Normalize` produces a canonical forward-slash representation
on all platforms:

- All separators become `/`
- Consecutive duplicate slashes are collapsed (except the leading `//` of UNC paths)
- Windows drive roots (`C:/`) preserve their trailing slash
- UNC roots (`//server/share` and `//server/share/`) are correctly preserved
- Optional `trimTrailingSlash: false` parameter retains trailing slash for directory context

---

## 8. Dependency Injection Integration

**Location:** `src/Jeninnet.FileQuery.DependencyInjection/`

```csharp
builder.Services.AddFileQuery();
```

Registers `IFileQueryEngine`, `ITraversalPlanBuilder`, `ITraversalExecutor`,
`PatternInvariantRegistry`, `IPatternCompilerRegistry`, and `PatternPipeline`
as singleton services.

---

## Key Architectural Invariants

### Matchers never inspect raw pattern text
Matchers receive only `ICompiledPatternSet`. Raw strings never reach the
matching layer. This is enforced by architecture tests in
`ArchitectureTests.EngineLayer_Must_Not_Reference_Patterns_Namespace`.

### PatternScanner never applies semantic transforms
`PatternScanner` is a pure lexer. It does not know about `PatternKind` semantics.
The only place where implicit `**` segments are inserted is
`GitIgnoreImplicitRecursiveInvariant` in the Semantic phase.

### Matchers carry no per-query mutable state
`GitIgnoreInstructionMatcher`, `GlobInstructionMatcher`, and
`RegexInstructionMatcher` instances are shared singletons inside
`MatchPrecedenceResolver.Default`. The regex cache in `RegexInstructionMatcher`
is the sole mutable state, protected by `ConcurrentDictionary`.

### Construction authority
All `IPathMatcher` implementations have `internal` constructors. The sole
construction point is `PathMatcherFactory`. Architecture tests enforce this.
