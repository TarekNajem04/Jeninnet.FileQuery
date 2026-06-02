<#
    Backup Script v6.0 (Headless)
    - No GUI (Auto-execution)
    - Dynamic backup path: ..\DevOps-Git-Stages\{SolutionName}\Backups\{LONGDATE}
    - Backs up Modified, Untracked, and Deleted files
    - Generates a batch script to delete the deleted files from the project folder
    - FIXED: Slash normalization and relative path construction
#>

# ============================
#        CONFIGURATION
# ============================

$Config = @{
    BackupRoot          = "..\DevOps-Git-Stages"
    LogEnabled          = $true
    LogFileName         = "backup_log.txt"
}

# ============================
#        INITIAL SETUP
# ============================

# Anchor everything to the script's directory
$ProjectFolder = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
$ProjectFolderName = Split-Path $ProjectFolder -Leaf

# Detect solution file (.sln or .slnx)
$solutionFile = Get-ChildItem -Path $ProjectFolder -Include *.sln, *.slnx -File -ErrorAction SilentlyContinue | Select-Object -First 1

$RawName = if ($solutionFile) { [System.IO.Path]::GetFileNameWithoutExtension($solutionFile.Name) } else { $ProjectFolderName }

# Clean invalid characters from name
$CleanName = ($RawName -replace '[\\\/\:\*\?\"<>\|]', '').Replace('[', '').Replace(']', '').Trim()
$SolutionName = $CleanName

# Generate timestamp
$LongDate = (Get-Date).ToString("yyyy-MM-dd_HH-mm-ss")

# Build absolute backup path
$AbsBackupRoot = [System.IO.Path]::GetFullPath((Join-Path $ProjectFolder $Config.BackupRoot))
$BackupFolder = Join-Path $AbsBackupRoot $SolutionName
$BackupFolder = Join-Path $BackupFolder "Backups"
$BackupFolder = Join-Path $BackupFolder $LongDate

# Ensure backup folder exists
if (!(Test-Path $BackupFolder)) {
    New-Item -ItemType Directory -Path $BackupFolder -Force -ErrorAction Stop | Out-Null
}

# Logging function
function Write-Log {
    param([string]$Message)
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    $line = "[$timestamp] $Message"
    Write-Host $line

    if ($Config.LogEnabled) {
        $logFile = Join-Path $BackupFolder $Config.LogFileName
        if (!(Test-Path $logFile)) { New-Item -ItemType File -Path $logFile -Force | Out-Null }
        Add-Content -Path $logFile -Value $line
    }
}

Write-Log "=== Headless Backup Started ==="
Write-Log "Project Folder: $ProjectFolder"
Write-Log "Backup folder: $BackupFolder"

# ============================
#        GIT DETECTION
# ============================

# Get changed (modified + others) and deleted files
[array]$changedFiles = git ls-files --modified --others --exclude-standard
[array]$deletedFiles = git ls-files --deleted

Write-Log "Detected changed files: $($changedFiles.Count)"
Write-Log "Detected deleted files: $($deletedFiles.Count)"

# ============================
#        BACKUP PROCESS
# ============================

function Copy-To-Backup {
    param([string]$file, [bool]$isDeleted)

    $relativeFile = $file.Replace('/', '\')
    $destination = Join-Path $BackupFolder $relativeFile
    $destDir = Split-Path $destination -Parent

    if (!(Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }

    if ($isDeleted) {
        # File is deleted from disk, try to get it from git HEAD
        try {
            git show "HEAD:$file" | Out-File -FilePath $destination -Encoding utf8 -Force -ErrorAction Stop
            Write-Log "Backed up (from Git HEAD): $relativeFile"
            return $true
        } catch {
            Write-Log "FAILED to retrieve deleted file $relativeFile : $($_.Exception.Message)"
            return $false
        }
    } else {
        # File exists on disk
        $source = Join-Path $ProjectFolder $relativeFile
        if (Test-Path $source) {
            try {
                Copy-Item $source -Destination $destination -Force -ErrorAction Stop
                Write-Log "Backed up: $relativeFile"
                return $true
            } catch {
                Write-Log "FAILED to copy $relativeFile : $($_.Exception.Message)"
                return $false
            }
        } else {
            Write-Log "WARNING: Source not found: $source"
            return $false
        }
    }
}

$backedUpCount = 0

# Backup changed files
foreach ($file in $changedFiles) {
    if (Copy-To-Backup $file $false) { $backedUpCount++ }
}

# Backup deleted files (retrieving their last state)
foreach ($file in $deletedFiles) {
    if (Copy-To-Backup $file $true) { $backedUpCount++ }
}

# ============================
#        DELETE SCRIPT
# ============================

if ($deletedFiles.Count -gt 0) {
    $deleteScript = Join-Path $BackupFolder "delete_deleted_files.bat"
    
    # Generate content with requested format
    $content = @()
    $content += "@echo off"
    $content += "echo === DELETION SCRIPT STARTED ==="
    $content += "echo Source Folder: $ProjectFolder"
    $content += "pause"
    $content += ""

    foreach ($file in $deletedFiles) {
        $relativeFile = $file.Replace('/', '\')
        $content += "echo Deleting: $relativeFile"
        $content += "del /f /q `".\$relativeFile`""
        $content += ""
    }

    $content += "echo === DELETION SCRIPT FINISHED ==="
    $content += "pause"

    $content | Set-Content -Path $deleteScript -Encoding ASCII
    Write-Log "Delete script generated: $deleteScript"
}

Write-Log "=== Backup Finished. $backedUpCount files processed. ==="
