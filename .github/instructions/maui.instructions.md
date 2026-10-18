---
applyTo: "src/Tobot.Maui/**"
---
- The MAUI app is a pure client: reference `Tobot.Client` only, never `Tobot.Device`.
- Android builds on any OS; Mac Catalyst only on macOS (`-r maccatalyst-arm64`, `-p:ValidateXcodeVersion=false` if Xcode is newer than the workload pins).
- Update UI from telemetry events via `MainThread.BeginInvokeOnMainThread`.
- XML-document every type and member.
