# Sonar preview tooling

`preview.ps1` runs a SonarScanner analysis locally (against SonarCloud or a local SonarQube
server), then prints the open issues and the Quality Gate status.

Two targets are supported:

- **SonarCloud** (default) — needs a User Token; the Free plan only accepts main-branch analyses.
- **Local SonarQube** (`-Local`) — no cloud account needed; ideal before the repository is on GitHub.

## Token selection

| Target | Default token source | Override |
|---|---|---|
| SonarCloud (default) | `SONAR_TOKEN` | `-Token` |
| Local SonarQube (`-Local`) | `SONAR_TOKEN_LOCAL` | `-Token` |

An explicit `-Token` always wins. Create a User Token at
`<server> > My Account > Security > Tokens` (a fresh local SonarQube server logs in with
`admin`/`admin` until the password is changed).

## Prerequisites

- .NET SDK 10; the script installs `dotnet-sonarscanner` automatically when it is missing.
- Java 17 or newer (required by SonarScanner); the script resolves a JDK automatically from
  `SONAR_JAVA_PATH` (path to `java.exe`), `JAVA_HOME`, and the common install locations.
- Local mode: SonarQube Community Build running at <http://localhost:9000> (default).
  Current Community Build releases require Java 21 for the server itself; `StartSonar.bat` reads
  `SONAR_JAVA_PATH` (or the first `java.exe` on `PATH`), so set it explicitly when an older JDK
  is first on `PATH`.

## Local SonarQube (Community Build)

1. Download and unzip SonarQube Community Build (Java 21 required by current releases):
   <https://www.sonarsource.com/products/sonarqube/downloads/>
2. Start the server (point `JAVA_HOME` or the `PATH` at your JDK when Java is not on `PATH`):

   ```powershell
   $env:JAVA_HOME = "C:\Program Files\Android\openjdk\jdk-21.0.8"
   & "$env:USERPROFILE\Tools\SonarQube\bin\windows-x86-64\StartSonar.bat"
   ```

3. Open <http://localhost:9000> (first login `admin`/`admin`), change the password, then
   create a token: **My Account > Security > Generate Tokens**.
4. Stop the server:

   ```powershell
   Get-CimInstance Win32_Process -Filter "Name='java.exe'" |
       Where-Object { $_.CommandLine -match 'sonar-application' } |
       ForEach-Object { Stop-Process -Id $_.ProcessId }
   ```

## Usage

```powershell
# SonarCloud (default) — token from SONAR_TOKEN
$env:SONAR_TOKEN = "<cloud token>"
.\scripts\sonar\preview.ps1

# Local SonarQube — token from SONAR_TOKEN_LOCAL
$env:SONAR_TOKEN_LOCAL = "<local token>"
.\scripts\sonar\preview.ps1 -Local

# Local with tests and coverage (full CI simulation)
.\scripts\sonar\preview.ps1 -Local -WithCoverage

# Only print the issues and Quality Gate currently stored on the server
.\scripts\sonar\preview.ps1 -FetchIssues

# Write the open issues to a working file (default: artifacts/sonar-issues.txt)
.\scripts\sonar\preview.ps1 -FetchIssues -ExportIssues

# ... or to an explicit path
.\scripts\sonar\preview.ps1 -FetchIssues -ExportIssues -ExportPath .\issues.txt

# Fail (exit 1) when the Quality Gate is not OK
.\scripts\sonar\preview.ps1 -Local -FailOnQualityGate
```

`-ServerUrl` and `-ProjectKey` override the defaults
(SonarCloud / `jeninnet-file-query`; with `-Local` the server URL defaults to
`http://localhost:9000`).

## Notes

- `-ExportIssues` writes one line per issue in the
  `<file>(<line>): <rule> [<severity>/<type>] <message>` shape plus a per-rule summary, so the
  list can be worked through offline (like a VS Error List export). The default path is
  `artifacts/sonar-issues.txt`.
- Coverage requires `coverlet.collector` (referenced by the test projects) and the
  `tests/coverage.opencover.runsettings` file; the script passes both to `dotnet test`.
- The Soak and AotSmoke harness projects are excluded from the coverage metric because they are
  executable harnesses, not shipped libraries; they remain part of the issue analysis.
- The scanner honors `.editorconfig` severities, so rules pinned there stay silent both
  locally and in CI.
- The main-branch warning applies to SonarCloud only. A local SonarQube server analyzes the
  working copy and has no branch concept, so branch parameters are omitted in local mode.

## Local baseline

The 2026-11-02 local run used SonarQube Community Build 26.10 with `-Local -WithCoverage
-FailOnQualityGate -ExportIssues`:

| Metric | Value |
|---|---|
| Open issues | **0** |
| Quality Gate | **OK** |
| Sonar coverage | **85.9%** (87.9% lines, 81.9% branches) |
| Maintained lines (ncloc) | 9,059 |
| Covered / uncovered lines | 4,504 / 618 |
| Duplicated lines | **0.0%** (0 lines, 0 blocks) |
| Bugs / vulnerabilities / code smells | 0 / 0 / 0 |

The Sonar server reuses the `SonarAnalyzer.CSharp` rules that already run during `dotnet build`;
a clean zero-warning build therefore normally produces zero Sonar issues. Use `-FetchIssues
-ExportIssues` after changing code to re-read the server without rerunning the analysis.

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `UnsupportedClassVersionError` when starting SonarQube | `StartSonar.bat` picked an older `java.exe` from `PATH` | Set `SONAR_JAVA_PATH` to the Java 21 `java.exe` used by the server |
| `MissingObjectException: Missing blob` at the SCM publisher step | The local Git object store is incomplete after history rewriting | Run `git repack -adf`, then rerun the preview |
| Quality Gate `NONE` on the first query | The compute engine has not finished the first analysis | Wait for the scanner task, then rerun with `-FetchIssues` |
