# Avalonia.Ribbon Build and Run Scripts

PowerShell scripts adapted for this repository (`AvaloniaRibbon.sln`).

## Prerequisites

- .NET 9 SDK or later
- PowerShell 7+ (`pwsh`)

## Usage

### Build desktop projects

```powershell
# Debug build
pwsh ./scripts/build_desktop.ps1

# Release build
pwsh ./scripts/build_desktop.ps1 -Configuration Release

# Skip restore for fast iteration
pwsh ./scripts/build_desktop.ps1 -NoRestore
```

### Run Flowery desktop demo

```powershell
# Build and run Debug
pwsh ./scripts/run-desktop.ps1

# Build and run Release
pwsh ./scripts/run-desktop.ps1 -Configuration Release
```

## build_desktop.ps1

Builds desktop-relevant projects in dependency order.

### Parameters

| Parameter | Type | Default | Description |
| --------- | ---- | ------- | ----------- |
| `-Configuration` | string | `Debug` | Build configuration (`Debug` or `Release`) |
| `-NoRestore` | switch | false | Passes `--no-restore` to `dotnet build` |

### Build order

1. `AvaloniaUI.Ribbon`
2. `AvaloniaUI.Ribbon.Desktop`
3. `AvaloniaUI.Ribbon.Demo` (`-f net9.0-windows`)
4. `AvaloniaUI.Ribbon.Demo.Desktop`
5. `AvaloniaUI.Ribbon.Demo.Flowery` (`-f net9.0-windows`)

## run-desktop.ps1

Builds and starts the Flowery desktop demo (`AvaloniaUI.Ribbon.Demo.Flowery`).

### Parameters

| Parameter | Type | Default | Description |
| --------- | ---- | ------- | ----------- |
| `-Configuration` | string | `Debug` | Build configuration (`Debug` or `Release`) |

### Behavior

- Resolves target framework from `AvaloniaUI.Ribbon.Demo.Flowery.csproj`
- Builds with `dotnet build -f <TargetFramework>`
- Starts `<ProjectDir>\bin\<Configuration>\<TargetFramework>\AvaloniaUI.Ribbon.Demo.Flowery.exe`
