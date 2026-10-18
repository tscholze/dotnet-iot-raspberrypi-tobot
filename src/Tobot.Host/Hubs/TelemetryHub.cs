using Microsoft.AspNetCore.SignalR;

namespace Tobot.Host.Hubs;

/// <summary>
/// SignalR hub for server-to-client telemetry (see <see cref="Shared.TobotHubEvents"/>).
/// </summary>
/// <remarks>
/// The hub deliberately exposes no methods: commands and queries go through the REST API,
/// the hub is only used by background services to push live data to all connected clients.
/// </remarks>
public sealed class TelemetryHub : Hub;
