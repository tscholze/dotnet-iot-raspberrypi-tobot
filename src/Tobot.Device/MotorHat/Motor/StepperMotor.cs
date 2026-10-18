using Iot.Device.Pwm;

namespace Tobot.Device.MotorHat.Motor;

/// <summary>Controls a bipolar stepper motor connected to a Motor HAT.</summary>
public sealed class StepperMotor : IDisposable
{
    private readonly Pca9685 _pca;
    private readonly int[] _coilChannels;
    private readonly int _microsteps;
    private readonly int[] _microstepCurve;
    private int _currentMicrostep;
    private bool _disposed;

    internal StepperMotor(Pca9685 pca, int[] channels, int microsteps)
    {
        if (microsteps < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(microsteps), "Microsteps must be at least 2.");
        }

        if (microsteps % 2 != 0)
        {
            throw new ArgumentException("Microsteps must be even.", nameof(microsteps));
        }

        _pca = pca;
        _coilChannels = [channels[1], channels[2], channels[0], channels[3]];
        _microsteps = microsteps;
        _microstepCurve = Enumerable.Range(0, microsteps + 1)
            .Select(index => (int)Math.Round(ushort.MaxValue * Math.Sin(Math.PI / (2 * microsteps) * index)))
            .ToArray();
        UpdateCoils();
    }

    /// <summary>Gets the configured number of microsteps between full steps.</summary>
    public int Microsteps => _microsteps;

    /// <summary>Gets the current absolute microstep position maintained by this controller.</summary>
    public int CurrentMicrostep => _currentMicrostep;

    /// <summary>De-energizes all coils so the motor can move freely.</summary>
    public void Release()
    {
        ThrowIfDisposed();
        foreach (var channel in _coilChannels)
        {
            _pca.SetDutyCycle(channel, 0);
        }
    }

    /// <summary>Performs one step and returns the updated microstep position.</summary>
    /// <param name="direction">Direction of movement.</param>
    /// <param name="style">Coil activation style.</param>
    public int Onestep(
        StepperDirection direction = StepperDirection.Forward,
        StepperStyle style = StepperStyle.Single)
    {
        ThrowIfDisposed();
        if (direction is not StepperDirection.Forward and not StepperDirection.Backward)
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        if (style is not StepperStyle.Single and not StepperStyle.Double
            and not StepperStyle.Interleave and not StepperStyle.Microstep)
        {
            throw new ArgumentOutOfRangeException(nameof(style));
        }

        var stepSize = 0;
        var directionValue = direction == StepperDirection.Forward ? 1 : -1;
        if (style == StepperStyle.Microstep)
        {
            stepSize = 1;
        }
        else
        {
            var halfStep = _microsteps / 2;
            var additionalMicrosteps = PositiveModulo(_currentMicrostep, halfStep);
            if (additionalMicrosteps != 0)
            {
                _currentMicrostep += direction == StepperDirection.Forward
                    ? halfStep - additionalMicrosteps
                    : -additionalMicrosteps;
            }
            else if (style == StepperStyle.Interleave)
            {
                stepSize = halfStep;
            }

            var currentInterleave = FloorDivide(_currentMicrostep, halfStep);
            if ((style == StepperStyle.Single && currentInterleave % 2 != 0)
                || (style == StepperStyle.Double && currentInterleave % 2 == 0))
            {
                stepSize = halfStep;
            }
            else if (style is StepperStyle.Single or StepperStyle.Double)
            {
                stepSize = _microsteps;
            }
        }

        _currentMicrostep += directionValue * stepSize;
        UpdateCoils(style == StepperStyle.Microstep);
        return _currentMicrostep;
    }

    private void UpdateCoils(bool microstepping = false)
    {
        var dutyCycles = new int[4];
        var trailingCoil = PositiveModulo(FloorDivide(_currentMicrostep, _microsteps), 4);
        var leadingCoil = (trailingCoil + 1) % 4;
        var microstep = PositiveModulo(_currentMicrostep, _microsteps);
        dutyCycles[leadingCoil] = _microstepCurve[microstep];
        dutyCycles[trailingCoil] = _microstepCurve[_microsteps - microstep];

        if (!microstepping && dutyCycles[leadingCoil] == dutyCycles[trailingCoil]
            && dutyCycles[leadingCoil] > 0)
        {
            dutyCycles[leadingCoil] = ushort.MaxValue;
            dutyCycles[trailingCoil] = ushort.MaxValue;
        }

        for (var coil = 0; coil < _coilChannels.Length; coil++)
        {
            _pca.SetDutyCycle(_coilChannels[coil], PwmDutyCycle.ToFraction(dutyCycles[coil]));
        }
    }

    private static int PositiveModulo(int value, int divisor) => (value % divisor + divisor) % divisor;

    private static int FloorDivide(int value, int divisor)
    {
        var quotient = value / divisor;
        return value % divisor < 0 ? quotient - 1 : quotient;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>Releases all coils and releases this motor controller.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var channel in _coilChannels)
        {
            _pca.SetDutyCycle(channel, 0);
        }

        _disposed = true;
    }
}
