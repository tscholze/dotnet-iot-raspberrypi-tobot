using Iot.Device.Pwm;

namespace Tobot.Device.MotorHat.Motor;

/// <summary>Controls a DC motor connected to a Motor HAT H-bridge.</summary>
public sealed class DCMotor : IDisposable
{
    private readonly Pca9685 _pca;
    private readonly int _positiveChannel;
    private readonly int _negativeChannel;
    private double? _throttle;
    private DecayMode _decayMode;
    private bool _disposed;

    internal DCMotor(Pca9685 pca, int positiveChannel, int negativeChannel)
    {
        _pca = pca;
        _positiveChannel = positiveChannel;
        _negativeChannel = negativeChannel;
    }

    /// <summary>
    /// Gets or sets motor speed from -1.0 (full reverse) to 1.0 (full forward).
    /// A value of 0 brakes the motor; <see langword="null"/> turns the driver off.
    /// </summary>
    public double? Throttle
    {
        get => _throttle;
        set
        {
            ThrowIfDisposed();
            if (value is < -1.0 or > 1.0 || (value.HasValue && !double.IsFinite(value.Value)))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Throttle must be null or between -1.0 and +1.0.");
            }

            _throttle = value;
            if (value is null)
            {
                WriteDutyCycles(0, 0);
            }
            else if (value.Value == 0)
            {
                WriteDutyCycles(ushort.MaxValue, ushort.MaxValue);
            }
            else
            {
                var dutyCycle = (int)(ushort.MaxValue * Math.Abs(value.Value));
                if (_decayMode == DecayMode.Slow)
                {
                    WriteDutyCycles(
                        value < 0 ? ushort.MaxValue - dutyCycle : ushort.MaxValue,
                        value < 0 ? ushort.MaxValue : ushort.MaxValue - dutyCycle);
                }
                else
                {
                    WriteDutyCycles(value < 0 ? 0 : dutyCycle, value < 0 ? dutyCycle : 0);
                }
            }
        }
    }

    /// <summary>Gets or sets the motor driver's current-decay mode.</summary>
    public DecayMode DecayMode
    {
        get => _decayMode;
        set
        {
            ThrowIfDisposed();
            if (value is not DecayMode.Fast and not DecayMode.Slow)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _decayMode = value;
        }
    }

    /// <summary>Runs the motor forward at the specified throttle.</summary>
    /// <param name="speed">Speed from 0.0 (stopped) to 1.0 (full speed).</param>
    public void Forward(double speed = 1.0)
    {
        ValidateSpeed(speed);
        Throttle = speed;
    }

    /// <summary>Runs the motor backward at the specified throttle.</summary>
    /// <param name="speed">Speed from 0.0 (stopped) to 1.0 (full speed).</param>
    public void Backward(double speed = 1.0)
    {
        ValidateSpeed(speed);
        Throttle = -speed;
    }

    /// <summary>Brakes the motor by driving both H-bridge inputs high.</summary>
    public void Brake() => Throttle = 0;

    /// <summary>Turns off the motor driver, allowing the motor to coast.</summary>
    public void Stop() => Throttle = null;

    private void WriteDutyCycles(int positive, int negative)
    {
        _pca.SetDutyCycle(_positiveChannel, PwmDutyCycle.ToFraction(positive));
        _pca.SetDutyCycle(_negativeChannel, PwmDutyCycle.ToFraction(negative));
    }

    private static void ValidateSpeed(double speed)
    {
        if (!double.IsFinite(speed) || speed is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between 0.0 and 1.0.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>Turns off the motor driver and releases this motor.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _throttle = null;
        WriteDutyCycles(0, 0);
        _disposed = true;
    }
}
