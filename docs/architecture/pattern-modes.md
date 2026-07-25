# Pattern Matching Modes

`Jeninnet.FileQuery` supports four distinct matching modes. These can be mixed in **Hybrid** mode or forced globally.

## 1. Hybrid Mode (Default)
In Hybrid mode, the engine automatically classifies each pattern based on its syntax.
- Patterns starting with `r:` are treated as Regular Expressions.
- Patterns starting with `!` are treated as negated GitIgnore rules.
- Standard patterns are analyzed to determine if they follow Glob or GitIgnore semantics.

**Example:**
```csharp
.Where("**")           // GitIgnore (Exclude all)
.Where("!*.tmp")      // GitIgnore (Include temp)
.Where("r:^data_.*")   // Regex (Start with data_)
```

## 2. GitIgnore Mode
Forces all patterns to be interpreted using GitIgnore semantics. This is the primary mode for folder-level exclusions and recursive wildcards.

**Key features:**
- `**` for recursive directory matching.
- `!` for negation (last-rule-wins).
- Directory-specific rules (e.g., `bin/`).

## 3. Glob Mode
Forces all patterns to be interpreted as standard Globs. Best for simple filename matching.

**Key features:**
- `*` matches any sequence of characters within a directory.
- `?` matches a single character.
- `[a-z]` character classes.

## 4. Regex Mode
Forces all patterns to be treated as .NET Regular Expressions. Provides the maximum power for complex string matching.

**Example:**
- `^File_\d{3}\.txt$` matches exactly "File_001.txt", "File_999.txt", etc.

---

## Summary Table

| Mode | Recursive (`**`) | Negation (`!`) | Complex Logic | Use Case |
| :--- | :---: | :---: | :---: | :--- |
| **Hybrid** | Yes | Yes | Yes | General purpose, maximum flexibility |
| **GitIgnore**| Yes | Yes | No | Project-wide exclusions |
| **Glob** | No | No | No | Simple file extensions/wildcards |
| **Regex** | No | No | Yes | Exact, complex naming constraints |
