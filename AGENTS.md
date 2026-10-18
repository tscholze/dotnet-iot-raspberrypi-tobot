# AGENTS.md

Guidance for AI coding agents working on Tobot, a personal .NET 10 / C# robotics platform for Raspberry Pi (HC-SR04 and MotorHat driver). See `README.md` for the overview and `CONTRIBUTING.md` for project intent (a hobby playground, not a product).

## Solution layout (`Tobot.slnx`)

| Project | Runs on | Purpose |
|---|---|---|
| `src/Tobot.Shared` | everywhere | Hardware-free models (`DistanceReading`, `PiStatusSummary`, `PiStatusSnapshot`), `ITobotApi`, `TobotRoutes`, `TobotHubEvents`. |
| `src/Tobot.Device` | Pi | Hardware: `HcSr04/`, `MotorHat/`, `Pi/` (`PiSystemInfo`), `TobotController` (shared `GpioController`, distance Rx observable). |
| `src/Tobot.Host` | Pi | ASP.NET Core: REST in `Endpoints/`, SignalR `TelemetryHub` (push only), broadcasters in `Services/`, Blazor pages in `Components/Pages`. `TobotController` is a DI singleton. |
| `src/Tobot.Client` | any | `TobotApiClient` (REST) and `TobotTelemetryClient` (SignalR) used by CLI and MAUI. |
| `src/Tobot.Cli` | Pi / dev | Console client (`dotnet run --project src/Tobot.Cli distance`). |
| `src/Tobot.Maui` | phone/desktop | MAUI client (Android on any OS, Mac Catalyst only on macOS); needs the MAUI workload, excluded from the default solution build. |
| `tests/Tobot.Tests` | any | xUnit v3 tests for Shared/Client (no hardware). |
| `firmware/Tobot.PicoRemote` | Pico W | MicroPython remote, listed in the solution as files only. Copy `secret.example.py` to the untracked `secret.py` for Wi-Fi config; never commit real secrets. |
| `scripts/` | Pi | Autostart and kiosk shell scripts. |

Dependencies: `Host` -> `Device`, `Shared`; `Cli`/`Maui` -> `Client` -> `Shared`. Keep hardware code in `Tobot.Device` only; clients must never reference it. Commands/queries are REST endpoints (routes in `TobotRoutes`); SignalR is only for server-to-client pushes. The API has no authentication yet (Microsoft Account SSO planned).

## Build and verify

- SDK pinned in `global.json` (10.0.x). Target `net10.0`, `Nullable` enabled.
- `dotnet restore Tobot.slnx && dotnet build Tobot.slnx -c Release` then `dotnet test --solution Tobot.slnx --no-build -c Release` (this is what CI runs, plus an Android MAUI build). Tests use Microsoft.Testing.Platform, enabled in `global.json`.
- Unit tests cover Shared and Client only. GPIO/I2C code only works on a Pi, so on a dev machine verify hardware code by building; do not try to run hardware paths.

## Coding conventions (follow the existing code)

- XML doc comments (`///`) on every type and member, including private fields.
- Use `#region` blocks to group members (e.g. `Private Fields`, `Motor Control`).
- File-scoped namespaces; one type per file; folder = functional package.
- `MotorKit` exposes `Motor1`..`Motor4` (throttle -1..1); the API/`TobotController.Drive` use -100..+100 percent (Motor1 = left, Motor2 = right).
- Host settings (camera URL, remote speed) live in `appsettings.json` under `Tobot`; do not hard-code hosts in pages.
- Hardware is wrapped by `IDisposable` classes; dispose and stop motors on shutdown (see `ApplicationStopping` hook in `src/Tobot.Host/Program.cs`).
- SignalR event names live in `TobotHubEvents` and routes in `TobotRoutes` (Shared); reuse them rather than string literals. Broadcast from services via `IHubContext<TelemetryHub>`.
- Distance reads use `TryReadDistance` (returns false on failure) and Rx `ObserveDistance`.
- Third-party-derived code must keep its notice (`MotorHat/THIRD-PARTY-NOTICES.md`).

## Gotchas

- Pan-Tilt HAT support was intentionally removed; do not reintroduce it. The v4 direction favors standard servos and motor controllers over specialty HATs.
- `tobot-web.log` is a runtime artifact; do not edit it.
- Directory.Build.props turns missing XML docs (CS1591) into errors. Update the relevant README / package docs when changing public APIs.
