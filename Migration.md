# Migration Guide

## Upgrading to v2.0.0

v2.0.0 is a **major** release because the public `IFileQueryEngine` contract gained members without
default implementations, and several observable matching/diagnostic behaviors changed. Applications
that only use the built-in engine and the fluent builder usually need no source changes beyond the
package version bump.

### Why this is 2.0

- A third-party `IFileQueryEngine` implementation no longer compiles (and is not binary-compatible)
  until it implements the new members.
- The `FileQueryOptionsConfig` primary constructor grew from 10 to 17 parameters. Existing source
  using named arguments or the optional defaults still compiles, but precompiled consumers and
  plugins bind to the old constructor/`Deconstruct` signatures and are **not binary-compatible**.
- Regex timeout failures now use a different exception type.
- `IgnoreCase()` now folds character-class literals and ranges, which can add matches.
- Malformed-pattern diagnostics now carry a concrete reason and position.
- `dotnet-filequery --ignore-files` no longer inserts the implicit `!**` include-all fallback.
- `Specific` interpretation now rejects untyped patterns and mismatched typed dialects with
  `PatternException` instead of compiling rules that can never match.
- Empty/whitespace patterns now throw `ArgumentException`; Windows separators in Glob patterns now
  throw `PatternException`; GitIgnore directory-negation ordering and cross-dialect exclusion
  attribution now follow the documented last-match/first-include model.

Everything else in the release is additive at the source level.

---

### 1. Implement the expanded `IFileQueryEngine`

The v1.5 interface had three members:

```csharp
IEnumerable<string> Execute(FileQuery query);
IAsyncEnumerable<string> ExecuteAsync(FileQuery query, CancellationToken cancellationToken = default);
IAsyncEnumerable<string> ExecuteAsync(
    FileQuery query,
    IProgress<FileQueryProgress>? progress,
    CancellationToken cancellationToken = default);
```

v2.0 adds seven members:

```csharp
IEnumerable<string> Execute(FileQuery query, CancellationToken cancellationToken);
IEnumerable<string> Execute(
    FileQuery query,
    IProgress<FileQueryProgress>? progress,
    CancellationToken cancellationToken = default);

IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query);
IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query, CancellationToken cancellationToken);
IEnumerable<FileQueryEntry> ExecuteEntries(
    FileQuery query,
    IProgress<FileQueryProgress>? progress,
    CancellationToken cancellationToken = default);
IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
    FileQuery query,
    CancellationToken cancellationToken = default);
IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
    FileQuery query,
    IProgress<FileQueryProgress>? progress,
    CancellationToken cancellationToken = default);
```

Custom implementations must add them. If an implementation does not support entry results, throw
`NotSupportedException` from the entry methods rather than returning misleading data. The built-in
`FileQueryEngine` already implements the full contract.

### 2. Rebuild `FileQueryOptionsConfig` consumers

v1.5.0 had ten positional parameters:

```csharp
public record FileQueryOptionsConfig(
    PatternInput PatternInput,
    bool RecurseSubdirectories = true,
    int MaxRecursionDepth = FileQueryOptions.UNLIMITED_RECURSION_DEPTH,
    bool IgnoreInaccessible = true,
    PatternMatchingMode PatternMatchingMode = PatternMatchingMode.GitIgnore,
    CaseSensitivity CaseSensitivity = CaseSensitivity.PlatformDefault,
    TraversalOptions? Traversal = null,
    bool AuditMatches = false,
    IProgress<FileQueryDiagnostic>? Diagnostics = null,
    FileQueryErrorRecoveryOptions? ErrorRecovery = null);
```

v2.0.0 adds seven optional parameters (metadata/filter and ignore-file options):

```csharp
var config = new FileQueryOptionsConfig(
    PatternInput: new PatternInput(patterns: ["**", "!*.log"]),
    MinSize: 1024,
    MaxSize: 10 * 1024 * 1024,
    ModifiedAfter: DateTimeOffset.UtcNow.AddDays(-7),
    AttributesToSkip: FileAttributes.Hidden,
    GitIgnoreFiles: new GitIgnoreOptions(),
    IncludeDirectories: false);
```

Source code that uses named arguments or the optional defaults usually recompiles unchanged.
Precompiled libraries/plugins that construct or deconstruct `FileQueryOptionsConfig` must be
rebuilt against v2.0.0; binding to the old constructor or `Deconstruct` fails with
`MissingMethodException`.

### 3. Update regex timeout handling

**Before (v1.5):**

```csharp
try
{
    var files = FileQuery.From(root).UsingRegex().Where("r:(a+)+$").Execute().ToList();
}
catch(RegexMatchTimeoutException)
{
    // timeout
}
```

**After (v2.0):**

