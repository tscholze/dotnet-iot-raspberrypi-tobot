namespace Tobot.Device.MotorHat.Motor;

/// <summary>Specifies how a DC motor driver's recirculating current is handled.</summary>
public enum DecayMode
{
    /// <summary>Fast decay; the motor coasts when throttle is removed.</summary>
    Fast = 0,

    /// <summary>Slow decay; the motor brakes when throttle is removed.</summary>
    Slow = 1
}
