# Testing and Release Plan

## Test Naming

Use subject-based test files such as `GlobMatcherTests.cs` for focused units. Async file enumeration tests keep the established `EnumerateFilesAsync_*Tests.cs` convention because the prefix identifies the public async behavior under test.

## Required Test Areas

- Unit tests for tokenizers, compilers, matchers, traversal decisions, and option validation.
- Contract tests for public engine behavior and API boundaries.
- Integration tests for end-to-end file enumeration.
- Cross-platform tests for path casing, separators, inaccessible directories, and symlink cycle handling.

## Samples

Samples must build as part of release validation and demonstrate one clear scenario each. Keep sample code minimal and avoid relying on machine-specific paths.