```csharp
try
{
    var files = FileQuery.From(root).UsingRegex().Where("r:(a+)+$").Execute().ToList();
}
catch(PatternException ex) when (ex.InnerException is RegexMatchTimeoutException)
{
    Console.WriteLine($"Pattern '{ex.Message}' timed out.");
}
```

The `PatternException` also carries the source pattern in its message and the timeout as
`InnerException`.

### 4. Re-check ignore-case character classes

`IgnoreCase()` now folds character-class literals and ranges:

```csharp
FileQuery.From(root)
    .UsingGlob()
    .Where(PatternKind.Glob, ["**/[a-z]*.cs"])
    .IgnoreCase()
    .Execute();
```

`[a-z]` now matches `A`–`Z` as well. POSIX categories such as `[:upper:]` and `[:lower:]` keep their
category semantics. Re-run tests that asserted the previous narrower behavior.

### 5. Use typed malformed-pattern diagnostics

The exception type remains `PatternException`, but the message now includes the validator reason and
the exception exposes `Position`/`Segment`:

```csharp
try
{
    FileQuery.From(root).UsingGlob().Where(PatternKind.Glob, ["src/[abc"]).Build();
}
catch(PatternException ex)
{
    Console.WriteLine($"{ex.Message} at position {ex.Position}, segment {ex.Segment}.");
}
```

Code that matched the old “No compiler registered for pattern type 'Unknown'” text should switch to
the position data or the new message.

### 6. Adjust CLI scripts that use `--ignore-files`

`dotnet-filequery --ignore-files` no longer inserts the implicit `!**` include-all fallback. Loaded
ignore rules can now exclude files as intended. Scripts that depended on the old fallback should add
an explicit pattern or remove the expectation that every file is included.

### 7. Align `Jeninnet.FileQuery.FileProviders`

Install the adapter and core from the same release:

```xml
<PackageReference Include="Jeninnet.FileQuery" Version="2.0.0" />
<PackageReference Include="Jeninnet.FileQuery.FileProviders" Version="2.0.0" />
```

The adapter uses friend access to the core's internal `IFileSystem`; mixing it with an older core
fails at runtime. `FileQueryBatch` remains physical-only; provider queries run one root at a time.

### 8. Switch test code to the standalone assertions package (test projects only)

