# Coverage Tool

Runs the test suite with XPlat Code Coverage and generates an HTML + text coverage report with
ReportGenerator.

The script is **preview-only by default**. Pass `-Execute` to run it.

## Prerequisites

- .NET 10 SDK.
- ReportGenerator (`dotnet-reportgenerator-globaltool`).
- `coverlet.collector` in every test project that participates in coverage. All seven test
  projects in this repository reference it.

## Run

```powershell
# Preview the workflow
pwsh scripts/coverage/coverage.ps1

# Run tests, collect coverage, and generate the report
pwsh scripts/coverage/coverage.ps1 -Execute -Clean -Restore -InstallTools
```

Options:

| Option | Effect |
|---|---|
| `-Execute` | Actually run the workflow (otherwise preview only). |
| `-Clean` | Remove `TestResults` and the previous report first. |
| `-Restore` | Run `dotnet restore` before tests. |
| `-InstallTools` | Install ReportGenerator automatically if missing. |
| `-OpenReport` | Open the generated HTML report. |

## Output

```text
TestResults/
└── <run-id>/
    └── coverage.cobertura.xml
artifacts/coverage/
├── index.html
├── summary.html
└── Summary.txt
```

`Summary.txt` is the machine-readable source for the README coverage badge.

## Current baseline

Report from `2026-11-02`:

| Metric | Value |
|---|---|
| Line coverage | **89.4%** (5,852 of 6,540) |
| Branch coverage | **79.7%** (2,277 of 2,855) |
| Method coverage | **92.1%** (967 of 1,049) |
| Assemblies | 9 |
| Classes | 175 |

The report is generated from all seven test projects after the `coverlet.collector` wiring. The
core engine, CommandLine, DependencyInjection, Tool, Tooling, FileProviders, Build, and Analyzers
assemblies are included through the `+Jeninnet.FileQuery*` filter.

## Configuration

`coverage.config.json` controls:

- `coverage.resultsDirectory` — where raw Cobertura files are collected.
- `coverage.reportDirectory` — where the HTML/text report is written.
- `coverage.reportTypes` — `Html`, `HtmlSummary`, `TextSummary`.
- `reportGenerator.assemblyFilters` — assemblies included in the report.

CI uses the same collector in the canonical Ubuntu coverage leg; the script is the local
reproduction of that gate.

## Badge integration

After a coverage run:

```powershell
pwsh scripts/badges/generate.ps1 -SkipTests
```

This reads `artifacts/coverage/Summary.txt` and regenerates `badges/coverage.svg`. The root README
references all generated badges.

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `Unable to find a datacollector with friendly name 'XPlat Code Coverage'` | Test project missing `coverlet.collector` | Add the package to that test project |
| `Coverage files were not generated` | Tests failed before collection, or no collector ran | Fix the test failure and rerun |
| ReportGenerator missing | Global tool not installed | Pass `-InstallTools` or install `dotnet-reportgenerator-globaltool` |
| Badge says `not run` | No `artifacts/coverage/Summary.txt` | Run this script with `-Execute` |

Do not commit `TestResults/` or `artifacts/coverage/`; both are generated and ignored.
