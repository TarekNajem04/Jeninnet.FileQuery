# In-Memory Path Matching

`FileQueryPatternSet` compiles patterns once and matches individual root-relative paths without
traversing a filesystem. It is intended for linters, IDE actions, path classifiers, and tests.

---

## Matching

```csharp
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;

var patternSet = FileQueryPatternSet.Compile(
    new PatternInput(
        TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>>
        {
            [PatternKind.GitIgnore] = ["**", "!*.cs", "!bin/"]
        },
        InterpretationMode: PatternInterpretationMode.Specific),
    PatternMatchingMode.GitIgnore);

bool isCs = patternSet.IsMatch("src/Program.cs");   // true
bool isLog = patternSet.IsMatch("logs/app.log");    // false
bool isBin = patternSet.IsMatch("bin/");             // true
```

Paths are root-relative and use forward slashes. A trailing slash marks a directory, which is
required for directory-only rules such as `bin/`. The string overload normalizes backslashes; the
span overload expects a normalized path and performs no allocation.

```csharp
ReadOnlySpan<char> path = "src/Program.cs";
bool match = patternSet.IsMatch(path);   // allocation-free
```

---

## Explaining a decision

```csharp
var explanation = patternSet.Explain("src/Program.cs");

Console.WriteLine(explanation.Outcome);      // Include
Console.WriteLine(explanation.Pattern);      // !*.cs
Console.WriteLine(explanation.PatternKind);  // GitIgnore
Console.WriteLine(explanation.PatternIndex); // 1
Console.WriteLine(explanation.Reason);       // Include by GitIgnore pattern.
```

`FileQueryPatternExplanation` reports the final `FileQueryMatchOutcome` (`NoMatch`, `Include`, or
`Exclude`), the deciding source pattern and dialect when one exists, and a human-readable reason.

---

## Linter and tooling usage

A linter can cache one pattern set for a repository layout and match candidate paths as they are
edited:

```csharp
var generated = FileQueryPatternSet.Compile(["src/**/Generated/**", "!src/Generated/Keep.cs"]);

foreach (var candidate in changedPaths)
{
    if (!generated.IsMatch(candidate))
    {
        Console.WriteLine($"{candidate}: {generated.Explain(candidate).Reason}");
    }
}
```

Compilation uses the same pipeline as filesystem queries, including hybrid classification,
GitIgnore negation, Glob semantics, and Regex patterns.
