namespace Tobot.Device.MotorHat.Motor;

internal static class PwmDutyCycle
{
    public static double ToFraction(int value)
    {
        if (value is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        if (value == ushort.MaxValue)
        {
            return 1;
        }

        return value < 0x10 ? 0 : (value >> 4) / 4096.0;
    }
}
