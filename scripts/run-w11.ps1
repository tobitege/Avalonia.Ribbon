<#
.SYNOPSIS
    Builds and starts the Avalonia.Ribbon.w11 desktop application.

.DESCRIPTION
    Builds the w11 Desktop project and, if the resulting .exe exists, starts it in the background.

.PARAMETER Configuration
    Build configuration to use (default: Debug).

.EXAMPLE
    pwsh ./scripts/run-w11.ps1
    pwsh ./scripts/run-w11.ps1 -Configuration Release
#>
param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$desktopProject = Join-Path $repoRoot "Avalonia.Ribbon.w11/Avalonia.Ribbon.w11.csproj"

if (-not (Test-Path $desktopProject)) {
    Write-Host "ERROR: Desktop project not found at $desktopProject" -ForegroundColor Red
    exit 1
}

[xml]$desktopProjectXml = Get-Content -Raw $desktopProject
$tfm = ($desktopProjectXml.Project.PropertyGroup | Where-Object { $_.TargetFramework } | Select-Object -First 1).TargetFramework

if ([string]::IsNullOrWhiteSpace($tfm)) {
    $targetFrameworks = ($desktopProjectXml.Project.PropertyGroup | Where-Object { $_.TargetFrameworks } | Select-Object -First 1).TargetFrameworks
    if (-not [string]::IsNullOrWhiteSpace($targetFrameworks)) {
        $tfm = ($targetFrameworks -split ';' | Where-Object { $_ -like '*-windows' } | Select-Object -First 1)
        if ([string]::IsNullOrWhiteSpace($tfm)) {
            $tfm = ($targetFrameworks -split ';' | Select-Object -First 1)
        }
    }
}

if ([string]::IsNullOrWhiteSpace($tfm)) {
    Write-Host "ERROR: Could not detect TargetFramework/TargetFrameworks in $desktopProject" -ForegroundColor Red
    exit 1
}

Write-Host "Building: Avalonia.Ribbon.w11" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Gray
Write-Host "  TargetFramework: $tfm" -ForegroundColor Gray

dotnet build "$desktopProject" -c $Configuration -f $tfm

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$projectName = [System.IO.Path]::GetFileNameWithoutExtension($desktopProject)
$projectDirectory = Split-Path -Parent $desktopProject
$desktopOutput = Join-Path $projectDirectory "bin/$Configuration/$tfm"
$exePath = Join-Path $desktopOutput "$projectName.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Desktop executable not found at $exePath (skipping run)" -ForegroundColor Yellow
    exit 0
}

Write-Host "Starting Desktop app in background..." -ForegroundColor Cyan
Start-Process -FilePath $exePath -WorkingDirectory $desktopOutput | Out-Null
Write-Host "Started: $exePath" -ForegroundColor Green
