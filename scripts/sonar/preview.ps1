<#
.SYNOPSIS
    Previews SonarCloud analysis locally before pushing to GitHub.

.DESCRIPTION
    Runs the same SonarScanner analysis configured in
    .github/workflows/sonarcloud.yml from your machine, then prints the
    open issues and the Quality Gate status that CI would report.

    Two modes are available:

    -FetchIssues
        Queries SonarCloud only. Read-only, no local analysis is run.

    Default
        Builds the solution, runs the analysis, then prints the report.
        Tests and coverage are skipped unless -WithCoverage is specified.

    Notes:
    - Requires a SonarCloud token (-Token or the SONAR_TOKEN environment
      variable). Create a User Token at:
      SonarCloud > My Account > Security > Tokens
    - The SonarCloud Free plan only accepts main branch analyses, and a
      local analysis replaces the latest main branch snapshot.
      The next CI run restores it.

.PARAMETER FetchIssues
    Fetches the open issues currently reported by SonarCloud
    without running a local analysis.

.PARAMETER WithCoverage
    Runs the test suite with coverage like CI does.
    Required to preview the coverage condition of the Quality Gate.

.PARAMETER FailOnQualityGate
    Exits with code 1 when the Quality Gate status is not OK,
    simulating sonar.qualitygate.wait=true used by CI.

.PARAMETER Token
    SonarCloud token.
    Defaults to the SONAR_TOKEN environment variable.

.EXAMPLE
    .\preview.ps1 -FetchIssues

    Lists the open issues currently stored in SonarCloud.

.EXAMPLE
    .\preview.ps1

    Builds and analyzes the working copy, then prints the report.

.EXAMPLE
    .\preview.ps1 -WithCoverage -FailOnQualityGate

    Full local simulation of the CI quality gate.

.NOTES
    Author:
        Tarek Najem

    Tool:
        Jeninnet SonarCloud Preview Tool
#>


[CmdletBinding()]
param(

    [switch]
    $FetchIssues,


    [switch]
    $WithCoverage,


    [switch]
    $FailOnQualityGate,


    [string]
    $Token = $env:SONAR_TOKEN
)


Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"


$script:Organization = "jeninnet-file-query"
$script:ProjectKey = "jeninnet-file-query"
$script:SonarHostUrl = "https://sonarcloud.io"


Import-Module -Name (Join-Path $PSScriptRoot ".." "common" "Common.psd1") -Force


function Get-SonarApi {

    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [string]$Query
    )


    $uri = "$script:SonarHostUrl/$Path"

    if (-not [string]::IsNullOrEmpty($Query)) {
        $uri = "$uri`?$Query"
    }


    $attempts = @(
        "Bearer $Token",
        "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${Token}:")))"
    )


    foreach ($attempt in $attempts) {

        try {

            $headers = @{ Authorization = $attempt }

            return Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
        }
        catch {

            $statusCode = 0

            if ($null -ne $_.Exception.Response) {
                $statusCode = [int]$_.Exception.Response.StatusCode
            }

            if ($statusCode -eq 401 -or $statusCode -eq 403) {
                continue
            }

            throw "SonarCloud API request failed: $($_.Exception.Message)"
        }
    }

    throw "SonarCloud rejected the token. Create a User Token at SonarCloud > My Account > Security > Tokens, then pass it with -Token or `$env:SONAR_TOKEN."
}


function Test-SonarToken {

    $result = Get-SonarApi -Path "api/authentication/validate"

    if (-not $result.valid) {
        throw "The SonarCloud token is not valid. Create a User Token at SonarCloud > My Account > Security > Tokens."
    }
}


function Get-SonarIssueList {

    param(
        [string]
        $BranchName = "main"
    )


    $query = "componentKeys=$script:ProjectKey&resolved=false&ps=500&branch=$BranchName"
    $result = Get-SonarApi -Path "api/issues/search" -Query $query

    return , @($result.issues)
}


