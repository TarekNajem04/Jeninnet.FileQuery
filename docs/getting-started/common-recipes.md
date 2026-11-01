# Common Recipes

Copy-paste starting points for the tasks users ask for most. All examples use the current
builder API; add `using Jeninnet.FileQuery;` and, when using `PatternKind`,
`using Jeninnet.FileQuery.Enums;`.

---

## Find all C# files

```csharp
var files = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .Execute();
```

## Exclude build output

```csharp
var files = FileQuery.From(root)
    .Where("bin/", "obj/", "!**/*.cs")
    .Execute();
```

Directory-only rules end with `/` and prune the subtree.

## Respect `.gitignore`

```csharp
var files = FileQuery.From(repositoryRoot)
    .UseGitIgnoreFiles()
    .Execute();
```

Add explicit patterns after `UseGitIgnoreFiles()` when you need project-specific overrides.

## Find large or recently modified files

```csharp
var entries = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.log"])
    .MinSize(10 * 1024 * 1024)
    .ModifiedAfter(DateTimeOffset.UtcNow.AddDays(-7))
    .ExecuteEntries();
```

## Include directories as well as files

```csharp
var entries = FileQuery.From(root)
    .IncludeDirectories()
    .ExecuteEntries();
```

## Find only immediate children

```csharp
var files = FileQuery.From(root)
    .WithoutRecursion()
    .Execute();
```

## Limit depth or switch traversal strategy

```csharp
var files = FileQuery.From(root)
    .WithMaxDepth(2)          // root + two levels; -1 = unlimited
    .WithTraversal(new TraversalOptions(
        Strategy: TraversalStrategy.BreadthFirst,
        SymlinkPolicy: SymlinkPolicy.Ignore))
    .Execute();
```

Use `FollowWithCycleDetection` when the tree may contain symlink cycles.

## Case-insensitive extensions

```csharp
var files = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.jpg", "**/*.jpeg", "**/*.png"])
    .IgnoreCase()
    .Execute();
```

## Match several extensions with braces

```csharp
var files = FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.{cs,vb,fs}"])
    .Execute();
```

## Query several roots in one stream

```csharp
var batch = FileQueryBatch.From(
    FileQuery.From(firstRoot).UsingGlob().Where(PatternKind.Glob, ["**/*.cs"]),
    FileQuery.From(secondRoot).UsingGlob().Where(PatternKind.Glob, ["**/*.cs"]));

var entries = batch.ExecuteEntries();
```

## Match a path without touching the disk

```csharp
var patterns = FileQueryPatternSet.Compile(
    ["**", "!**/*.cs", "!bin/"]);

bool isSource = patterns.IsMatch("src/Program.cs");
```

## Query changed files only

```csharp
var files = FileQuery.From(repositoryRoot)
    .UsingGitIgnore()
    .RestrictToPaths(changedPaths)
    .Execute();
```

## Stream results with cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

await foreach (var file in FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.cs"])
    .ExecuteAsync(cts.Token))
{
    await ProcessAsync(file, cts.Token);
}
```

## Query an ASP.NET Core provider

```csharp
using var provider = new PhysicalFileProvider(webRoot);

var files = FileProviderQuery.From(provider)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/*.css"])
    .Execute();
```

## Anchor a regex to the root

```csharp
var files = FileQuery.From(root)
    .UsingRegex()
    .Where("r:^src/.*\\.cs$")
    .Execute();
```

## Surface access errors instead of skipping

```csharp
var files = FileQuery.From(root)
    .IgnoreInaccessible(false)
    .Execute();
```

The default is skip-and-continue. Disabling it selects `Abort` unless you configure
`WithErrorRecovery(...)`.

## Tune error recovery

```csharp
var files = FileQuery.From(root)
    .WithErrorRecovery(FileQueryErrorRecoveryOptions.Retry(2))
    .Execute();
```

`Skip`, `Retry(n)`, and `Abort` are supported.

---

## Related

- [Quick start](quick-start.md)
- [Entry metadata and filters](../guides/entry-metadata-and-filters.md)
- [Git ignore files](../guides/gitignore-files.md)
- [Multiple roots](../guides/multiple-roots.md)
- [Error handling and validation](../guides/error-handling-and-validation.md)
