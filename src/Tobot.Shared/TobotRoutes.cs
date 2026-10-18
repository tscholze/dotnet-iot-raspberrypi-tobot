namespace Tobot.Shared;

/// <summary>
/// Route constants of the Tobot HTTP API and SignalR hub, shared by the host and all clients.
/// </summary>
public static class TobotRoutes
{
	/// <summary>
	/// Route of the SignalR telemetry hub (server-to-client push only).
	/// </summary>
	public const string TelemetryHub = "/hubs/telemetry";

	/// <summary>
	/// Route returning a single <see cref="Models.DistanceReading"/>.
	/// Optional query parameter: <c>samples</c>.
	/// </summary>
	public const string Distance = "/api/distance";

	/// <summary>
	/// Route returning the current <see cref="Models.PiStatusSummary"/>.
	/// </summary>
	public const string Status = "/api/status";
}
