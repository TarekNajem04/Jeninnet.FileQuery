<#
.SYNOPSIS
    One-command generation of the README reliability badges.

.DESCRIPTION
    Every badge is derived from tests, build output, or the documented package set, never
    hand-written. This script:
      1. Builds the solution (Release).
      2. Runs the architecture test gate.
      3. Reads the target framework from Directory.Build.props and the documented package
         set from docs/docfx.json.
      4. Reads the last coverage report (artifacts/coverage/Summary.txt) when available.
      5. Renders four SVG badges under badges/ referenced by README.md.

    Use -SkipTests to re-render badges from the last build/coverage without re-running the gate.
#>

[CmdletBinding()]
param(
    [switch]
    $SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$TestGateFilters = @(
    "FullyQualifiedName~Architecture"
)

function Find-RepositoryRoot {
    param(
        [Parameter(Mandatory)]
        [string]$StartPath
    )

    $current = (Get-Item -LiteralPath $StartPath).FullName

    while ($true) {
        $solutionFile = Get-ChildItem -Path $current -File -Filter "*.sln*" -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($solutionFile) {
            return $current
        }

        $parent = [System.IO.Directory]::GetParent($current)
        if ($null -eq $parent) {
            throw "Unable to locate repository root. No solution file was found."
        }

        $current = $parent.FullName
    }
}

function Invoke-TestGate {
    param(
        [Parameter(Mandatory)]
        [string]$Filter
    )

    Write-Host ("  gate: {0}" -f $Filter) -ForegroundColor Yellow
    & dotnet test $TestProject "-c" "Release" "--nologo" "-v" "q" "--filter" $Filter | Out-Null
    return $LASTEXITCODE -eq 0
}

function New-Badge {
    param(
        [Parameter(Mandatory)]
        [string]$Label,
        [Parameter(Mandatory)]
        [string]$Value,
        [Parameter(Mandatory)]
        [string]$Color
    )

    $labelWidth = [math]::Max(24, $Label.Length * 6.5 + 10)
    $valueWidth = [math]::Max(24, $Value.Length * 6.5 + 10)
    $totalWidth = [int]$labelWidth + [int]$valueWidth
    $labelX = [int]($labelWidth / 2)
    $valueX = [int]$labelWidth + [int]($valueWidth / 2)

    return @"
<svg xmlns="http://www.w3.org/2000/svg" width="$totalWidth" height="20" role="img" aria-label="${Label}: ${Value}">
  <linearGradient id="s" x2="0" y2="100%"><stop offset="0" stop-color="#bbb" stop-opacity=".1"/><stop offset="1" stop-opacity=".1"/></linearGradient>
  <clipPath id="r"><rect width="$totalWidth" height="20" rx="3" fill="#fff"/></clipPath>
  <g clip-path="url(#r)">
    <rect width="$([int]$labelWidth)" height="20" fill="#555"/>
    <rect x="$([int]$labelWidth)" width="$([int]$valueWidth)" height="20" fill="$Color"/>
    <rect width="$totalWidth" height="20" fill="url(#s)"/>
  </g>
  <g fill="#fff" text-anchor="middle" font-family="Verdana,Geneva,DejaVu Sans,sans-serif" font-size="11">
    <text x="$labelX" y="14">$Label</text>
    <text x="$valueX" y="14">$Value</text>
  </g>
</svg>
"@
}

$root = Find-RepositoryRoot -StartPath $PSScriptRoot
$propsFile = Join-Path $root "Directory.Build.props"
$docfxConfig = Join-Path $root "docs\docfx.json"
$TestProject = Join-Path $root "tests\Jeninnet.FileQuery.Tests\Jeninnet.FileQuery.Tests.csproj"
$badgeDirectory = Join-Path $root "badges"

New-Item -ItemType Directory -Force -Path $badgeDirectory | Out-Null

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Jeninnet FileQuery Reliability Badges" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

if (-not $SkipTests) {
    Write-Host "Step 1: build (Release)" -ForegroundColor Cyan
    & dotnet build (Join-Path $root "Jeninnet.FileQuery.slnx") "-c" "Release" "--nologo" "-v" "q" | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Solution build failed."
    }

    Write-Host "Step 2: test gates" -ForegroundColor Cyan
    $gatesPassed = $true
    foreach ($filter in $TestGateFilters) {
        if (-not (Invoke-TestGate -Filter $filter)) {
            $gatesPassed = $false
        }
    }

    if (-not $gatesPassed) {
        throw "One or more badge-gate tests failed; badges were not re-rendered as green."
    }
}

