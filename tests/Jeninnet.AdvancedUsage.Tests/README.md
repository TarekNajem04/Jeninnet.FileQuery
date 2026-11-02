# Jeninnet.AdvancedUsage.Tests

Tests for the `AdvancedUsage` sample's reproducible evaluation infrastructure.

**11 tests.**

## What is covered

- Dataset generator boundary conditions (zero/negative counts, depth limits, seed handling).
- Dataset capacity and topology invariants.
- Evaluation option validation and defaults.
- Deterministic dataset layout used by the sample's `--evaluate` mode.

## Run

```bash
dotnet test tests/Jeninnet.AdvancedUsage.Tests/Jeninnet.AdvancedUsage.Tests.csproj -c Release
```

## Design notes

- The tests reference the sample project directly so the evaluation code and its tests cannot drift.
- They validate configuration and dataset generation, not wall-clock benchmark numbers.
- Dataset generation uses a fixed seed and temporary directories.
- Assertions use the external `Jeninnet.Assertions` package.

## Related

- [AdvancedUsage sample](../../samples/AdvancedUsage/README.md)
- [All tests](../README.md)
