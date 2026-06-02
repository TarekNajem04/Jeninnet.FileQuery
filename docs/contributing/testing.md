# Testing Strategy

## Overview
Jeninnet.FileQuery employs a rigorous testing strategy to ensure the correctness, determinism, and performance of the file discovery engine. Given the complexity of combined pattern matching (Glob, Regex, GitIgnore), the test suite is designed to cover edge cases in path traversal and pattern evaluation.

## Why it is Used
To prevent regressions in the matching engine and ensure that the `IPathMatcher` implementations adhere to the defined invariants.

## Implementation in this Solution
The project uses **MSTest** as the primary test runner and **Moq** for isolating dependencies.

### Test Layers
1. **Architecture Tests**: (`ArchitectureTests.cs`) Verify that the solution adheres to structural constraints (e.g., internal visibility, project dependencies).
2. **Unit Tests**: 
   - `MatcherTests`: Specific tests for `GlobInstructionMatcher`, `RegexInstructionMatcher`, and `GitIgnoreInstructionMatcher`.
   - `PatternEngineTests`: Validation of the `PatternClassifier` and `PatternCompiler`.
3. **Integration Tests**: (`EndToEnd_FileEnumTests.cs`) End-to-end scenarios simulating real-world directory structures.
4. **Regression Tests**: (`RegressionTests.cs`) A consolidated suite of previously discovered bugs to prevent recurrence.

## Code Example: Integration Test
```csharp
[TestMethod]
public async Task EnumerateFilesAsync_HandlesMixedPatterns()
{
    var builder = new FileQueryBuilder()
        .WithPattern("src/**/*.cs")
        .WithPattern("!bin/")
        .WithPattern("!obj/");

    var engine = builder.Build();
    var results = await engine.EnumerateFilesAsync("C:\\Project", CancellationToken.None);

    Assert.IsFalse(results.Any(f => f.Contains("bin") || f.Contains("obj")));
}
```

## Best Practices
- **Isolation**: Use the `TestEnvironment` helper to create ephemeral temporary directories.
- **Determinism**: Always test with both case-sensitive and case-insensitive configurations.
- **Coverage**: Priority is given to the `Matching` and `Traversal` namespaces.
- **Cross-platform readiness**: Treat Windows, Linux, and macOS CI results as
  release-gating for traversal changes. Pay particular attention to
  `CaseSensitivityTests` and `InaccessibleDirectoryTests`, because filesystem
  casing and directory permission behavior differ by platform.
