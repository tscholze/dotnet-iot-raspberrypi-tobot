---
applyTo: "tests/**"
---
- Tests must not touch GPIO/I2C; cover `Tobot.Shared` and `Tobot.Client` (use fake `HttpMessageHandler`s).
- xUnit v3 on Microsoft.Testing.Platform; run with `dotnet test --solution Tobot.slnx`.
