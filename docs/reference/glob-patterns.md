# Glob Patterns

Glob patterns provide traditional wildcard matching. Unlike GitIgnore patterns, glob patterns in Jeninnet.FileQuery are always **anchored to the root** — they match from the beginning of the path.

---

## When to Use Glob

Use Glob patterns when:

- You want classical Unix-style globbing behaviour
- You do not need negation (`!`) — if you do, use GitIgnore patterns
- You are porting patterns from another tool that uses glob syntax
- You want explicit anchoring without a leading `/`

---

## Syntax Reference

| Syntax | Description |
|--------|-------------|
| `*` | Match any sequence of characters within one segment |
| `**` | Match zero or more path segments (must be a standalone segment) |
| `?` | Match exactly one character |
| `[abc]` | Character set |
| `[a-z]` | Character range |
| `[!abc]` | Negated character set |

Negation (`!`) is **not supported** in Glob mode. Use GitIgnore mode for negation.

---

## Anchoring Behaviour

Glob patterns are anchored. The pattern `*.cs` matches only files in the root directory, not files in subdirectories.

```
*.cs            → Program.cs         (match)
*.cs            → src/Program.cs     (no match — different segment)
**/*.cs         → src/Program.cs     (match — ** crosses the boundary)
src/**/*.cs     → src/utils/Helper.cs (match)
src/**/*.cs     → test/Helper.cs      (no match — different root)
```

---

## Selecting Glob Mode

```csharp
var query = FileQuery.From(root)
                     .UsingGlob()
                     .Where("**/*.cs", "src/**/*.md")
                     .Build();
```

Or supply patterns with explicit `PatternKind.Glob`:

```csharp
var options = new FileQueryOptions(
    new FileQueryOptionsConfig(
        PatternInput: new(
            typedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                [PatternKind.Glob] = ["**/*.cs", "src/**/*.md"]
            }
        )
    )
);
```

---

## Mixed Usage

In `Hybrid` mode (the default), a pattern containing a backslash and no GitIgnore-specific syntax is
classified as Glob. The Glob grammar uses `/` as the path separator and `\` only as an escape for a
glob metacharacter (`*`, `?`, `!`, `#`, `[`, `]`, `\`, `{`, `}`). Windows-style separators are **not**
accepted as path separators: compilation fails with an actionable `PatternException` instead of
silently producing a pattern that can never match a normalized path. Use forward slashes in queries:

```csharp
var query = FileQuery.From(root)
                     .Where("src/**/*.cs")
                     .Build();
```

For example, `src\file.cs` is rejected because `\f` is not a Glob escape; write `src/file.cs`.
A backslash before a Glob metacharacter such as `*` is an escape for that literal character, so
`src\*.cs` matches a literal file name containing `*` rather than a `src` directory; use
`src/*.cs` for the directory form.