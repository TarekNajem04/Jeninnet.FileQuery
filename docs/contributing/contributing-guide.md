# Contributing Guide

Thank you for your interest in contributing to Jeninnet.FileQuery. This guide explains how to set up your development environment, what to check before submitting a pull request, and how the project is organized.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- An editor with C# support (Visual Studio 2022, VS Code with C# Dev Kit, or JetBrains Rider)

---

## Getting Started

```bash
git clone https://github.com/TarekNajem04/Jeninnet.FileQuery.git
cd Jeninnet.FileQuery
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

All 235+ tests must pass before submitting a pull request.

---

## Running Benchmarks

Benchmarks must be run in Release configuration. Do not run them inside the IDE:

```bash
cd benchmarks
dotnet run -c Release -- --job Short
```

For a full run (takes 5–15 minutes):

```bash
dotnet run -c Release
```

For a quick sanity check (one iteration, fast):

```bash
dotnet run -c Release -- --job Dry
```

---

## Architecture Contracts

The project enforces several architectural contracts through reflection-based tests in `Jeninnet.FileQuery.Tests/Architecture/`. Before submitting, run these specifically:

```bash
dotnet test -c Release --filter "FullyQualifiedName~Architecture"
```

**Layer boundaries:**
- The `Engine` layer must not reference the `Patterns` namespace.
- Matchers must not have public constructors — they are created only by `PathMatcherFactory`.
- `PatternScanner` must remain `internal`.
- `CompiledPattern` must not have public constructors.

**Hot-path allocation:**
- `Matching_Must_Not_Allocate` verifies that `GitIgnoreInstructionMatcher.Match` produces zero heap allocations.

If your change causes any of these tests to fail, the PR will not be merged until the contract is restored.

---

## Pull Request Checklist

Before opening a pull request:

- [ ] All existing tests pass: `dotnet test -c Release`
- [ ] Architecture tests pass: `dotnet test -c Release --filter "FullyQualifiedName~Architecture"`
- [ ] New behaviour is covered by at least one test
- [ ] Fixed bugs are covered by a regression test in `Tests/Regression/`
- [ ] Public API changes have XML documentation comments
- [ ] No new allocations in the matching hot path
- [ ] `CHANGELOG.md` is updated with a summary of the change

---

## Coding Standards

The project uses C# 14 with:
- File-scoped namespaces
- Primary constructors where appropriate
- Collection expressions (`[.. items]`)
- Pattern matching (`is`, `switch` expressions, `and`/`or`/`not` patterns)
- Raw string literals for multi-line strings

All public members require XML documentation comments. The project is configured with `GenerateDocumentationFile = true` and docfx publishes API reference from the XML output.

**No `#region` blocks.** Organize code with blank lines and comments instead.

**No `throw new Exception(...)`.** Use `PatternException` for pattern-related errors and the appropriate BCL exception for everything else.

---

## Adding a New Pattern Invariant

1. Create a class in `src/Jeninnet.FileQuery/Patterns/Invariants/` implementing `IPatternInvariant`.
2. Set `Phase` to the correct phase (`Lexical`, `Structural`, or `Semantic`).
3. Set `AppliesTo` to the pattern kind(s) it applies to, or `null` for all kinds.
4. Register it in `PatternPipeline.CreateDefault()`.
5. Add tests in `tests/Jeninnet.FileQuery.Tests/Invariants/`.
6. Document it in `docs/architecture/matcher-invariants.md`.

---

## Adding a New Benchmark

Add a class to `benchmarks/Jeninnet.FileQuery.Benchmarks/` following the pattern of existing benchmarks:

```csharp
[MemoryDiagnoser]
public class MyBenchmark {
    [GlobalSetup]
    public void Setup() { /* initialize state */ }

    [Benchmark]
    public bool MyOperation() { /* hot-path code */ return result; }
}
```

Register the class in `Program.cs` by adding it to the `BenchmarkRunner.Run(new[] { ... })` array.

---

## Reporting Issues

Use the GitHub Issues tab. For bug reports, include:
- The pattern list that produces unexpected behaviour
- The directory structure (or a minimal reproduction)
- The expected result
- The actual result
- The OS and .NET runtime version

For performance regressions, include BenchmarkDotNet output from before and after the regression.

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.