namespace Tobot.Web.Hubs;

/// <summary>
/// Constants for SignalR events sent from the TobotHub to clients.
/// </summary>
public static class TobotHubEvents
{
    /// <summary>
    /// Event sent when a motor's speed changes.
    /// Parameters: motorNumber (int), speed (double)
    /// </summary>
    public const string MotorSpeedChanged = nameof(MotorSpeedChanged);

    /// <summary>
    /// Event sent when all motors are stopped.
    /// Parameters: none
    /// </summary>
    public const string AllMotorsStopped = nameof(AllMotorsStopped);

    /// <summary>
    /// Event sent when an LED's state changes.
    /// Parameters: ledNumber (int), isOn (bool)
    /// </summary>
    public const string LedStateChanged = nameof(LedStateChanged);

    /// <summary>
    /// Event sent when an LED is toggled.
    /// Parameters: ledNumber (int)
    /// </summary>
    public const string LedToggled = nameof(LedToggled);

    /// <summary>
    /// Event sent when a digital output's state changes.
    /// Parameters: outputNumber (int), isOn (bool)
    /// </summary>
    public const string OutputStateChanged = nameof(OutputStateChanged);

    /// <summary>
    /// Event sent when a digital output is toggled.
    /// Parameters: outputNumber (int)
    /// </summary>
    public const string OutputToggled = nameof(OutputToggled);

    /// <summary>
    /// Event sent when the HC-SR04 distance changes by threshold.
    /// Parameters: distanceCm (double)
    /// </summary>
    public const string DistanceChanged = nameof(DistanceChanged);

    /// <summary>
    /// Event sent when a light show is started.
    /// Parameters: cycleCount (int)
    /// </summary>
    public const string LightShowStarted = nameof(LightShowStarted);

    /// <summary>
    /// Event sent when pan/tilt is changed.
    /// Parameters: pan (double), tilt (double)
    /// </summary>
    public const string PanTiltChanged = nameof(PanTiltChanged);

    /// <summary>
    /// Event sent when random drive starts.
    /// Parameters: forwardSpeed (int), turnSpeed (int), obstacleDistanceCm (double), clearDistanceCm (double)
    /// </summary>
    public const string RandomDriveStarted = nameof(RandomDriveStarted);

    /// <summary>
    /// Event sent when random drive stops.
    /// Parameters: none
    /// </summary>
    public const string RandomDriveStopped = nameof(RandomDriveStopped);

    /// <summary>
    /// Event sent after a detection sweep completes.
    /// Parameters: distanceCm (double), panAngle (int), direction (string)
    /// </summary>
    public const string ObjectDetectionCompleted = nameof(ObjectDetectionCompleted);

    /// <summary>
    /// Event sent after a direction classification measurement.
    /// Parameters: panAngle (int), distanceCm (double), direction (string)
    /// </summary>
    public const string DirectionClassified = nameof(DirectionClassified);

    /// <summary>
    /// Event sent periodically with Pi system status.
    /// Parameters: wifiSsid (string?), wifiIp (string?), cpuLoadPercent (double), memLoadPercent (double)
    /// </summary>
    public const string PiStatusUpdated = nameof(PiStatusUpdated);
}
