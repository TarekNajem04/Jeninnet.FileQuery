param(
    [switch]$ForceClean,
    [switch]$ForceRestore
)

Write-Host "====== [1/4] Checking environment ======" -ForegroundColor Cyan
if ($ForceClean) {
    Write-Host "Cleaning old test results..." -ForegroundColor Yellow
    if (Test-Path "TestResults") { Remove-Item -Recurse -Force "TestResults" }
    if (Test-Path "TestCoverageReport") { Remove-Item -Recurse -Force "TestCoverageReport" }
}

Write-Host "====== [2/4] Preparing environment ======" -ForegroundColor Cyan
if ($ForceRestore) {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet clean
    dotnet restore
}

$reportGeneratorInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
if (-not $reportGeneratorInstalled) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool update --global dotnet-reportgenerator-globaltool
} else {
    Write-Host "ReportGenerator is already installed. Skipping update." -ForegroundColor Green
}

Write-Host "====== [3/4] Running dotnet test and collecting coverage ======" -ForegroundColor Cyan
dotnet test --collect:"XPlat Code Coverage"

# Dynamically find any coverage XML file (cobertura or opencover)
$coverageFiles = Get-ChildItem -Path "TestResults/**/*xml" -Recurse -ErrorAction SilentlyContinue

if ($null -eq $coverageFiles -or $coverageFiles.Count -eq 0) {
    Write-Host "`n[ERROR] Failed to generate coverage source files! Process aborted." -ForegroundColor Red
    exit 1
}

Write-Host "====== [4/4] Generating visual coverage report with exclusions ======" -ForegroundColor Cyan
# Using the dynamic pattern to catch any generated XML format
reportgenerator `
  -reports:"TestResults/**/*xml" `
  -targetdir:"TestCoverageReport" `
  -reporttypes:"Html;HtmlSummary;TextSummary" `
  -assemblyfilters:"+Jeninnet.FileQuery*;+Jeninnet.FileQuery.CommandLine*;+Jeninnet.FileQuery.DependencyInjection*;+Jeninnet.Testing.Assertions*"

# Verify that the final HTML report was actually generated
if (Test-Path "TestCoverageReport/index.html") {
    Write-Host "====== Code coverage report generated successfully ======" -ForegroundColor Green
    Write-Host "====== Opening report in your browser... ======" -ForegroundColor Yellow
    Start-Process "TestCoverageReport/index.html"
} else {
    Write-Host "`n[ERROR] ReportGenerator failed to create the final HTML report!" -ForegroundColor Red
}
