# TelescopeWatcher – Copilot Instructions

## Available Skills
- **telescopeproject** (`.github/skills/telescopeproject/SKILL.md`) – Use for any general question about the DIY telescope hardware, optics, cameras, filters, motors, networking, or astrophotography topics. The skill contains full specs and instructs the agent to search the internet for supplementary information.
- **telescopeconfiguration** (`.github/skills/telescopeconfiguration/SKILL.md`) – Telescope configuration details.

## Project Overview
Windows Forms (.NET 8, `net8.0-windows`) application that is the **PC-side controller** for a Raspberry Pi–based telescope. The Pi runs a small HTTP server; this app connects to it over Wi-Fi or a direct serial COM port.

## Architecture & Key Components

| Component | File(s) | Role |
|---|---|---|
| **TelescopeController** | `TelescopeController.cs` | Single point for all motor/focus commands. Abstracts two transport modes (see below). |
| **SerialServerClient** | `SerialServerClient.cs` | HTTP transport: sends commands via `GET /motor/write?cmd=…` (fire-and-forget, 500 ms timeout). |
| **MjpegStreamClient** | `MjpegStreamClient.cs` | Streams MJPEG video from Pi camera(s); decodes JPEG frames and raises `FrameReceived` event with a `Bitmap`. |
| **TelescopeSettings** | `TelescopeSettings.cs` | Singleton (`TelescopeSettings.Instance`) that stores speed/focus settings and broadcasts changes via events across forms. |
| **StarFollower2Form** | `StarFollower2Form.cs` | Autonomous star-tracking: polls MJPEG snapshot endpoint, finds the brightest star, then calls `TelescopeController` to correct drift. |
| **PhaseCorrelation** | `PhaseCorrelation.cs` | Pure-C# 2-D FFT phase-correlation used to estimate frame-to-frame pixel drift. Returns `(dx, dy, mean)`. |
| **MainForm** | `MainForm.cs` | Entry point UI; manages Wi-Fi connection to Pi SSID, COM-port connect/disconnect, and opens child forms. |

## Two Transport Modes
`TelescopeController` supports a `isServerMode` flag:
- **Server mode** – uses `SerialServerClient` (HTTP GET to Pi's REST endpoints).
- **Serial mode** – uses `System.IO.Ports.SerialPort` directly (legacy/direct USB cable).

Always check `IsConnected()` before sending; `WriteCommand()` dispatches to the correct transport.

## Motor Command Protocol
Commands are short key-value strings sent over HTTP or serial:
- `v=0|1` – axis (0 = horizontal/RA, 1 = vertical/Dec)
- `d=0|1` – direction
- `t=<ms>` – time between steps (maps from `TelescopeSettings.TimeBetweenSteps`)
- `s=<n>` – step count (`s=0` stops)
- `b=<n>` – focus speed; `a=0|1` – focus direction; `c=<n>` – focus steps

## StarFollower Motor Direction Mapping
```
UP    → v=1, d=1
DOWN  → v=1, d=0
LEFT  → v=0, d=0
RIGHT → v=0, d=1
```
This mirrors the Python-side `StarFollower.py` on the Pi.

## Settings Singleton Pattern
Use `TelescopeSettings.Instance` everywhere – never `new TelescopeSettings()`. Subscribe to `StepsPerSecondChanged` / `FocusSpeedChanged` to keep UI controls in sync across forms. Speed is stored as **steps/second**; the controller uses `TimeBetweenSteps` (ms), which is auto-calculated. Special cases: `TimeBetweenSteps == 0` → `t=0.1`, `== -1` → `t=0.01`.

## MjpegStreamClient Notes
- Supports up to two simultaneous streams (primary + secondary camera).
- Uses a dedicated background `Task.Run` loop with a `BufferedStream` and a sync `ReadLine` (avoids async overhead at frame rate).
- `FrameReceived` delivers a `Bitmap`; consumer must dispose it after use.
- `FlipHorizontal` / `FlipVertical` properties apply `RotateFlip` in-place before raising the event.

## Key Dependencies
- `LibVLCSharp` / `VideoLAN.LibVLC.Windows` – video playback in `VideoPlayerForm`
- `Microsoft.Web.WebView2` – embedded browser panel
- `CosineKitty.AstronomyEngine` – sidereal/celestial calculations in `SiderealTrackerForm` / `CelestialCatalog`
- `System.IO.Ports` – direct serial COM port

## Build & Run
```bash
dotnet restore TelescopeWatcher.sln
dotnet build TelescopeWatcher.sln
dotnet run --project TelescopeWatcher.csproj
```
`BenchmarkSuite1` is excluded from the main project build via `<DefaultItemExcludes>` and `<Compile Remove>`.

## Camera Settings
JSON presets in `Camera_settings/` are loaded at runtime for different lighting conditions. Follow the same filename pattern (`<camera model> <condition>.json`) when adding new presets.
