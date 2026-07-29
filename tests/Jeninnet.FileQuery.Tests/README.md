# Jeninnet.FileQuery.Tests

The test suite for the **Jeninnet.FileQuery** library. This project contains unit, integration, architectural, and regression tests that verify the correctness and consistency of the production codebase.

This README is written for contributors who want to understand the test structure, add new test scenarios, or maintain consistency across the project.

---

## Purpose

This test project exists to:

- Verify that every public (and internal) behavior of the library has at least one test.
- Catch regressions before they reach production.
- Document expected behavior through executable specifications.
- Enforce architectural rules (no leaking abstractions, no forbidden dependencies).
- Provide a safety net for refactoring.

### What belongs here

- **Unit tests** for individual components (matchers, compilers, parsers, path utilities, traversal).
- **Integration tests** that exercise multiple components working together (e.g. `FileQueryBuilder` end-to-end).
- **Architecture tests** that enforce structural rules (e.g. engine layer must not reference pattern internals).
- **Regression tests** for specific bugs that have been fixed and must not recur.

### What does NOT belong here

- Production code or business logic.
- Performance benchmarks (use a separate benchmark project).
- Tests that require external services, databases, or network access.
- Manual or exploratory test scripts.

---

## Project Structure

```text
Jeninnet.FileQuery.Tests/
│
├── Architecture/              Architectural contract & dependency tests
├── CommandLine/               Command-line parsing tests
├── FileEnumeration/           File enumeration tests (sync & async)
│   ├── Sync/                  Synchronous enumeration scenarios
│   └── Async/                 Asynchronous enumeration scenarios
├── Integration/               End-to-end integration tests
├── Regression/                Regression tests for fixed bugs
├── Shared/                    Reusable test helpers, fixtures & utilities
├── Unit/
│   ├── Engine/                FileQueryEngine & builder tests
│   ├── IO/                    FileSystem abstraction tests
│   ├── Matchers/              Path matcher tests (GitIgnore, Glob, Hybrid, Regex)
│   ├── Options/               FileQueryOptions tests
│   ├── Path/                  Path normalization & utility tests
│   ├── Patterns/
│   │   ├── Analysis/          Pattern analyzer tests
│   │   ├── Canonical/         Pattern canonicalization tests
│   │   ├── Classification/    Pattern classifier tests
│   │   ├── Compilation/       Pattern compiler tests
│   │   ├── Parsing/           Character class parser tests
│   │   ├── Tokens/            Token reader & scanner tests
│   │   └── Validation/        Pattern validator tests
│   └── Traversal/             Traversal strategy tests (BFS)
├── Validation/                FileQuery validation tests
│
├── GlobalUsings.cs            Global using directives
├── MSTestSettings.cs          MSTest parallelization config
└── Jeninnet.FileQuery.Tests.csproj
```

---

## Folder Responsibilities

### Architecture/

Tests that enforce structural and architectural rules. These do not test business logic — they verify that the production codebase respects its own layering rules. Examples: engine must not reference pattern internals, matchers must not have public constructors, production code must not call `System.IO.Path.GetFullPath` directly.

**Belongs here:** Dependency checks, access modifier rules, forbidden API usage.
**Does NOT belong here:** Behavioral tests, happy-path tests.

### CommandLine/

Tests for the `System.CommandLine`-based CLI parsing layer. Covers option definitions, pattern splitting, pattern building from CLI input, and parser behavior.

**Belongs here:** Anything related to how user CLI input is parsed and converted into `FileQuery` options.
**Does NOT belong here:** Engine behavior, pattern matching logic.

### FileEnumeration/

Tests that exercise the full file enumeration pipeline — creating a `FileQueryEngine` or `FileQueryRuntime`, walking a real temporary directory tree, and asserting the correct files are returned.

