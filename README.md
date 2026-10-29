# TaskbarZ

**TaskbarZ is a community fork of [TaskbarX](https://github.com/ChrisAnd1998/TaskbarX)** for the classic Windows taskbar. It centers taskbar buttons, animates changes when apps open or close, and keeps its settings in the notification area.

This fork focuses on quicker closing animations, less work while the desktop is idle or a window is being dragged, and a single app for the taskbar engine and settings. It is based on TaskbarX 1.7.8.0 and retains the original MIT license and attribution.

## Download

Download **TaskbarZ-v0.1.0-win-x64.zip** from [Releases](https://github.com/sub9k/TaskbarZ/releases). Extract the entire ZIP to a permanent folder, then run `TaskbarZ.exe`. Keep the DLL files beside the executable. Requires Windows 10 with the classic taskbar and .NET Framework 4.8. This release was tested on Windows 10 build 19045.

TaskbarZ is portable. Left-click its tray icon, or choose **Settings** from the tray menu. Closing or minimizing Settings returns it to the tray. Use **Exit** to stop the app and restore the taskbar position.

If you already have TaskbarX set to launch at sign-in, remove its old scheduled task before enabling TaskbarZ startup in Settings. Running both apps at once makes them compete over icon positions.

## What changed from TaskbarX

| Area | Change |
| --- | --- |
| App structure | The taskbar engine and Configurator run in one process. Settings loads only when opened from the tray. |
| Closing animations | Centering begins when Explorer starts shrinking a closing button. The animation follows the changing button layout without restarting its easing curve on every update. |
| Opening animations | Removed the wait for the taskbar to settle, so position changes are handled as soon as they are observed. Animation frames are paced at about 16 ms. |
| Responsiveness | Taskbar layout is checked every 25 ms on external power and 50 ms on battery. |
| Dragging and redraw | Removed repeated redraw suppression and the tight position loop from the tray fix. Taskbar styling is applied at most once per second, and TaskbarZ leaves styling to TranslucentTB while it runs. |
| Error handling | A failed taskbar accessibility read gets a short retry delay. Invalid icon bounds are ignored instead of being treated as a zero-width taskbar. |

The first part of a closing animation still depends on when Windows Explorer changes its own taskbar button layout. TaskbarZ cannot begin moving before Explorer reports that change.

## Styling and compatibility

TaskbarZ retains TaskbarX's transparent, blur, acrylic, and color options. Windows 10 does not expose an adjustable blur radius for these taskbar effects. Acrylic behavior around the Start menu has **not** been changed in this fork. If you use TranslucentTB for taskbar appearance, choose **Default** styling in TaskbarZ so only one app controls the background.

The centering engine targets the classic Windows 10 taskbar. It is not a replacement for the native Windows 11 taskbar.

## Build from source

On Windows, install Visual Studio 2022 with MSBuild and the .NET desktop development tools. From PowerShell at the repository root:

```powershell
.\build.ps1
```

The Release build is written to `APPX/TaskbarZ64/Release/`. To build and package the portable ZIP:

```powershell
.\package.ps1
```

The ZIP is written to `dist/`. Both `APPX/` and `dist/` are excluded from source control, so GitHub contains source and scripts while releases contain the runnable package.

### Source layout

- `TaskbarZ.sln` — both source projects in one Visual Studio solution.
- `TaskbarX/TaskbarX/` — centering, animations, taskbar styling, and Windows integration. The upstream folder and internal namespace names remain for source compatibility; the built assembly is `TaskbarZ.Core.dll`.
- `TaskbarX Configurator/TaskbarX Configurator/` — WPF settings and the single `TaskbarZ.exe` entry point.
- `build.ps1` — restore dependencies and build both projects.
- `package.ps1` — create the portable release ZIP.

## Credits and license

TaskbarZ is derived from [TaskbarX by Chris Andriessen](https://github.com/ChrisAnd1998/TaskbarX). The original project, source assets, and this fork are distributed under the [MIT License](LICENSE). TaskbarZ is an independent fork; for issues with this version, use the [TaskbarZ issue tracker](https://github.com/sub9k/TaskbarZ/issues).
