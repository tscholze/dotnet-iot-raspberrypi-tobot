namespace Tobot.Web.Hubs;

/// <summary>
/// Constants for SignalR events sent from the TobotHub to clients.
/// </summary>
public static class TobotHubEvents
{
    /// <summary>
    /// Event sent when the HC-SR04 distance changes by threshold.
    /// Parameters: distanceCm (double)
    /// </summary>
    public const string DistanceChanged = nameof(DistanceChanged);

    /// <summary>
    /// Event sent periodically with Pi system status.
    /// Parameters: wifiSsid (string?), wifiIp (string?), cpuLoadPercent (double), memLoadPercent (double)
    /// </summary>
    public const string PiStatusUpdated = nameof(PiStatusUpdated);
}