- **Sync/** — tests using `FileQueryEngine.Execute()` (synchronous).
- **Async/** — tests using `FileQueryRuntime.ExecuteAsync()` (asynchronous, cancellation, concurrency).

**Belongs here:** End-to-end enumeration tests that create real directories and verify file discovery.
**Does NOT belong here:** Pure unit tests of individual components (those go in `Unit/`).

### Integration/

Tests that combine multiple components to verify they work together correctly. This includes `FileQueryBuilder` API tests, dependency injection parity, and core API surface tests.

**Belongs here:** Multi-component scenarios, builder API tests, DI tests.
**Does NOT belong here:** Single-component unit tests, architectural rules.

### Regression/

Targeted tests for specific bugs that have been fixed. Each test documents the exact scenario that triggered the bug and verifies the fix holds.

**Belongs here:** One bug fix = one test (or small test group) with clear scenario descriptions.
**Does NOT belong here:** General behavioral tests, new feature tests.

### Shared/

Reusable test infrastructure. This folder contains helpers, fixtures, and utilities used across all other test folders. Everything here is available via `global using Jeninnet.FileQuery.Tests.Shared;`.

| File | Purpose |
|------|---------|
| `TestEnvironment.cs` | Creates isolated temp directories, builds file structures, cleans up on dispose. |
| `TestEnvironmentExtensions.cs` | Extension methods for building deep directory hierarchies with `TestEnvironment`. |
| `TestAssertEx.cs` | Custom assertion helpers (`Throws<T>`, `DoesNotContain`, `ContainsSingle`, etc.). |
| `TestPathUtils.cs` | Cross-platform path normalization and comparison utilities. |
| `TestPath.cs` | Forward-slash path builder for cross-platform test assertions. |
| `TestPattern.cs` | Factory for constructing compiled patterns from raw strings. |
| `TestMatcher.cs` | Factory for creating `HybridPathMatcher` instances. |
| `PatternHelpers.cs` | Creates pattern dictionaries (`PatternKind` → pattern strings). |
| `FakeCompiledPattern.cs` | Stub `ICompiledPattern` for tests that need a default implementation. |
| `InaccessibleDirectorySimulator.cs` | Simulates inaccessible directories without modifying ACLs. |
| `PathExtensions.cs` | Path string assertion extensions (`EndsWith` with forward-slash normalization). |

**Belongs here:** Shared utilities, base classes, factories, stubs.
**Does NOT belong here:** Test methods, scenario-specific setup.

### Unit/

Pure unit tests for individual production components. Each subfolder mirrors the production code's namespace structure.

| Subfolder | What it tests |
|-----------|---------------|
| `Engine/` | `FileQueryEngine`, `FileQueryBuilder`, `DefaultEngineBuilder`, observability, diagnostics. |
| `IO/` | `FileSystem`, `FileSystemEntry`, `FileSystemGuards`. |
| `Matchers/` | `GitIgnorePathMatcher`, `GlobPathMatcher`, `HybridPathMatcher`, `RegexPathMatcher`, `NullMatcher`, match precedence, match determinism. |
| `Options/` | `FileQueryOptions` validation. |
| `Path/` | `PathUtilities.Normalize`, `BuildRelativePath`, `SplitNormalizedPath`, UNC paths. |
| `Patterns/` | Pattern analysis, canonicalization, classification, compilation, parsing, tokenization, validation. |
| `Traversal/` | `BfsTraversal`, `PathUtilitiesUnc`. |

**Belongs here:** Tests that instantiate a single component and verify its behavior in isolation.
**Does NOT belong here:** Tests that require multiple components, real file systems, or DI setup.

### Validation/

Tests for the `FileQueryValidator` class that validates `FileQuery` instances before execution.

**Belongs here:** Input validation, error message correctness, edge cases.
**Does NOT belong here:** Happy-path execution tests.

---

## Naming Conventions

### Folders

- Use **PascalCase** singular nouns.
- Name folders after the component or concept they test.
- Subfolders mirror the production namespace hierarchy.

```
Unit/Matchers/          ✓
Unit/MATCHERS/          ✗
unit/matchers/          ✗
Matchers Tests/         ✗
```

### Namespaces

Namespaces must match the folder path relative to the project root.

```
Folder:                              Namespace:
Unit/Patterns/Compilation/           Jeninnet.FileQuery.Tests.Unit.Patterns.Compilation
FileEnumeration/Sync/                Jeninnet.FileQuery.Tests.FileEnumeration.Sync
Shared/                              Jeninnet.FileQuery.Tests.Shared
```

### Test Classes

- Use **PascalCase**.
- End with `Tests` (plural).
- Name after the component being tested.

```
FileQueryEngineTests                 ✓
PathUtilitiesTests                   ✓
GitIgnoreMatcherTests                ✓
Test1                                ✗
UnitTest                             ✗
MyTest                               ✗
```

### Test Methods

Use the **`Should_X_When_Y`** format:

- **`Should_`** + expected behavior.
- **`When_`** + the condition or input that triggers the behavior.

If there is no condition to express, `Should_X` alone is acceptable.

```
Should_ReturnEmpty_When_DirectoryDoesNotExist    ✓
Should_MatchDeepPaths_When_RecursiveWildcardUsed  ✓
Should_OverridePrevious_When_NegationApplied       ✓
Should_HandleUnicodeCharacters                    ✓  (no condition needed)
Test1                                             ✗
CheckMethod                                       ✗
NegationWorks                                     ✗
```

For async tests, append `_Async`:

```
Should_ReturnMatchingFiles_Async                  ✓
Should_Cancel_When_DuringDeepRecursion_Async      ✓
```

### Helper Classes

- Use **PascalCase**.
- Name after their responsibility.
- Do **not** suffix with `Tests`.

```
TestEnvironment          ✓
TestAssertEx             ✓
TestPathUtils            ✓
TestEnvironmentTests     ✗  (helpers are not test classes)
```

---

## Where Should I Put My Test?

Use this decision guide to find the right folder for a new test.

| If I am testing... | Place it in... |
|--------------------|----------------|
| Pattern compilation (GitIgnore, Glob, Hybrid) | `Unit/Patterns/Compilation/` |
| Pattern classification / kind detection | `Unit/Patterns/Classification/` |
| Pattern tokenization / scanning | `Unit/Patterns/Tokens/` |
| Character class parsing (`[a-z]`, `[:digit:]`) | `Unit/Patterns/Parsing/` |
| Pattern validation / malformed detection | `Unit/Patterns/Validation/` |
| Pattern canonicalization / deduplication | `Unit/Patterns/Canonical/` |
| Pattern analysis (recursive wildcard detection) | `Unit/Patterns/Analysis/` |
| Path normalization / UNC / separators | `Unit/Path/` |
| GitIgnore matcher behavior | `Unit/Matchers/GitIgnoreMatcherTests.cs` |
| Glob matcher behavior | `Unit/Matchers/GlobMatcherTests.cs` |
| Hybrid matcher behavior | `Unit/Matchers/HybridPathMatcherTests.cs` |
| Regex matcher behavior | `Unit/Matchers/RegexPathMatcherTests.cs` |
| Match precedence / determinism | `Unit/Matchers/` |
| FileQueryEngine execution | `Unit/Engine/` |
| FileQueryBuilder API | `Integration/FileQueryBuilderTests.cs` |
| FileQuery options / validation | `Unit/Options/` or `Validation/` |
| FileSystem abstraction | `Unit/IO/` |
| Traversal strategy (BFS) | `Unit/Traversal/` |
| Full sync file enumeration | `FileEnumeration/Sync/` |
| Full async file enumeration | `FileEnumeration/Async/` |
| Dependency injection setup | `Integration/DependencyInjectionParityTests.cs` |
| CLI argument parsing | `CommandLine/` |
| Architectural / layering rules | `Architecture/` |
| A specific bug fix | `Regression/` |

---

## Adding a New Test

### Step 1: Identify the component

What production class, method, or behavior are you testing?

### Step 2: Choose the folder

Use the decision guide above. When in doubt:

- **Single component, isolated?** → `Unit/<Subfolder>/`
- **Multiple components working together?** → `Integration/`
- **Full directory walk with real files?** → `FileEnumeration/Sync/` or `FileEnumeration/Async/`
- **Architectural rule?** → `Architecture/`
- **Bug regression?** → `Regression/`

### Step 3: Create the test class

```csharp
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Compilation;

[TestClass]
public class MyNewFeatureTests {
    [TestMethod]
    public void Should_DoExpected_When_SpecificCondition() {
        // Arrange
        // Act
        // Assert
    }
}
```

### Step 4: Write the test method

Follow the `Should_X_When_Y` naming convention and the Arrange / Act / Assert pattern:

```csharp
[TestMethod]
public void Should_ReturnEmptySet_When_InputIsEmpty() {
    // Arrange
    var input = "";

    // Act
    var result = PatternCanonicalizer.Canonicalize(input);

    // Assert
    Assert.IsEmpty(result);
}
```

### Step 5: Reuse shared infrastructure

Before writing helper code, check `Shared/` for existing utilities:

- Need a temp directory? → Use `TestEnvironment`.
- Need path assertions? → Use `TestPathUtils.ToForwardSlash()`.
- Need to assert exceptions? → Use `TestAssertEx.Throws<T>()`.
- Need a compiled pattern? → Use `TestPattern.GitIgnore()` or `TestPattern.Glob()`.

### Step 6: Verify

Run the full test suite before committing:

```bash
dotnet test
```

All tests must pass. No existing test should break.

---

## Best Practices

### One responsibility per test class

Each test class should focus on one component or one aspect of behavior. If a class has more than 15-20 tests, consider splitting it by behavior.

### One behavior per test

Each `[TestMethod]` should verify exactly one behavior. Avoid testing multiple unrelated things in a single method.

### Use descriptive assertions

Prefer assertion messages and custom helpers over generic `Assert.IsTrue`:

```csharp
// Good
Assert.Contains(result, x => x.EndsWith("expected.txt"));
TestAssertEx.DoesNotContain(result, x => x.Contains("bin"));

// Bad
Assert.IsTrue(result.Count > 0);
```

### Keep tests deterministic

Tests must produce the same result every time. Avoid relying on:
- Current date/time (unless explicitly testing date behavior).
- Network calls.
- External file system state beyond what `TestEnvironment` creates.
- Non-deterministic ordering.

### Avoid duplicated setup

If multiple tests need the same directory structure or object graph, extract it into:
- A `TestEnvironment` setup in `Shared/`.
- A helper method in the test class.
- A `[TestInitialize]` method (sparingly).

### Prefer real objects over mocks

Use the real production implementation whenever possible. Only mock when the real object has side effects (network, disk, time). The `Shared/` folder provides stubs (`FakeCompiledPattern`, `TestMatcher`) for cases where a real implementation is impractical.

### Follow Arrange / Act / Assert

Structure every test method in three clear sections:

```csharp
[TestMethod]
public void Should_EnumerateOnlyRoot_When_MaxDepthZero() {
    // Arrange
    using var env = new TestEnvironment();
    env.CreateFiles("root.txt", "sub/child.txt");

    // Act
    var results = engine.Execute(query).ToList();

    // Assert
    TestAssertEx.HasCount(results, 1);
    Assert.Contains(results, x => x.EndsWith("root.txt"));
}
```

---

## Contributing

Before submitting a Pull Request that adds or modifies tests, verify the following:

### Checklist

- [ ] **Correct folder** — the test is in the right folder for what it tests.
- [ ] **Naming conventions** — class ends with `Tests`, method follows `Should_X_When_Y`.
- [ ] **Namespace matches folder** — `namespace` declaration reflects the folder path.
- [ ] **Deterministic** — the test produces the same result on every run.
- [ ] **Isolated** — the test does not depend on other tests or shared state.
- [ ] **No duplicated helpers** — check `Shared/` before writing utility code.
- [ ] **Reuses `TestEnvironment`** — tests that need temp directories use `TestEnvironment`, not manual `Path.GetTempPath()`.
- [ ] **Arrange / Act / Assert** — the test body is clearly structured.
- [ ] **All tests pass** — run `dotnet test` and confirm zero failures.
- [ ] **No warnings** — the build produces no new warnings.

---

## Maintenance Guidelines

As the library grows, the test project should evolve with it. Follow these principles:

### Organize by feature, not by implementation

Test folders should reflect **what** is being tested, not **how** the production code is implemented internally. If the implementation changes but the behavior doesn't, the test structure should remain stable.

### Avoid unnecessary nesting

Keep the folder hierarchy shallow. Two levels of nesting is usually sufficient (e.g. `Unit/Patterns/Compilation/`). Three levels is the maximum. If you need more, consider whether the tests belong in a separate top-level folder.

### Keep namespaces aligned with folders

Every folder rename must be accompanied by a namespace update across all `.cs` files within that folder. The namespace should always match the folder path relative to the project root.

### Prefer moving tests over creating new top-level folders

When a test no longer fits its current location, move it to the correct folder rather than creating a new top-level folder. The existing hierarchy should accommodate most scenarios.

### Refactor common code into `Shared/`

If two or more test classes need the same setup logic, helper method, or fixture, extract it into `Shared/` and make it reusable. The `Shared/` folder is globally available via `GlobalUsings.cs`.

### Keep the test count healthy

Every new public API or behavior in the production code should have at least one corresponding test. When deleting or refactoring production code, remove or update the corresponding tests — do not leave orphaned tests that verify removed behavior.
