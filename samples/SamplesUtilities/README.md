# SamplesUtilities

Shared infrastructure for the sample applications. It is a library, not a runnable sample.

## What it provides

| Helper | Purpose |
|---|---|
| `SampleUtils.CreateDemoTree(name)` | Creates a deterministic 12-file tree under the temp directory |
| `SampleUtils.RunDemo(...)` | Prints the query, expectations, matches, and total count |
| `SampleUtils.Cleanup(root)` | Best-effort deletion of the demo tree |

The demo tree contains:

```text
app.log
ReadMe.md
assets/logo1.png
assets/logo2.png
backup/old.txt
docs/guide.md
logs/app.log
logs/error.log
src/FileQuery.cs
src/Program.cs
src/cli/Program.cs
src/test/helpers.cs
```

Every sample uses the same tree, so the expected result counts in the sample READMEs are
comparable and reproducible on Windows, Linux, and macOS.

## Why it exists

Samples should teach one concept at a time. `SamplesUtilities` keeps fixture creation, console
formatting, and cleanup out of the sample bodies so each `Program.cs` is the shortest possible
demonstration of its feature.

It references the core, CommandLine, and DependencyInjection projects for the advanced sample.

## Related

- [All samples](../README.md)
- [AdvancedUsage](../AdvancedUsage/README.md)
