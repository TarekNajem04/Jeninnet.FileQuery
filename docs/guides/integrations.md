# Ecosystem Integrations

This page inventories the integration targets for Jeninnet.FileQuery, records the priority
decisions, and documents the first shipped integration: a composite GitHub Action that runs the
global tool inside a workflow and exposes the results as step outputs.

---

## Inventory

| Target | Priority | Status | Rationale |
|---|---|---|---|
| GitHub Actions (composite action) | High | **Shipped** | No new dependency: the action composes the published tool + `setup-dotnet`, and CI jobs can consume `files`/`count` directly. Prototyped in this repository's CI. |
| MSBuild task | High | Candidate | Incremental builds and pattern validation belong in evaluation; needs a package referencing `Microsoft.Build.Utilities.Core` and a documented `UsingTask`. |
| SARIF exporter | Medium | Candidate | Code-scanning dashboards need a SARIF projection of validation diagnostics; the engine currently returns plain messages. |
| Azure DevOps task | Medium | Candidate | Same value as the GitHub Action, but requires extension packaging and Marketplace publishing. |
| Repository analysis tools | Low | Candidate | No stable plugin contract identified; revisit when one emerges. |
| pre-commit hook | Low | Not planned | Patterns are supplied per invocation, so a hook would need to know each project's intent; the CLI already covers the validation step. |

The common design rule: integrations are **thin wrappers over the CLI or the Tooling SDK**. They
must not fork pattern semantics, and they must pin the tool version for reproducibility.

---

## GitHub Action

The action lives at `.github/actions/filequery` and is usable from any workflow:

```yaml
jobs:
  changed-files:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v7
        with:
          fetch-depth: 0 # required for --changed-since

      - name: Collect changed C# files
        id: files
        uses: TarekNajem04/Jeninnet.FileQuery/.github/actions/filequery@main
        with:
          patterns: '**;!*.cs'
          changed-since: origin/main
          version: '1.5.0'

      - name: Use the result
        run: |
          echo "Matched ${{ steps.files.outputs.count }} file(s)"
          echo "${{ steps.files.outputs.files }}"
```

Pin `@main` to a release tag or commit in real workflows.

### Inputs

| Input | Default | Description |
|---|---|---|
| `patterns` | empty (every file) | Semicolon-separated patterns. |
| `root` | `.` | Directory to query, relative to the workspace. |
| `changed-since` | empty | Only files changed since a Git revision; requires `fetch-depth: 0`. |
| `changed-since-date` | empty | Only files changed since an ISO 8601 timestamp. |
| `version` | latest | Version of `Jeninnet.FileQuery.Tool` to install. |
| `source` | empty | Additional NuGet source (used by the self-test with a locally packed tool). |
| `output-file` | empty | Optional path that receives the matched files. |

### Outputs

| Output | Description |
|---|---|
| `files` | Newline-separated matching paths, relative to the root. |
| `count` | Number of matching files. |

The action also writes a short summary (count + first 50 files) to the job summary.

### How it works

1. `actions/setup-dotnet` installs the .NET 10 SDK.
2. `dotnet tool install --global Jeninnet.FileQuery.Tool` runs unless `dotnet-filequery` is
   already on `PATH`. `--version` and `--add-source` are forwarded.
3. The tool runs with `--root`, `--relative`, the optional diff baseline, and the pattern.
4. The result is captured in `$RUNNER_TEMP`, exposed through `$GITHUB_OUTPUT`, and summarized.

The action is exercised end to end by the `filequery-action` CI job, which packs the tool locally,
installs it from that source, runs the action, and asserts the outputs.

### Limitations

- **Shell**: the composite steps use `bash`. GitHub-hosted Linux, macOS, and Windows runners all
  provide it, but minimal self-hosted runners may not.
- **Global install**: the tool is installed into `~/.dotnet/tools` and added to `PATH` for later
  steps. A cached install is a possible follow-up.
- **Newline-delimited output**: paths are newline-separated (the tool's `--null` mode is not used
  here), so pathological file names containing newlines are not represented faithfully.
- **Diff baselines** need a full checkout (`fetch-depth: 0`) so the baseline revision exists.
- **No SARIF** yet: validation results are plain messages, not code-scanning annotations.

---

## Next steps

1. **MSBuild task**: ship `Jeninnet.FileQuery.MSBuild` with a `ValidatePatterns` task and an
   `UsingTask` sample, reusing the Tooling SDK for classification/validation.
2. **SARIF exporter**: map `PatternValidation.Error` to a SARIF result, then feed
   `github/codeql-action/upload-sarif`.
3. **Azure DevOps task**: wrap the same CLI contract as the GitHub Action.
4. **Caching**: add an optional tool cache to the action to skip the install step on warm runners.

Each candidate should meet the same bar as the GitHub Action: no forked semantics, a pinned tool
version, an end-to-end test in this repository, and documented limitations.

---

## See also

- [Global Tool](global-tool.md) — the CLI contract used by the action.
- [Diff-Aware Queries](diff-aware-queries.md) — the `changed-since` baseline semantics.
- [Build Tooling SDK](tooling-sdk.md) — the facade intended for MSBuild and analyzer integrations.
