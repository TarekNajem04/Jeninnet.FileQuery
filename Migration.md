# Migration Guide

## Upgrading to v2.0.0 (hardening release)

v2.0.0 is a major release. The following changes matter when upgrading:

### `IFileQueryEngine` has new members

Custom engine implementations must add:

```csharp
IEnumerable<FileQueryEntry> ExecuteEntries(FileQuery query);
IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(FileQuery query, CancellationToken cancellationToken = default);
```

The built-in engine implements both. Callers that only use `Execute`/`ExecuteAsync` are unaffected.

### New public surface (additive)

- `FileQueryEntry` + `ExecuteEntries()`/`ExecuteEntriesAsync()` and the `MinSize`, `MaxSize`,
  `ModifiedAfter`, `ModifiedBefore`, `AttributesToSkip` builder filters.
- `IncludeDirectories()` to add directory results to the existing traversal.
- Synchronous `Execute`/`ExecuteEntries` overloads that accept `CancellationToken` and
  `IProgress<FileQueryProgress>`; the token-free overloads remain.
- `FileQueryBatch` for multiple roots, and `FileQueryEntry.RootPath` on entry results.
- `FileQueryPatternSet` (`IsMatch` string/span, `Explain`) for in-memory matching.
- `GitIgnoreOptions` + `UseGitIgnoreFiles()`; CLI `--ignore-files` / `--global-ignore`.
- `FileQueryTelemetry` names and built-in spans/metrics.
- `Jeninnet.FileQuery.Analyzers` NuGet package with `JNQ1001` and a code fix.
- Native AOT smoke host / CI validation.

### Behavior notes

- An escaped leading `!` (`\!name`) in a GitIgnore pattern is now a literal, matching Git.
- `dotnet-filequery --ignore-files` removes the implicit `!**` include-all fallback so loaded ignore
  rules can exclude files.
- Invalid metadata filter ranges now fail when the query is built.

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

