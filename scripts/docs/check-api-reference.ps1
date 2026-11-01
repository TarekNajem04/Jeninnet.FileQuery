#requires -Version 7
<#
.SYNOPSIS
  Verifies that the generated DocFX API metadata covers every documented project.

.DESCRIPTION
  Reads the project/assembly list from docs/docfx.json and checks the DocFX API manifest
  under docs/api. The manifest is produced by:
      dotnet docfx docs/docfx.json
  It is git-ignored, so this check runs after metadata generation in CI.

  A documented project counts as covered when the manifest contains a UID equal to the
  project name (its root namespace) or under that namespace. UIDs that belong to a longer,
  more specific documented project name are not credited to the shorter one.

.PARAMETER ApiDirectory
  The generated DocFX API directory. Defaults to <repo>/docs/api.

.PARAMETER DocfxConfig
  The DocFX configuration that lists the documented projects. Defaults to <repo>/docs/docfx.json.

.OUTPUTS
  Prints the covered project count and exits 1 when a project is missing or the metadata is absent.

.EXAMPLE
  pwsh scripts/docs/check-api-reference.ps1
#>
param(
    [string]$ApiDirectory = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path 'docs\api'),
    [string]$DocfxConfig = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path 'docs\docfx.json')
)

$ErrorActionPreference = 'Stop'

$manifestPath = Join-Path $ApiDirectory '.manifest'
$tocPath = Join-Path $ApiDirectory 'toc.yml'

if (-not (Test-Path -LiteralPath $DocfxConfig)) {
    Write-Error "DocFX configuration not found at '$DocfxConfig'."
    exit 2
}

if (-not (Test-Path -LiteralPath $manifestPath)) {
    Write-Error "DocFX API metadata not found at '$ApiDirectory'. Run 'dotnet docfx docs/docfx.json' first."
    exit 2
}

if (-not (Test-Path -LiteralPath $tocPath)) {
    Write-Error "DocFX API table of contents not found at '$tocPath'."
    exit 2
}

# docs/docfx.json lists C# projects; a compiled assembly path is accepted as well.
$configuration = Get-Content -LiteralPath $DocfxConfig -Raw | ConvertFrom-Json
$projects = [System.Collections.Generic.List[string]]::new()
foreach ($metadata in @($configuration.metadata)) {
    foreach ($source in @($metadata.src)) {
        foreach ($file in @($source.files)) {
            $name = $null
            if ($file -match '(?<name>[^/\\]+)\.csproj$') {
                $name = $Matches['name']
            }
            elseif ($file -match '(?<name>[^/\\]+)\.dll$') {
                $name = $Matches['name']
            }

            if ($null -ne $name -and -not $projects.Contains($name)) {
                [void]$projects.Add($name)
            }
        }
    }
}

if ($projects.Count -eq 0) {
    Write-Error "No documented projects or assemblies are listed in '$DocfxConfig'."
    exit 2
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$uids = @($manifest.PSObject.Properties.Name)

# Longer names first so a nested package (Jeninnet.FileQuery.CommandLine) owns its own UIDs
# instead of lending them to the core package (Jeninnet.FileQuery).
$bySpecificity = @($projects | Sort-Object -Property Length -Descending)

$missing = [System.Collections.Generic.List[string]]::new()
foreach ($project in $bySpecificity) {
    $prefix = $project + '.'

    $found = $false
    foreach ($uid in $uids) {
        if ($uid -ne $project -and -not $uid.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            continue
        }

        $belongsToNested = $false
        foreach ($other in $bySpecificity) {
            if ($other.Length -le $project.Length) { continue }
            if ($uid -eq $other -or $uid.StartsWith($other + '.', [System.StringComparison]::Ordinal)) {
                $belongsToNested = $true
                break
            }
        }

        if (-not $belongsToNested) {
            $found = $true
            break
        }
    }

    if (-not $found) {
        $missing.Add($project)
    }
}

if ($missing.Count -gt 0) {
    Write-Host "API REFERENCE CHECK FAILED ($($missing.Count) missing project(s)):" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

Write-Host "API reference OK: $($projects.Count) documented project(s) are represented in the generated metadata." -ForegroundColor Green
