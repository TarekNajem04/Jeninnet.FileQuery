# Pattern Language Specification

`Jeninnet.FileQuery` provides a flexible pattern system designed for deterministic file matching across large directory trees.

Instead of relying on a single syntax, the engine supports **multiple pattern languages** that can coexist within the same query.

Patterns may be written using:

```
GitIgnore-style rules
Glob-style patterns
Regular expressions
```

These pattern types are unified through a classification and matcher pipeline that ensures consistent evaluation semantics.

---

# Pattern Input Model

Patterns are provided to the engine through the `PatternInput` component of `FileQueryOptions`.

Example:

```csharp
var options = new FileQueryOptions
{
    PatternInput = new(
        patterns:
        [
            "**",
            "!*.txt"
        ]
    )
};
```

Patterns are evaluated sequentially.

The evaluation model follows the rule:

```
last rule wins
```

This rule is fundamental to the behavior of the engine.

Typed patterns can be supplied when the caller already knows the dialect and
wants to bypass automatic classification:

```csharp
var input = new PatternInput(
    typedPatterns: new Dictionary<PatternKind, IEnumerable<string>>
    {
        [PatternKind.GitIgnore] = ["**", "!src/**/*.cs"],
        [PatternKind.Regex] = ["r:^src/.*Tests\\.cs$"]
    }
);
```

The `typedPatterns` constructor argument may be `null`; this is treated the
same as no typed patterns. The `PatternInput.TypedPatterns` property itself is
non-null and returns an empty dictionary when no explicitly typed patterns are
configured:

```csharp
foreach (var (kind, values) in input.TypedPatterns)
{
    Console.WriteLine($"{kind}: {values.Count}");
}

if (input.TypedPatterns.Count == 0)
{
    Console.WriteLine("No explicitly typed patterns.");
}
```

---

# Ordered Rule Evaluation

Patterns are processed in the exact order they appear.

Each pattern updates the **current inclusion state** of a path.

Example rule set:

```
**
!*.log
data.log
```

Evaluation proceeds as follows:

1. Include all files
2. Exclude files ending in `.log`
3. Re-include `data.log`

Final result:

```
data.log is included
```

This deterministic behavior eliminates ambiguity in rule evaluation.

---

# Pattern Canonicalization

Before patterns are processed, they pass through the **PatternCanonicalizer**.

The canonicalization stage ensures that patterns are normalized into a consistent internal form.

Responsibilities include:

```
normalizing path separators
removing redundant segments
trimming whitespace
standardizing pattern representation
```

This step ensures that matchers receive patterns in a predictable format.

---

# Pattern Classification

After canonicalization, patterns are analyzed by the **PatternClassifier**.

Classification determines which matcher should process the pattern.

The classification process maps patterns into one of the supported pattern types.

Possible pattern types include:

```
GitIgnore
Glob
Regex
```

Example classification:

```
*.cs          → Glob
**/*.txt      → GitIgnore
r:^data_.*    → Regex
```

Regex patterns are explicitly identified using the `r:` prefix.

Classification enables the hybrid matcher architecture to route patterns to the appropriate matching engine.

---

# Pattern Tokenization

Pattern matching is performed using a tokenized representation of patterns.

Tokenization occurs within the namespace:

```
Jeninnet.FileQuery.Patterns.Tokenization
```

During tokenization, pattern strings are converted into structured tokens representing pattern semantics.

Example pattern:

```
**/*.txt
```

Possible token representation:

```
RecursiveWildcard
PathSeparator
Wildcard
Literal("txt")
```

Tokenization allows the engine to perform pattern matching efficiently without repeatedly parsing raw pattern strings.

It also enables validation and enforcement of matcher invariants.

---

# GitIgnore Pattern Semantics

GitIgnore-style patterns are the default rule system used by the engine.

These patterns support recursive matching, directory filtering, and negation rules.

Examples:

```
**
!*.log
build/
```

Features include:

```
recursive wildcard (**)
negation (!pattern)
directory rules
ordered evaluation
```

GitIgnore semantics allow developers to describe complex file inclusion and exclusion rules using a compact syntax.

---

# Glob Pattern Semantics

Glob patterns provide traditional wildcard matching used by many file systems.

Examples:

```
*.cs
file?.txt
file[0-9].log
```

Supported constructs include:

```
*  matches any sequence of characters
?  matches a single character
[a-z] matches a character range
```

Glob patterns are ideal for simple file filters.

---

# Regular Expression Patterns

Regular expressions allow advanced filtering based on full regular expression syntax.

Regex patterns must be prefixed with:

```
r:
```

Example:

```
r:^data_.*\.log$
```

Regex patterns are evaluated using the .NET regular expression engine.

This allows powerful matching for complex naming schemes.

---

# Combining Pattern Languages

A key feature of the engine is the ability to combine different pattern languages within the same rule set.

Example:

```
**
!*.tmp
logs/*.log
r:^data_.*
```

In this example:

```
GitIgnore patterns control inclusion rules
Glob patterns match structured filenames
Regex patterns perform advanced filtering
```

The hybrid matcher architecture resolves these patterns deterministically.

---

# Pattern Splitting

Command-line integrations may provide multiple patterns in a single string.

Example:

```
*.txt;!temp.txt;logs/*.log
```

These patterns are separated using the `;` character.

Splitting is performed by:

```
PatternSpliter.Split(...)
```

This produces a sequence of individual patterns which are then classified and tokenized.

---

# Default Behavior

If no patterns are defined, the engine applies a fallback rule.

Default rule:

```
!**
```

This rule includes all files.

This behavior ensures that the engine always operates with a valid rule set.

---

# Pattern Evaluation Pipeline

The complete pipeline for pattern processing can be summarized as:

```
raw pattern strings
        │
        ▼
PatternCanonicalizer
        │
        ▼
PatternClassifier
        │
        ▼
PatternTokenization
        │
        ▼
Matcher Execution
```

This pipeline guarantees that patterns are:

```
normalized
classified
validated
efficiently evaluated
```

---

# Why This Pattern System Exists

Many pattern engines provide only a single syntax or unclear rule semantics.

`Jeninnet.FileQuery` introduces a unified model that provides:

```
deterministic evaluation
multiple pattern languages
efficient tokenized matching
predictable rule precedence
```

This design allows developers to build reliable file filtering systems without sacrificing performance or expressiveness.
