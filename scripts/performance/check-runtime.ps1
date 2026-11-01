#requires -Version 7
<#
.SYNOPSIS
    Checks the current .NET runtime and SDK against the recorded benchmark baseline.

.DESCRIPTION
    Compares the newest Microsoft.NETCore.App runtime with the runtime recorded in
    docs/performance/runtime-evolution.json, and verifies that the cross-platform baseline
    records the same runtime. When the runtime changed, it prints the re-baseline procedure;
    with -Enforce it also exits 1 so a pipeline can require the refresh.

.PARAMETER BaselinePath
    The runtime tracking file. Defaults to <repo>/docs/performance/runtime-evolution.json.

.PARAMETER PerformanceBaselinePath
    The cross-platform baseline. Defaults to <repo>/docs/performance/cross-platform-baseline.json.

.PARAMETER Enforce
    Exit 1 on runtime drift instead of only warning.

.OUTPUTS
    Prints the recorded and current versions. Exit codes:
      0  runtime matches (or drift without -Enforce)
      1  runtime drift with -Enforce
      2  missing dotnet, missing configuration, or inconsistent baselines

.EXAMPLE
    pwsh scripts/performance/check-runtime.ps1

.EXAMPLE
    pwsh scripts/performance/check-runtime.ps1 -Enforce
#>
param(
    [string]$BaselinePath = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path 'docs\performance\runtime-evolution.json'),

    [string]$PerformanceBaselinePath = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path 'docs\performance\cross-platform-baseline.json'),

    [switch]$Enforce
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-LatestRuntimeVersion {

    $output = & dotnet --list-runtimes 2>&1

    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    $versions = [System.Collections.Generic.List[version]]::new()

    foreach ($line in $output) {

        $match = [regex]::Match(
            [string]$line,
            '^Microsoft\.NETCore\.App\s+(?<version>\d+\.\d+\.\d+)\s')

        if ($match.Success) {
            [void]$versions.Add([version]$match.Groups["version"].Value)
        }
    }

    if ($versions.Count -eq 0) {
        return $null
    }

    return ($versions | Sort-Object -Descending | Select-Object -First 1).ToString()
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet was not found on PATH." -ForegroundColor Red
    exit 2
}

if (-not (Test-Path -LiteralPath $BaselinePath)) {
    Write-Host "Runtime tracking file not found: $BaselinePath" -ForegroundColor Red
    exit 2
}

if (-not (Test-Path -LiteralPath $PerformanceBaselinePath)) {
    Write-Host "Performance baseline not found: $PerformanceBaselinePath" -ForegroundColor Red
    exit 2
}

$tracking = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json
$performance = Get-Content -LiteralPath $PerformanceBaselinePath -Raw | ConvertFrom-Json

$recordedRuntime = [string]$tracking.runtime
$recordedPinnedSdk = [string]$tracking.pinnedSdk
$recordedLatestSdk = [string]$tracking.latestSdk
$baselineRuntime = [regex]::Match([string]$performance.runtime, '\d+\.\d+\.\d+').Groups[0].Value

if ([string]::IsNullOrWhiteSpace($recordedRuntime)) {
    Write-Host "The tracking file does not define 'runtime'." -ForegroundColor Red
    exit 2
}

if ([string]::IsNullOrWhiteSpace($baselineRuntime)) {
    Write-Host "The performance baseline does not define a runtime version." -ForegroundColor Red
    exit 2
}

if ($baselineRuntime -ne $recordedRuntime) {
    Write-Host "The tracking and performance baselines disagree on the runtime:" -ForegroundColor Red
    Write-Host "  runtime-evolution.json : $recordedRuntime"
    Write-Host "  cross-platform-baseline.json : $baselineRuntime"
    exit 2
}

$currentRuntime = Get-LatestRuntimeVersion

if ([string]::IsNullOrWhiteSpace($currentRuntime)) {
    Write-Host "Unable to determine the installed Microsoft.NETCore.App runtime." -ForegroundColor Red
    exit 2
}

$currentSdk = (& dotnet --version).Trim()

Write-Host "Recorded runtime : $recordedRuntime"
Write-Host "Current runtime  : $currentRuntime"
Write-Host "Pinned SDK       : $recordedPinnedSdk (global.json)"
Write-Host "Recorded SDK     : $recordedLatestSdk"
Write-Host "Current SDK      : $currentSdk"
Write-Host "History entries  : $($tracking.history.Count)"
Write-Host ""

if ($currentSdk -ne $recordedLatestSdk) {
    Write-Host "SDK drift detected (recorded $recordedLatestSdk, current $currentSdk)." -ForegroundColor Yellow
    Write-Host "Update the 'latestSdk' value after refreshing the baseline." -ForegroundColor Yellow
    Write-Host ""
}

if ($currentRuntime -ne $recordedRuntime) {

    Write-Host "Runtime drift detected: recorded $recordedRuntime, current $currentRuntime." -ForegroundColor Yellow
    Write-Host "Re-baseline procedure:"
    Write-Host "  1. dotnet run --project src/Jeninnet.FileQuery.Benchmarks -c Release -- --filter *CrossPlatformTraversalBenchmark* --job Medium --exporters json markdown"
    Write-Host "  2. dotnet run --project src/Jeninnet.FileQuery.Soak -c Release -- --duration-seconds 60 --warmup-seconds 5 --directories 100 --files-per-directory 50 --sample-interval 50"
    Write-Host "  3. Update docs/performance/cross-platform-baseline.json and runtime-evolution.json (history entry)."
    Write-Host "  4. Update the narrative in docs/performance/cross-platform-baseline.md and long-running-stability.md."

    if ($Enforce) {
        exit 1
    }

    exit 0
}

Write-Host "Runtime matches the recorded baseline." -ForegroundColor Green
exit 0