Write-Host "Step 3: render badges" -ForegroundColor Cyan

$targetFramework = [regex]::Match((Get-Content -LiteralPath $propsFile -Raw), '<TargetFramework>([^<]*)</TargetFramework>').Groups[1].Value

if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    throw "No TargetFramework was found in Directory.Build.props."
}

$documentedPackages = 0
$configuration = Get-Content -LiteralPath $docfxConfig -Raw | ConvertFrom-Json
foreach ($metadata in @($configuration.metadata)) {
    foreach ($source in @($metadata.src)) {
        foreach ($file in @($source.files)) {
            if ($file -match '\.(csproj|dll)$') {
                $documentedPackages++
            }
        }
    }
}

if ($documentedPackages -eq 0) {
    throw "No documented projects were found in docs/docfx.json."
}

$coverageValue = "not run"
$coverageColor = "#9f9f9f"
$coverageSummary = Join-Path $root "artifacts\coverage\Summary.txt"

if (Test-Path -LiteralPath $coverageSummary) {
    $summaryText = Get-Content -LiteralPath $coverageSummary -Raw
    $coverageMatch = [regex]::Match($summaryText, 'Line coverage:\s*([0-9]+(?:\.[0-9]+)?%)')

    if ($coverageMatch.Success) {
        $coverageValue = $coverageMatch.Groups[1].Value
        $coverageNumber = [double]::Parse(
            $coverageValue.TrimEnd('%'),
            [System.Globalization.CultureInfo]::InvariantCulture)

        if ($coverageNumber -ge 85) {
            $coverageColor = "#4c1"
        }
        elseif ($coverageNumber -ge 70) {
            $coverageColor = "#dfb317"
        }
        else {
            $coverageColor = "#e05d44"
        }
    }
}

$badges = @{
    "architecture" = New-Badge -Label "architecture"       -Value "invariants ok" -Color "#4c1"
    "frameworks"   = New-Badge -Label "target framework"   -Value $targetFramework -Color "#1f6feb"
    "packages"     = New-Badge -Label "API-documented projects" -Value "$documentedPackages" -Color "#1f6feb"
    "coverage"     = New-Badge -Label "line coverage"      -Value $coverageValue -Color $coverageColor
}

foreach ($entry in $badges.GetEnumerator()) {
    # Here-strings inherit this script's CRLF (Carriage Return and Line Feed); badges are LF files (see .gitattributes).
    $svg = $entry.Value -replace "`r`n", "`n"
    [System.IO.File]::WriteAllText((Join-Path $badgeDirectory ($entry.Key + ".svg")), $svg, [System.Text.UTF8Encoding]::new($false))
}

Write-Host ""
Write-Host "============================================="
Write-Host "Summary"
Write-Host "---------------------------------------------"
Write-Host ("architecture : invariants ok (gate: ArchitectureTests)")
Write-Host ("frameworks   : {0}" -f $targetFramework)
Write-Host ("packages     : {0} API-documented project(s) in docs/docfx.json" -f $documentedPackages)
Write-Host ("coverage     : {0} (artifacts/coverage/Summary.txt)" -f $coverageValue)
Write-Host "---------------------------------------------"
Write-Host "Badges written to badges/ and referenced by README.md."
Write-Host "============================================="
