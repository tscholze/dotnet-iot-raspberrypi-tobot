using System.Device.I2c;
using Iot.Device.Pwm;
using Tobot.Device.MotorHat.Motor;

namespace Tobot.Device.MotorHat;

/// <summary>
/// Controls the DC and stepper motors on Adafruit's PCA9685-based Motor HAT,
/// Motor Shield, and Motor FeatherWing.
/// </summary>
/// <remarks>
/// This is a .NET port of the public functionality of
/// <see href="https://github.com/adafruit/Adafruit_CircuitPython_MotorKit"/>.
/// </remarks>
public sealed class MotorKit : IDisposable
{
    private const int DefaultI2cBusId = 1;
    private const int DefaultI2cAddress = 0x60;
    private const double DefaultPwmFrequency = 1600.0;
    private const int DefaultStepperMicrosteps = 16;

    private readonly Pca9685 _pca;
    private readonly int _stepperMicrosteps;
    private DCMotor? _motor1;
    private DCMotor? _motor2;
    private DCMotor? _motor3;
    private DCMotor? _motor4;
    private StepperMotor? _stepper1;
    private StepperMotor? _stepper2;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MotorKit"/> class.
    /// </summary>
    /// <param name="address">The PCA9685 I2C address. Defaults to 0x60.</param>
    /// <param name="i2cBusId">The I2C bus ID. Defaults to 1.</param>
    /// <param name="steppersMicrosteps">Microsteps per full step. Defaults to 16.</param>
    /// <param name="pwmFrequency">PCA9685 PWM frequency in hertz. Defaults to 1600.</param>
    public MotorKit(
        int address = DefaultI2cAddress,
        int i2cBusId = DefaultI2cBusId,
        int steppersMicrosteps = DefaultStepperMicrosteps,
        double pwmFrequency = DefaultPwmFrequency)
    {
        if (address is < 0x03 or > 0x77)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "I2C address must be between 0x03 and 0x77.");
        }

        if (i2cBusId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(i2cBusId));
        }

        ValidateFrequency(pwmFrequency);
        _stepperMicrosteps = steppersMicrosteps;

        var device = I2cDevice.Create(new I2cConnectionSettings(i2cBusId, address));
        try
        {
            _pca = new Pca9685(device, pwmFrequency);
        }
        catch
        {
            device.Dispose();
            throw;
        }
    }

    /// <summary>Gets or sets the overall PCA9685 PWM frequency in hertz.</summary>
    public double Frequency
    {
        get
        {
            ThrowIfDisposed();
            return _pca.PwmFrequency;
        }
        set
        {
            ThrowIfDisposed();
            ValidateFrequency(value);
            _pca.PwmFrequency = value;
        }
    }

    /// <summary>Gets the configured number of microsteps per full step.</summary>
    public int SteppersMicrosteps => _stepperMicrosteps;

    /// <summary>Gets the controls for DC motor 1.</summary>
    public DCMotor Motor1 => GetMotor(1);

    /// <summary>Gets the controls for DC motor 2.</summary>
    public DCMotor Motor2 => GetMotor(2);

    /// <summary>Gets the controls for DC motor 3.</summary>
    public DCMotor Motor3 => GetMotor(3);

    /// <summary>Gets the controls for DC motor 4.</summary>
    public DCMotor Motor4 => GetMotor(4);

    /// <summary>Gets the controls for stepper motor 1, using the M1 and M2 terminals.</summary>
    public StepperMotor Stepper1
    {
        get
        {
            ThrowIfDisposed();
            if (_stepper1 is not null)
            {
                return _stepper1;
            }

            if (_motor1 is not null || _motor2 is not null)
            {
                throw new InvalidOperationException("Stepper 1 cannot be used while DC motor 1 or 2 is in use.");
            }

            var stepper = new StepperMotor(
                _pca,
                new[] { 10, 9, 11, 12 },
                _stepperMicrosteps);
            _pca.SetDutyCycle(8, 1.0);
            _pca.SetDutyCycle(13, 1.0);
            _stepper1 = stepper;
            return stepper;
        }
    }

    /// <summary>Gets the controls for stepper motor 2, using the M3 and M4 terminals.</summary>
    public StepperMotor Stepper2
    {
        get
        {
            ThrowIfDisposed();
            if (_stepper2 is not null)
            {
                return _stepper2;
            }

            if (_motor3 is not null || _motor4 is not null)
            {
                throw new InvalidOperationException("Stepper 2 cannot be used while DC motor 3 or 4 is in use.");
            }

            var stepper = new StepperMotor(
                _pca,
                new[] { 4, 3, 5, 6 },
                _stepperMicrosteps);
            _pca.SetDutyCycle(7, 1.0);
            _pca.SetDutyCycle(2, 1.0);
            _stepper2 = stepper;
            return stepper;
        }
    }

    private DCMotor GetMotor(int number)
    {
        ThrowIfDisposed();

        return number switch
        {
            1 => _motor1 ??= CreateMotor(1, 8, 9, 10, 1),
            2 => _motor2 ??= CreateMotor(2, 13, 11, 12, 1),
            3 => _motor3 ??= CreateMotor(3, 2, 3, 4, 2),
            4 => _motor4 ??= CreateMotor(4, 7, 5, 6, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(number))
        };
    }

    private DCMotor CreateMotor(int number, int enableChannel, int positiveChannel, int negativeChannel, int stepperNumber)
    {
        var stepperInUse = stepperNumber == 1 ? _stepper1 is not null : _stepper2 is not null;
        if (stepperInUse)
        {
            throw new InvalidOperationException($"DC motor {number} cannot be used while stepper {stepperNumber} is in use.");
        }

        _pca.SetDutyCycle(enableChannel, 1.0);
        return new DCMotor(_pca, positiveChannel, negativeChannel);
    }

    private static void ValidateFrequency(double frequency)
    {
        if (!double.IsFinite(frequency) || frequency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frequency), "PWM frequency must be a finite value greater than zero.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>Stops and releases all motors and disposes the PCA9685 and its I2C device.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _motor1?.Dispose();
        _motor2?.Dispose();
        _motor3?.Dispose();
        _motor4?.Dispose();
        _stepper1?.Dispose();
        _stepper2?.Dispose();
        _pca.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
