# Copilot instructions

Follow [AGENTS.md](../AGENTS.md) in the repository root: it describes the solution layout, build/test commands, coding conventions and gotchas for Tobot (v4 architecture: Shared / Device / Host / Client / Cli / Maui).

Key reminders:
- Document every type and member with English XML doc comments; missing docs fail the build.
- Hardware code belongs in `src/Tobot.Device` only; clients talk to `Tobot.Host` via REST/SignalR.
- Keep all "v2"/"v3" statements in READMEs; they distinguish older architectures from v4.
