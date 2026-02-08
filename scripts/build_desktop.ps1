<#
Builds desktop-relevant Avalonia.Ribbon projects with correct per-project settings.

Usage:
  pwsh ./scripts/build_desktop.ps1
  pwsh ./scripts/build_desktop.ps1 -Configuration Release
  pwsh ./scripts/build_desktop.ps1 -NoRestore
#>
param(
    [string]$Configuration = "Debug",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
$script:buildResults = @()
$script:startTime = Get-Date

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$Command
    )

    Write-Host $Title -ForegroundColor Cyan
    Write-Host "  $Command"
    $stepStart = Get-Date
    Invoke-Expression $Command
    $stepDuration = (Get-Date) - $stepStart

    if ($LASTEXITCODE -ne 0) {
        $script:buildResults += [PSCustomObject]@{ Project = $Title; Status = "FAILED"; Duration = $stepDuration }
        Write-Host "FAILED: $Title" -ForegroundColor Red
        exit 1
    }

    $script:buildResults += [PSCustomObject]@{ Project = $Title; Status = "OK"; Duration = $stepDuration }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

$ribbonProject = Join-Path $repoRoot "AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj"
$desktopLibraryProject = Join-Path $repoRoot "AvaloniaUI.Ribbon.Desktop/AvaloniaUI.Ribbon.Desktop.csproj"
$demoProject = Join-Path $repoRoot "AvaloniaUI.Ribbon.Demo/AvaloniaUI.Ribbon.Demo.csproj"
$demoDesktopProject = Join-Path $repoRoot "AvaloniaUI.Ribbon.Demo.Desktop/AvaloniaUI.Ribbon.Demo.Desktop.csproj"
$floweryDemoProject = Join-Path $repoRoot "AvaloniaUI.Ribbon.Demo.Flowery/AvaloniaUI.Ribbon.Demo.Flowery.csproj"

$requiredProjects = @(
    $ribbonProject,
    $desktopLibraryProject,
    $demoProject,
    $demoDesktopProject,
    $floweryDemoProject
)

foreach ($projectPath in $requiredProjects) {
    if (-not (Test-Path $projectPath)) {
        Write-Host "ERROR: Project not found at $projectPath" -ForegroundColor Red
        exit 1
    }
}

$noRestoreArg = if ($NoRestore) { " --no-restore" } else { "" }

Invoke-Step "Build: AvaloniaUI.Ribbon" "dotnet build `"$ribbonProject`" -c $Configuration$noRestoreArg"
Invoke-Step "Build: AvaloniaUI.Ribbon.Desktop" "dotnet build `"$desktopLibraryProject`" -c $Configuration$noRestoreArg"
Invoke-Step "Build: AvaloniaUI.Ribbon.Demo (net9.0-windows)" "dotnet build `"$demoProject`" -f net9.0-windows -c $Configuration$noRestoreArg"
Invoke-Step "Build: AvaloniaUI.Ribbon.Demo.Desktop" "dotnet build `"$demoDesktopProject`" -c $Configuration$noRestoreArg"
Invoke-Step "Build: AvaloniaUI.Ribbon.Demo.Flowery (net9.0-windows)" "dotnet build `"$floweryDemoProject`" -f net9.0-windows -c $Configuration$noRestoreArg"

$totalDuration = (Get-Date) - $script:startTime

Write-Host ""
Write-Host "===========================================================" -ForegroundColor Green
Write-Host " BUILD SUMMARY" -ForegroundColor Green
Write-Host "===========================================================" -ForegroundColor Green
Write-Host ""
foreach ($result in $script:buildResults) {
    $statusColor = if ($result.Status -eq "OK") { "Green" } else { "Red" }
    $duration = $result.Duration.ToString("mm\:ss\.ff")
    Write-Host ("  [{0}] {1,-56} {2}" -f $result.Status, $result.Project, $duration) -ForegroundColor $statusColor
}
Write-Host ""
Write-Host ("  Total time: {0:mm\:ss\.ff}" -f $totalDuration) -ForegroundColor Cyan
Write-Host ("  Projects built: {0}" -f $script:buildResults.Count) -ForegroundColor Cyan
Write-Host ""
Write-Host "All builds completed successfully." -ForegroundColor Green