function Get-SonarQualityGate {

    param(
        [string]
        $BranchName = "main"
    )


    $query = "projectKey=$script:ProjectKey&branch=$BranchName"

    foreach ($attempt in 1..6) {

        $result = Get-SonarApi -Path "api/qualitygates/project_status" -Query $query
        $status = $result.projectStatus.status

        if ($status -ne "WAITING") {
            return $result.projectStatus
        }

        Start-Sleep -Seconds 5
    }

    return $result.projectStatus
}


function Wait-SonarAnalysis {

    param(
        [int]
        $TimeoutSeconds = 120
    )


    Write-Step "Waiting for SonarCloud to process the analysis..."


    Start-Sleep -Seconds 5

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {

        $activity = Get-SonarApi -Path "api/ce/activity" -Query "component=$script:ProjectKey&ps=10"

        $tasks = @()

        if ($activity.PSObject.Properties["tasks"] -and $activity.tasks) {
            $tasks = @($activity.tasks)
        }

        $activeTask = $tasks | Where-Object {
            $_.status -in @("PENDING", "IN_PROGRESS", "RUNNING")
        } | Select-Object -First 1

        if ($null -eq $activeTask) {
            return
        }

        Start-Sleep -Seconds 5
    }

    Write-Host "Timed out waiting for SonarCloud processing. Results may be stale." -ForegroundColor Yellow
}


function Write-IssueList {

    param(
        [array]
        $Issues
    )


    if ($Issues.Count -eq 0) {

        Write-Host ""
        Write-Host "No open issues found." -ForegroundColor Green

        return
    }


    $sorted = $Issues | Sort-Object -Property component, line

    foreach ($issue in $sorted) {

        $file = $issue.component -replace "^$($script:ProjectKey):", ""

        $line = ""

        if ($issue.PSObject.Properties["line"] -and $issue.line) {
            $line = ":$($issue.line)"
        }

        $color = switch ($issue.severity) {
            "BLOCKER" { "Red" }
            "CRITICAL" { "Red" }
            "MAJOR" { "Yellow" }
            "MINOR" { "DarkYellow" }
            default { "Gray" }
        }

        Write-Host ""
        Write-Host "$file$line" -ForegroundColor Cyan -NoNewline
        Write-Host "  [$($issue.severity)] $($issue.rule)" -ForegroundColor $color
        Write-Host "  $($issue.message)"
    }


    Write-Host ""
    Write-Host "Open issues: $($Issues.Count)" -ForegroundColor Cyan

    foreach ($group in ($Issues | Group-Object severity | Sort-Object Name)) {
        Write-Host ("  {0,-10}: {1}" -f $group.Name, $group.Count)
    }
}


