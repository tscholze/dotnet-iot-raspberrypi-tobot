namespace Tobot.Shared.Models;

/// <summary>
/// A single distance measurement of the HC-SR04 sensor.
/// </summary>
/// <param name="DistanceCm">Measured distance in centimeters.</param>
public sealed record DistanceReading(double DistanceCm);
