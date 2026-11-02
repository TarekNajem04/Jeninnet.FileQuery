# Release Checklist

Use this checklist before tagging a release.

## Required Gates

### Build and tests

- `dotnet restore`
- `dotnet build Jeninnet.FileQuery.slnx -c Release --no-restore`
- `dotnet test Jeninnet.FileQuery.slnx -c Release --no-build`
- `dotnet format Jeninnet.FileQuery.slnx --verify-no-changes`
- The build/test matrix is green on Windows, Ubuntu, and macOS.
- The Ubuntu-only gates are green: format, architecture, AOT smoke, analyzer package, Build
  package, soak, package-validation, VS Code extension, and FileQuery Action.
- The local sample suite runs successfully (all thirteen samples in Release mode).

### Documentation and analysis

- DocFX succeeds with warnings as errors. The generated API reference intentionally covers the
  five runtime SDK projects; the global tool, MSBuild task, and analyzer packages are documented
  by their package READMEs and guides.
- `pwsh scripts/docs/check-doc-links.ps1` passes.
- `pwsh scripts/docs/check-readme-claims.ps1` passes.
- `pwsh scripts/docs/check-api-reference.ps1` passes.
- `docs/articles/library-capability-review.md` is refreshed against the release scope.
- The local Sonar preview passes:
  `pwsh scripts/sonar/preview.ps1 -Local -WithCoverage -FailOnQualityGate -ExportIssues`
  with zero open issues, Quality Gate OK, zero duplicated lines, and the coverage condition met.
- The coverage report is regenerated and `badges/coverage.svg` is refreshed from
  `artifacts/coverage/Summary.txt`.

### Packages

- All **eight** packable projects are packed with `EnablePackageValidation=true`:
  core, CommandLine, DependencyInjection, FileProviders, Tool, Tooling, Build, and Analyzers.
  This validates package structure; it does not enforce API compatibility against v1.5.0 because
  v2.0.0 is a documented major release.
- Each package contains its README, icon, LICENSE, MIT license expression, repository metadata,
  and the expected payload (analyzer/code-fix DLLs, MSBuild task files, or global-tool files).
- Symbols (`snupkg`) are produced for the six library/tool packages; the Analyzer and Build
  packages intentionally ship their payload DLLs directly instead of a symbol package.
- Consumer smoke tests pass: install the global tool, compile against the analyzer package
  (valid pattern succeeds, malformed pattern fails with `JNQ1001`), and compile against the Build
  package (valid pattern writes SARIF; malformed pattern fails with `JNQ2001`).
- A Native AOT publish and run succeeds (CI covers `linux-x64`; `win-x64` was verified locally).
- Tag version matches every package version and `CHANGELOG.md`/`Migration.md` are current.

## Release Steps

1. Confirm every required gate above is green.
2. Run the full BenchmarkDotNet suite locally in Release mode (Medium/job configuration). The CI
   Benchmarks workflow is a `Short` cross-platform dry-run; do not treat it as the full release
   baseline.
3. Merge the release PR and confirm the protected `main` branch still passes all required checks.
4. Create a SemVer tag such as `v2.0.0` on the release commit.
5. Confirm the publish workflow verifies the tag/version match, packs all eight packages, publishes
   them with NuGet trusted publishing, and creates the GitHub release.
6. Verify the NuGet package pages show the README, icon, license, and symbols where applicable,
   then test one clean install from nuget.org.
