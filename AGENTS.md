# AGENTS.md

Guidance for AI coding agents working on Tobot, a personal .NET 10 / C# robotics platform for Raspberry Pi (HC-SR04 and MotorHat driver). See `README.md` for the overview and `CONTRIBUTING.md` for project intent (a hobby playground, not a product).

## Solution layout (`Tobot.slnx`)

| Project | Purpose |
|---|---|
| `Tobot.Device` | Hardware library. `HcSr04/` and `MotorHat/`. `TobotController` manages a shared `GpioController`, HC-SR04 distance sensor, and Rx distance observable. |
| `Tobot.Pi` | Pi system info (`PiSystemInfo`, `PiStatusSnapshot`). |
| `Tobot.Web` | Blazor Server (interactive) + SignalR hub `/tobothub`. Pages in `Components/Pages`, hub in `Hubs/`, background broadcasters in `Services/`. `TobotController` is a DI singleton. |
| `Tobot` | Interactive CLI (`dotnet run --project Tobot check`). |
| `Tobot.PicoRemote` | MicroPython remote for a Pico (not in the solution; `secret.py` holds Wi-Fi config, never commit real secrets). |
| `scripts/` | Pi autostart and kiosk shell scripts. |

Dependencies flow: `Tobot` / `Tobot.Web` -> `Tobot.Device`, `Tobot.Pi`. Keep hardware code out of the web project; add it to `Tobot.Device` and expose via `TobotController` or the hub.

## Build and verify

- SDK pinned in `global.json` (10.0.x). Target `net10.0`, `Nullable` enabled.
- `dotnet restore Tobot.slnx && dotnet build Tobot.slnx -c Release` (this is exactly what CI runs).
- There are no unit tests. GPIO/I2C code only works on a Pi, so on a dev machine, verify by building only; do not try to run hardware paths.

## Coding conventions (follow the existing code)

- XML doc comments (`///`) on every type and member, including private fields.
- Use `#region` blocks to group members (e.g. `Private Fields`, `Motor Control`).
- File-scoped namespaces; one type per file; folder = functional package.
- Collections expose indexers (`explorerHat.Motor[1]`); motor speed range is -100..+100.
- Hardware is wrapped by `IDisposable` classes; dispose and stop motors on shutdown (see `ApplicationStopping` hook in `Tobot.Web/Program.cs`).
- SignalR event names live as constants in `TobotHubEvents`; reuse them rather than string literals. Hub methods should broadcast state changes via `Clients.All`.
- Distance reads use `TryReadDistance` (returns false on failure) and Rx `ObserveDistance`.
- Third-party-derived code must keep its notice (`MotorHat/THIRD-PARTY-NOTICES.md`).

## Gotchas

- Pan-Tilt HAT support was intentionally removed; do not reintroduce it. The v4 direction favors standard servos and motor controllers over specialty HATs.
- `tobot-web.log` is a runtime artifact; do not edit it.
- Update the relevant README / package docs when changing public APIs.
