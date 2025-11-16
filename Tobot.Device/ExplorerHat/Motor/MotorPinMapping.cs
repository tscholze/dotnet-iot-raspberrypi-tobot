namespace Tobot.Device.ExplorerHat.Motor
{
    /// <summary>
    /// Represents motor pin mapping configuration.
    /// </summary>
    /// <param name="Enable">PWM enable pin.</param>
    /// <param name="Forward">Forward direction pin.</param>
    /// <param name="Backward">Backward direction pin.</param>
    internal record MotorPinMapping(int Enable, int Forward, int Backward);
}
