# Migration Guide

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