function Write-QualityGate {

    param(
        [Parameter(Mandatory)]
        $Gate
    )


    $color = if ($Gate.status -eq "OK") { "Green" } else { "Red" }

    Write-Host ""
    Write-Host "Quality Gate: $($Gate.status)" -ForegroundColor $color


    foreach ($condition in $Gate.conditions) {

        $passed = $condition.status -eq "OK"
        $symbol = if ($passed) { "OK  " } else { "FAIL" }
        $conditionColor = if ($passed) { "Green" } else { "Red" }
        $actual = ""

        if ($condition.PSObject.Properties["actualValue"] -and $condition.actualValue) {
            $actual = "$($condition.actualValue)"
        }

        Write-Host ("  [{0}] {1}: {2} (threshold {3})" -f `
                $symbol, $condition.metricKey, $actual, $condition.errorThreshold) -ForegroundColor $conditionColor
    }
}


function Test-JavaVersion {

    param(
        [Parameter(Mandatory)]
        [string]$JavaExe,


        [int]
        $MinimumMajor = 17
    )


    $versionOutput = & $JavaExe -version 2>&1 | Out-String

    if ($versionOutput -match 'version "(?<major>\d+)') {
        return [int]$Matches.major -ge $MinimumMajor
    }

    return $false
}


function Resolve-JavaHome {

    $candidates = @()


    if ($env:JAVA_HOME) {
        $candidates += $env:JAVA_HOME
    }


    $patterns = @()


    if (${env:ProgramFiles}) {

        $patterns += @(
            (Join-Path ${env:ProgramFiles} "Android\openjdk\jdk-*"),
            (Join-Path ${env:ProgramFiles} "Microsoft\jdk-*"),
            (Join-Path ${env:ProgramFiles} "Eclipse Adoptium\jdk-*"),
            (Join-Path ${env:ProgramFiles} "Amazon Corretto\jdk*"),
            (Join-Path ${env:ProgramFiles} "Zulu\zulu-*")
        )
    }


    if ($env:LOCALAPPDATA) {
        $patterns += (Join-Path $env:LOCALAPPDATA "Programs\Android Studio\jbr")
    }


    if ($env:USERPROFILE) {
        $patterns += (Join-Path $env:USERPROFILE ".jdks\*")
    }


    foreach ($pattern in $patterns) {

        foreach ($item in @(Get-Item $pattern -ErrorAction SilentlyContinue)) {

            if ($null -ne $item) {
                $candidates += $item.FullName
            }
        }
    }


    $candidates = $candidates | Where-Object { $_ } | Select-Object -Unique

    foreach ($candidate in $candidates) {

        $javaExe = Join-Path $candidate "bin\java.exe"

        if ((Test-Path $javaExe) -and (Test-JavaVersion -JavaExe $javaExe)) {
            return $candidate
        }
    }

    throw "Java 17 or newer is required by SonarScanner but was not found. Install it with: winget install Microsoft.OpenJDK.21"
}


function Confirm-SonarScanner {

    $toolsDirectory = Join-Path ([Environment]::GetFolderPath("UserProfile")) ".dotnet\tools"
    $scannerShim = Join-Path $toolsDirectory "dotnet-sonarscanner.exe"

    if (Test-Path $scannerShim) {
        return
    }


    Write-Step "Installing SonarScanner for .NET..."

    & dotnet tool install --global dotnet-sonarscanner

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install dotnet-sonarscanner."
    }


    if (($env:Path -split [IO.Path]::PathSeparator) -notcontains $toolsDirectory) {
        $env:Path = "$toolsDirectory$([IO.Path]::PathSeparator)$env:Path"
    }
}


function Invoke-DotNetCommand {

    param(
        [Parameter(Mandatory)]
        [string]$Label,

        [Parameter(Mandatory)]
        [string[]]$Arguments
    )


    Write-Step "$Label..."

    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed (exit code $LASTEXITCODE)."
    }
}


try {

    Write-ToolBanner -Name "SonarCloud Preview Tool"


    if ([string]::IsNullOrWhiteSpace($Token)) {

        throw "No SonarCloud token found. Set `$env:SONAR_TOKEN or pass -Token. Create a User Token at SonarCloud > My Account > Security > Tokens."
    }


    Test-SonarToken

    Write-Step "Token accepted." -Color Green


    if ($FetchIssues) {

        Write-Section "Open Issues"
        Write-IssueList -Issues (Get-SonarIssueList)

        Write-Section "Quality Gate"
        Write-QualityGate -Gate (Get-SonarQualityGate)

        exit 0
    }


    $repositoryRoot = Find-RepositoryRoot -StartPath $PSScriptRoot

    Write-Section "Environment"
    Write-Host "Repository : $repositoryRoot"
    Write-Host "Project    : $script:ProjectKey"


    $javaHome = Resolve-JavaHome

    $env:JAVA_HOME = $javaHome
    $env:Path = (Join-Path $javaHome "bin") + [IO.Path]::PathSeparator + $env:Path

    Write-Host "JAVA_HOME  : $javaHome"


    Confirm-SonarScanner


    $currentBranch = (& git -C $repositoryRoot rev-parse --abbrev-ref HEAD 2>$null | Out-String).Trim()

    if ($currentBranch -and $currentBranch -ne "main") {

        Write-Host ""
        Write-Host "Warning: git branch is '$currentBranch'." -ForegroundColor Yellow
        Write-Host "The SonarCloud Free plan only accepts main branch analyses," -ForegroundColor Yellow
        Write-Host "so results are posted to the main branch." -ForegroundColor Yellow
    }


    Push-Location $repositoryRoot


    $resultsDirectory = Join-Path $repositoryRoot "TestResults"

    if (Test-Path $resultsDirectory) {

        Write-Step "Cleaning previous TestResults (CI starts from a clean checkout)..."
        Remove-Item -Path $resultsDirectory -Recurse -Force
    }


    $beginArguments = @(
        "sonarscanner",
        "begin",
        "/k:$script:ProjectKey",
        "/o:$script:Organization",
        "/d:sonar.host.url=$script:SonarHostUrl",
        "/d:sonar.token=$Token",
        "/d:sonar.cs.vstest.reportsPaths=TestResults/**/*.trx",
        "/d:sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml",
        "/d:sonar.coverage.exclusions=**/*.ps1",
        "/d:sonar.scm.provider=git",
        "/d:sonar.scm.forceReloadAll=true",
        "/d:sonar.exclusions=samples/**,src/**[Bb]enchmarks/**,**/bin/**,**/obj/**,**/TestResults/**",
        "/d:sonar.test.exclusions=tests/**"
    )

    $failureMessage = ""
    $analysisStarted = $false


    try {

        Invoke-DotNetCommand -Label "Begin SonarCloud analysis" -Arguments $beginArguments

        $analysisStarted = $true


        try {

            Invoke-DotNetCommand -Label "Restore dependencies" -Arguments @("restore")

            Invoke-DotNetCommand -Label "Build solution" -Arguments @("build", "-c", "Release", "--no-restore")


            if ($WithCoverage) {

                Invoke-DotNetCommand -Label "Run tests with coverage" -Arguments @(
                    "test",
                    "-c", "Release",
                    "--no-build",
                    "--settings", "tests/Jeninnet.FileQuery.Tests/.runsettings",
                    "--collect:XPlat Code Coverage",
                    "--logger", "trx;LogFileName=results.trx",
                    "--results-directory", "TestResults"
                )
            }
            else {

                Write-Step "Tests skipped (use -WithCoverage to include them)."
            }
        }
        catch {

            $failureMessage = $_.Exception.Message
        }
    }
    finally {

        if ($analysisStarted) {

            Write-Step "End SonarCloud analysis..."

            & dotnet sonarscanner end "/d:sonar.token=$Token"

            if ($LASTEXITCODE -ne 0 -and [string]::IsNullOrEmpty($failureMessage)) {
                $failureMessage = "SonarScanner end failed (exit code $LASTEXITCODE)."
            }
        }
    }


    Pop-Location


    if (-not [string]::IsNullOrEmpty($failureMessage)) {
        throw $failureMessage
    }


    Wait-SonarAnalysis


    Write-Section "Open Issues"
    $issues = Get-SonarIssueList
    Write-IssueList -Issues $issues


    Write-Section "Quality Gate"
    $gate = Get-SonarQualityGate
    Write-QualityGate -Gate $gate


    Write-Summary -Items @{
        "Project"      = $script:ProjectKey
        "Open Issues"  = $issues.Count
        "Quality Gate" = $gate.status
    }


    Write-Host ""
    Write-Host "Dashboard:" -ForegroundColor Cyan
    Write-Host "  https://sonarcloud.io/project/issues?id=$script:ProjectKey"
    Write-Host "  https://sonarcloud.io/summary/new_code?id=$script:ProjectKey"


    if ($FailOnQualityGate -and $gate.status -ne "OK") {
        exit 1
    }

    exit 0
}
catch {

    Write-Host ""
    Write-Host "SonarCloud preview failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red

    exit 1
}
