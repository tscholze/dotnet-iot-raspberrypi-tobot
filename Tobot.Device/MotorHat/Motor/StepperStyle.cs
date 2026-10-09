namespace Tobot.Device.MotorHat.Motor;

/// <summary>Coil activation style used for a stepper motor step.</summary>
public enum StepperStyle
{
    /// <summary>Activate one coil at a time.</summary>
    Single = 1,

    /// <summary>Activate two coils at a time for higher torque.</summary>
    Double = 2,

    /// <summary>Alternate single and double coil steps for half-step motion.</summary>
    Interleave = 3,

    /// <summary>Move by one configured microstep.</summary>
    Microstep = 4
}
