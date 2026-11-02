# Badge Generator

Generates the SVGs referenced by the root README. Badges are derived from build/test/report data,
not hand-written.

## Run

```powershell
# Full run: build + architecture gate + badges
pwsh scripts/badges/generate.ps1

# Re-render from the last build and coverage report
pwsh scripts/badges/generate.ps1 -SkipTests
```

## Outputs

| Badge | Source |
|---|---|
| `badges/architecture.svg` | Architecture test gate (`FullyQualifiedName~Architecture`) |
| `badges/frameworks.svg` | `TargetFramework` in `Directory.Build.props` |
| `badges/packages.svg` | Documented project count in `docs/docfx.json` |
| `badges/coverage.svg` | `Line coverage` in `artifacts/coverage/Summary.txt` (or `not run`) |

The root README displays all four. `coverage.svg` requires a prior coverage run:

```powershell
pwsh scripts/coverage/coverage.ps1 -Execute -Clean -Restore -InstallTools
pwsh scripts/badges/generate.ps1 -SkipTests
```

## Rules

- Do not edit the SVGs by hand; rerun the script.
- `-SkipTests` reuses the last build and coverage report.
- If the architecture gate fails, the script does not render green badges.
- Badge files are LF and UTF-8 without BOM (see `.gitattributes`).
