---
applyTo: "firmware/**"
---
- Firmware is MicroPython for a Raspberry Pi Pico W; it is not compiled by the .NET solution.
- Talk to the host only through documented routes (`TobotRoutes` in `src/Tobot.Shared`), e.g. `GET /remote?action=`.
- Never commit real credentials: `secret.py` is untracked; edit `secret.example.py` for the template.
- Keep docstrings and comments in English.