The repository no longer ships `Jeninnet.Testing.Assertions`. If you referenced that package, use
the standalone [`Jeninnet.Assertions`](https://www.nuget.org/packages/Jeninnet.Assertions) 1.0.0
package and update namespaces:

```csharp
using Jeninnet.Assertions;
using Jeninnet.Assertions.IO;
```

Product packages have no dependency on either assertion package.

### 9. Review pattern validation and precedence changes

- `PatternInput` and `FileQueryBuilder.Where` reject empty or whitespace-only entries. Remove such
  entries from configuration and command lines instead of relying on them as no-ops.
- `PatternInterpretationMode.Specific` now requires typed patterns whose dialect matches the selected
  `PatternMatchingMode`. `Where(string)` assigns the forced dialect automatically when
  `UsingGitIgnore`/`UsingGlob`/`UsingRegex` is active.
- Glob patterns must use `/` as the path separator. `src\file.cs` now throws `PatternException`;
  use `src/file.cs`. A backslash before a glob metacharacter remains an escape for that literal
  character, so `src\*.cs` is not a directory pattern.
- A later explicit GitIgnore rule overrides an earlier directory-only negation. Rule sets such as
  `["**", "!keep/", "keep/file.txt"]` now exclude `keep/file.txt`, matching Git.
- A Glob or Regex fallback that does not match no longer erases a GitIgnore `Exclude`; `Explain`
  reports the GitIgnore rule and directory pruning follows the documented precedence model.
- Untyped `r:` patterns with literal or quantifier braces (for example `r:^a}$`) are treated as
  Regex syntax and are not checked by the Glob bracket/brace validator.

---

### New public surface (additive)

- `FileQueryEntry` + `ExecuteEntries()`/`ExecuteEntriesAsync()` and the `MinSize`, `MaxSize`,
  `ModifiedAfter`, `ModifiedBefore`, `AttributesToSkip` builder filters.
- `FileQueryOptionsConfig` gains `MinSize`, `MaxSize`, `ModifiedAfter`, `ModifiedBefore`,
  `AttributesToSkip`, `GitIgnoreFiles`, and `IncludeDirectories`, plus the `IncludedPaths` init
  property used by diff-aware queries.
- `IncludeDirectories()` to add directory results to the existing traversal.
- Synchronous `Execute`/`ExecuteEntries` overloads that accept `CancellationToken` and
  `IProgress<FileQueryProgress>`; the token-free overloads remain.
- `FileQueryBatch` for multiple roots, and `FileQueryEntry.RootPath` on entry results.
- `WithMaxDepth(int)` and `WithTraversal(TraversalOptions)` for depth, DFS/BFS, and symlink policy.
- `IgnoreInaccessible(bool)` for accessible-entry policy.
- `PatternException.Position`/`Segment` and `PatternValidation.Position`/`Segment` (additive; `null`
  when unknown).
- Brace alternation (`{a,b}`) in non-regex patterns; regex quantifiers are unchanged.
- `Jeninnet.FileQuery.Build` MSBuild validation/SARIF package and `SarifExporter` in the Tooling SDK.
- `Jeninnet.FileQuery.FileProviders` for querying
  `Microsoft.Extensions.FileProviders.IFileProvider` sources.
- `FileQueryPatternSet` (`IsMatch` string/span, `Explain`) for in-memory matching.
- `GitIgnoreOptions` + `UseGitIgnoreFiles()`; CLI `--ignore-files` / `--global-ignore`.
- `FileQueryTelemetry` names and built-in spans/metrics.
- `Jeninnet.FileQuery.Analyzers` NuGet package with `JNQ1001` and a code fix.
- Native AOT smoke host / CI validation.
- CLI `--json`, `--explain`, and diff-aware `--changed-since` workflows.

### What does not change

- Package IDs.
- `FileQuery.From`, `FileQueryBuilder` chaining, and the existing `Execute`/`ExecuteAsync` calls.
- .NET 10 as the target framework.
- The behavior of the default skip-on-inaccessible policy.

### Upgrade checklist

1. Update all product package references to `2.0.0`.
2. Implement the six new `IFileQueryEngine` members if you have a custom engine.
3. Rebuild code that constructs or deconstructs `FileQueryOptionsConfig` (10 → 17 parameters).
4. Replace `catch(RegexMatchTimeoutException)` with the `PatternException` pattern.
5. Re-run tests that use `IgnoreCase()` with character classes.
6. Replace message matching for malformed patterns with `Position`/`Segment`.
7. Review CLI scripts that use `--ignore-files`.
8. Keep `FileProviders` and core on the same version.
9. Move test-only assertion code to `Jeninnet.Assertions` 1.0.0.

For a full list of additions and fixes, see [`CHANGELOG.md`](CHANGELOG.md#200---2026-11-02).

## Upgrading from v1.0.0 / v1.1.0 to v1.2.0

Jeninnet.FileQuery v1.2.0 introduces a breaking change to the `FileQueryOptions` constructor. To improve configuration validation and consistency, the constructor no longer accepts individual parameters directly. Instead, it now requires a `FileQueryOptionsConfig` record.

### Code Changes

#### Before (v1.0.0 / v1.1.0)
```csharp
var fileQueryOptions = new FileQueryOptions(
     patternInput: new(
        typedPatterns: typedPatterns.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable())
    ),
    recurseSubdirectories: options.Recurse,
    maxRecursionDepth: FileQueryOptions.UNLIMITED_RECURSION_DEPTH,
    ignoreInaccessible: true,
    caseSensitivity: CaseSensitivity.PlatformDefault
);
```

#### After (v1.2.0)
You must now wrap your configuration in a `FileQueryOptionsConfig` object:

```csharp
// 1. Create the configuration object
var config = new FileQueryOptionsConfig(
    PatternInput: new(
        typedPatterns: typedPatterns.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable())
    ),
    RecurseSubdirectories: options.Recurse,
    MaxRecursionDepth: FileQueryOptions.UNLIMITED_RECURSION_DEPTH,
    IgnoreInaccessible: true,
    CaseSensitivity: CaseSensitivity.PlatformDefault
);

// 2. Pass the config to FileQueryOptions
var fileQueryOptions = new FileQueryOptions(config);
```

### Recommendation
Alternatively, you can utilize the `FileQueryBuilder` to streamline this process, which handles the internal creation of `FileQueryOptionsConfig`.

---

## Upgrading to v1.3.0 (Improved Path Validation)

Jeninnet.FileQuery v1.3.0 introduces improved cross-platform path validation prior to execution via `FileQueryValidator`.

### Behavioral Changes
The `FileQueryValidator.ValidateExecution` method now provides more descriptive validation feedback. It eagerly checks for:
- Paths containing invalid characters (`Path.GetInvalidPathChars()`).
- Paths exceeding 4096 characters.
- Malformed UNC paths (e.g., `\\C\`).

While this continues the existing behavior of enforcing strict paths, the error messages are now more explicit, helping identify configuration issues earlier in the execution pipeline.

### Migration Notes
No code changes are required for existing applications that already correctly handle exceptions during the validation phase. If your application was relying on generic exception handling, you may now see `ArgumentException` with more detailed messages, which is a positive improvement for debugging.

