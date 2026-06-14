Write-Host "====== [1/4] Cleaning old test results and reports ======" -ForegroundColor Cyan
if (Test-Path "TestResults") { Remove-Item -Recurse -Force "TestResults" }
if (Test-Path "TestCoverageReport") { Remove-Item -Recurse -Force "TestCoverageReport" }

Write-Host "====== [2/4] Restoring NuGet packages and updating tools ======" -ForegroundColor Cyan
dotnet clean
dotnet restore
dotnet tool update --global dotnet-reportgenerator-globaltool

Write-Host "====== [3/4] Running dotnet test and collecting coverage ======" -ForegroundColor Cyan
dotnet test --collect:"XPlat Code Coverage"

# Dynamically find any coverage XML file (cobertura or opencover)
$coverageFiles = Get-ChildItem -Path "TestResults/**/*xml" -Recurse -ErrorAction SilentlyContinue

if ($null -eq $coverageFiles -or $coverageFiles.Count -eq 0) {
    Write-Host "`n[ERROR] Failed to generate coverage source files! Process aborted." -ForegroundColor Red
    Exit 1
}

Write-Host "====== [4/4] Generating visual coverage report with exclusions ======" -ForegroundColor Cyan
# Using the dynamic pattern to catch any generated XML format
reportgenerator `
  -reports:"TestResults/**/*xml" `
  -targetdir:"TestCoverageReport" `
  -reporttypes:"Html;HtmlSummary;TextSummary" `
  -assemblyfilters:"-*.Tests;-*.Test;-*Migrations*"

# Verify that the final HTML report was actually generated
if (Test-Path "TestCoverageReport/index.html") {
    Write-Host "====== Code coverage report generated successfully ======" -ForegroundColor Green
    Write-Host "====== Opening report in your browser... ======" -ForegroundColor Yellow
    Start-Process "TestCoverageReport/index.html"
} else {
    Write-Host "`n[ERROR] ReportGenerator failed to create the final HTML report!" -ForegroundColor Red
}
