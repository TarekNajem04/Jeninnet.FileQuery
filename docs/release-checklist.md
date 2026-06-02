# Release Checklist

Use this checklist before tagging a release.

## Required Gates

- `dotnet restore`
- `dotnet build -c Release --no-restore`
- `dotnet test -c Release --no-build`
- `dotnet format --verify-no-changes`
- DocFX build succeeds with warnings as errors.
- Benchmark dry-run succeeds in CI.
- Full BenchmarkDotNet baseline is captured for release candidates.
- NuGet packages are packed with symbols, README, license, SourceLink, and repository metadata.
- Tag version matches package version.
- `CHANGELOG.md` contains release notes and any breaking-change migration notes.

## Release Steps

1. Confirm all CI, docs, security, and package validation jobs are green.
2. Run the full benchmark suite locally in Release mode.
3. Create a SemVer tag such as `v1.0.0`.
4. Review the draft GitHub release before publishing it.
