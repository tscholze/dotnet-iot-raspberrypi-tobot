namespace Tobot.Device.ExplorerHat.Motor
{
    /// <summary>
    /// Represents motor pin mapping configuration for DRV8833PWP H-Bridge.
    /// </summary>
    /// <param name="Forward">Forward direction pin.</param>
    /// <param name="Backward">Backward direction pin.</param>
    internal record MotorPinMapping(int Forward, int Backward);
}
