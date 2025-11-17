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
}
