# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projects

| Project | Framework | Status |
|---|---|---|
| `SmartSDR Settings File Manager WPF\` | .NET 8 WPF | **Active** |
| `SmartSDR Settings File Manager\` | .NET Framework 4.8 WinForms | Legacy / reference only |

## Build & Run

```bash
dotnet restore
dotnet build -c Release
dotnet run --project "SmartSDR Settings File Manager WPF"
```

Requires .NET 8 SDK and Windows. IDE: Visual Studio 2026. No test projects and no CI/CD pipeline.

## Architecture

WPF application targeting `net8.0-windows`, structured as strict MVVM using `CommunityToolkit.Mvvm`.

```
SmartSDR Settings File Manager WPF/
├── App.xaml / App.xaml.cs           — entry point, shared Button/TextBlock styles
├── Assets/
│   └── backup_and_restore_256x256.ico
├── ViewModels/
│   └── MainViewModel.cs             — all logic, 12 relay commands
└── Views/
    └── MainWindow.xaml(.cs)         — 2×2 GroupBox layout, menu, status bar
```

### What it does

Manages backup/restore/reset of settings files for three FlexRadio apps: **SmartSDR**, **CAT**, and **DAX**. The UI has one GroupBox per application (Save / Restore / Reset), plus a shared Optional Features box (Clean Up, backup file prefix).

### NuGet packages

| Package | Purpose |
|---|---|
| `CommunityToolkit.Mvvm` 8.2.2 | `[ObservableProperty]` / `[RelayCommand]` source generation |
| `Ookii.Dialogs.Wpf` 5.0.1 | Folder picker (`VistaFolderBrowserDialog`) for Export and Import |

### File paths (hardcoded in MainViewModel)

| Role | Path |
|---|---|
| Settings source | `%APPDATA%\FlexRadio Systems\` |
| User backup folder | `%DOCUMENTS%\SmartSDR Settings Backup\` |

Managed files: `SSDR.settings`, `CAT.settings`, `DAX.settings` (plus their `.backup` siblings written by SmartSDR itself).

### ViewModel design

All file operations and dialog interactions live in `MainViewModel`. The View has no code-behind beyond `InitializeComponent()`.

- `[ObservableProperty] StatusMessage` / `StatusColor` — drive the status bar; `SetStatus(message, level)` sets both (0 = black, 1 = blue, 2 = red)
- `partial void OnBackupFilePrefixChanged` — validates the prefix against `^[0-9A-Z\-_]*$` and updates status inline
- `ExecuteSave / ExecuteRestore / ExecuteReset` — process-guard dispatchers that call the core `SaveFile / RestoreFile / ResetFile` methods
- `IsAppRunning()` — wraps `Process.GetProcessesByName()` to block operations while the target app is running
- `FriendlyAppName()` — maps `"SSDR"` → `"SmartSDR for Windows"` for dialog text

### File dialog types

- **Open file** (Restore): `Microsoft.Win32.OpenFileDialog`, filtered to `*{cfgFile}`, opens in backup folder
- **Folder** (Export / Import): `Ookii.Dialogs.Wpf.VistaFolderBrowserDialog`

### Status feedback

`StatusMessage` (string) and `StatusColor` (Brush) are bound directly in XAML. All catch blocks call `SetStatus($" ERROR: {e.Message}", 2)`. Never set these fields directly — always go through `SetStatus()`.
