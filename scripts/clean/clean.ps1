<#
.SYNOPSIS
    Cleans generated files and directories from the repository.

.DESCRIPTION
    Removes generated directories based on patterns defined in clean.config.json.

    The script runs in preview mode by default.
    No files are deleted unless -Execute is specified.

.PARAMETER Execute
    Executes the cleanup operation.

.PARAMETER SkipConfirmation
    Skips confirmation prompts.
    Useful for CI/CD environments.

.PARAMETER Configuration
    Specifies the cleanup configuration file.

.PARAMETER Include
    Adds temporary directory patterns.

.PARAMETER Exclude
    Adds temporary excluded directory names.

.EXAMPLE
    .\clean.ps1

    Preview cleanup.

.EXAMPLE
    .\clean.ps1 -Execute

    Execute cleanup with confirmation.

.EXAMPLE
    .\clean.ps1 -Execute -SkipConfirmation

    Execute cleanup without confirmation prompts.

.EXAMPLE
    .\clean.ps1 -Include "logs*"

    Preview additional patterns.

.NOTES
    Author:
        Tarek Najem

    Tool:
        Jeninnet Repository Cleanup Tool
#>

[CmdletBinding(
    SupportsShouldProcess = $true,
    ConfirmImpact = "High"
)]
param(
    [switch]
    $Execute,

    [switch]
    $SkipConfirmation,

    [string]
    $Configuration = (Join-Path $PSScriptRoot "clean.config.json"),

    [string[]]
    $Include = @(),

    [string[]]
    $Exclude = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:ProtectedDirectories = @(
    "src",
    "tests",
    "scripts",
    ".git",
    ".github"
)


function Write-Banner {

    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host " Jeninnet Repository Cleanup Tool" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""
}


function Find-RepositoryRoot {

    param(
        [Parameter(Mandatory)]
        [string]$StartPath
    )

    $current = (Get-Item -LiteralPath $StartPath).FullName

    while ($true) {

        Write-Verbose "Checking: $current"

        $solution = Get-ChildItem `
            -Path $current `
            -File `
            -Filter "*.sln" `
            -ErrorAction SilentlyContinue |
            Select-Object -First 1


        if (-not $solution) {

            $solution = Get-ChildItem `
                -Path $current `
                -File `
                -Filter "*.slnx" `
                -ErrorAction SilentlyContinue |
                Select-Object -First 1
        }


        if ($solution) {

            Write-Verbose "Solution found: $($solution.FullName)"

            return $current
        }


        $parent = [System.IO.Directory]::GetParent($current)

        if ($null -eq $parent) {
            break
        }

        $current = $parent.FullName
    }


    throw @"
Unable to locate repository root.

No solution file (*.sln or *.slnx) was found while searching parent directories.
"@
}


function Read-CleanupConfiguration {

    param(
        [Parameter(Mandatory)]
        [string]$Path
    )


    if (-not (Test-Path $Path)) {

        throw "Configuration file not found: $Path"
    }


    $config = Get-Content `
        -Path $Path `
        -Raw |
        ConvertFrom-Json


    if ($null -eq $config.directories) {

        throw "Configuration is missing the 'directories' section."
    }


    return $config
}


function Get-CleanupTargets {

    param(
        [Parameter(Mandatory)]
        [string]$Root,

        [Parameter(Mandatory)]
        [string[]]$Patterns,

        [Parameter(Mandatory)]
        [string[]]$Excluded
    )


    $targets = foreach ($pattern in $Patterns) {

        Write-Verbose "Searching pattern: $pattern"

        Get-ChildItem `
            -Path $Root `
            -Directory `
            -Filter $pattern `
            -Recurse `
            -Force `
            -ErrorAction SilentlyContinue
    }


    return $targets |
        Where-Object {

            $_.Name -notin $Excluded -and
            $_.Name -notin $script:ProtectedDirectories

        } |
        Sort-Object FullName -Unique
}


function Remove-CleanupTargets {

    param(
        [Parameter(Mandatory)]
        [System.IO.DirectoryInfo[]]$Targets
    )


    $deleted = 0
    $wouldRemove = 0
    $skipped = 0


    foreach ($target in $Targets) {


        if ($Execute) {

            if ($SkipConfirmation) {

                Remove-Item `
                    -LiteralPath $target.FullName `
                    -Recurse `
                    -Force

                Write-Host "Deleted:" -ForegroundColor Green -NoNewline
                Write-Host " $($target.FullName)"

                $deleted++

                continue
            }


            if ($PSCmdlet.ShouldProcess(
                    $target.FullName,
                    "Remove directory"
                )) {

                Remove-Item `
                    -LiteralPath $target.FullName `
                    -Recurse `
                    -Force

                Write-Host "Deleted:" -ForegroundColor Green -NoNewline
                Write-Host " $($target.FullName)"

                $deleted++
            }
        }
        else {

            Write-Host "Preview:" -ForegroundColor Yellow -NoNewline
            Write-Host " $($target.FullName)"

            $wouldRemove++
        }
    }


    return @{
        Deleted     = $deleted
        WouldRemove = $wouldRemove
        Skipped     = $skipped
    }
}


try {

    $timer = [System.Diagnostics.Stopwatch]::StartNew()

    Write-Banner


    $repositoryRoot = Find-RepositoryRoot `
        -StartPath $PSScriptRoot


    Write-Host "Repository:"
    Write-Host $repositoryRoot -ForegroundColor Green
    Write-Host ""


    if ($Execute) {

        Write-Host "Mode: Execute" -ForegroundColor Red

    }
    else {

        Write-Host "Mode: Preview" -ForegroundColor Cyan
        Write-Host "No changes will be made."
    }


    Write-Host ""


    $config = Read-CleanupConfiguration `
        -Path $Configuration


    $patterns = @(
        $config.directories.include
        $Include
    )


    $excluded = @(
        $config.directories.exclude
        $Exclude
    )


    $targets = Get-CleanupTargets `
        -Root $repositoryRoot `
        -Patterns $patterns `
        -Excluded $excluded


    if (-not $targets) {

        Write-Host "Nothing to clean." -ForegroundColor Green
        exit 0
    }


    Write-Host "Targets found: $($targets.Count)"
    Write-Host ""


    $result = Remove-CleanupTargets `
        -Targets $targets


    $timer.Stop()


    Write-Host ""
    Write-Host "============================================="
    Write-Host "Cleanup Summary"
    Write-Host "---------------------------------------------"
    Write-Host "Found        : $($targets.Count)"
    Write-Host "Would Remove : $($result.WouldRemove)"
    Write-Host "Deleted      : $($result.Deleted)"
    Write-Host "Skipped      : $($result.Skipped)"
    Write-Host "Duration     : $($timer.Elapsed)"
    Write-Host "============================================="

    if (-not $Execute) {

        Write-Host ""
        Write-Host "To apply cleanup, run this script again with the " -ForegroundColor Green -NoNewline
        Write-Host "-Execute" -ForegroundColor Red -NoNewline
        Write-Host " parameter:" -ForegroundColor Green
        Write-Host ""

        Write-Host "Example:" -ForegroundColor Cyan
        Write-Host "$($MyInvocation.MyCommand.Path)" -ForegroundColor Yellow  -NoNewline
        Write-Host " -Execute" -ForegroundColor Red
    }
    exit 0

}
catch {

    Write-Host ""
    Write-Host "Cleanup failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red

    exit 1
}
