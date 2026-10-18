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

	/// <summary>
	/// Route accepting a <see cref="Models.DriveCommand"/> via POST.
	/// </summary>
	public const string Drive = "/api/drive";

	/// <summary>
	/// Route that stops both motors via POST.
	/// </summary>
	public const string DriveStop = "/api/drive/stop";

	/// <summary>
	/// Simple GET route for firmware remotes, e.g. <c>/remote?action=forward</c>.
	/// Actions: ping, forward, backwards, left, right, stop (light-on/light-off are accepted and ignored).
	/// </summary>
	public const string Remote = "/remote";
}
