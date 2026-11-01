# Jeninnet FileQuery GitHub Action

Run `dotnet-filequery` in a workflow and expose the matched files and count as step outputs.
The composite action installs or reuses the tool, runs a deterministic query, writes a job summary,
and optionally writes the file list to disk.

## Usage

```yaml
permissions:
  contents: read

jobs:
  changed-csharp-files:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v7
        with:
          fetch-depth: 0 # required for changed-since

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

Pin `@main` to a release tag or commit in production workflows.

## Inputs

| Input | Default | Description |
|---|---|---|
| `patterns` | empty (every file) | Semicolon-separated patterns |
| `root` | `.` | Directory to query, relative to the workspace |
| `changed-since` | empty | Only files changed since a Git revision; requires `fetch-depth: 0` |
| `changed-since-date` | empty | Only files changed since an ISO 8601 timestamp |
| `version` | latest | `Jeninnet.FileQuery.Tool` version to install |
| `source` | empty | Additional NuGet source for the tool package |
| `output-file` | empty | Optional path that receives matched files, one per line |

## Outputs

| Output | Description |
|---|---|
| `files` | Newline-separated matching paths, relative to the root |
| `count` | Number of matching files |

The action also writes a short summary (count and first 50 files) to the job summary.

## How it works

1. `actions/setup-dotnet` installs the .NET 10 SDK.
2. `dotnet tool install --global Jeninnet.FileQuery.Tool` runs unless `dotnet-filequery` is already
   on `PATH`. `--version` and `--add-source` are forwarded.
3. The tool runs with `--root`, `--relative`, the optional diff baseline, and the patterns.
4. The action captures stdout, derives `count`, and exposes the result through `$GITHUB_OUTPUT`.

## Notes

- The action runs on Linux/macOS shells; Windows runners are not supported by the bash steps.
- Diff-aware queries require full Git history (`fetch-depth: 0`).
- The tool version should be pinned for reproducible CI.
- No secrets are required.

## Related

- [GitHub Action guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/integrations.md)
- [Global tool guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/global-tool.md)
- [Root README](../../../README.md)
