namespace Tobot.Shared;

/// <summary>
/// Constants for SignalR events pushed from the telemetry hub to clients.
/// </summary>
public static class TobotHubEvents
{
	/// <summary>
	/// Event sent when the HC-SR04 distance changes by more than the threshold.
	/// Parameters: distanceCm (<see cref="double"/>).
	/// </summary>
	public const string DistanceChanged = nameof(DistanceChanged);

	/// <summary>
	/// Event sent periodically with a condensed Pi status.
	/// Parameters: <see cref="Models.PiStatusSummary"/>.
	/// </summary>
	public const string PiStatusUpdated = nameof(PiStatusUpdated);
}
