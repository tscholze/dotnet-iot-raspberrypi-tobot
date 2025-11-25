using System;
using System.Device.I2c;

namespace Tobot.Device.PanTiltHat;

/// <summary>
/// Configuration for the Pan-Tilt HAT.
/// </summary>
public sealed class PanTiltConfig
{
    /// <summary>
    /// I2C bus identifier. Defaults to 1 on Raspberry Pi.
    /// </summary>
    public int I2cBusId { get; init; } = 1;

    /// <summary>
    /// I2C address of the PCA9685 controller. Default is 0x40.
    /// </summary>
    public int I2cAddress { get; init; } = 0x15;

    /// <summary>
    /// PWM frequency in Hz for servo control. Default is 50 Hz.
    /// </summary>
    public double PwmFrequencyHz { get; init; } = 50.0;

    /// <summary>
    /// Minimum pulse width in microseconds corresponding to -90°.
    /// </summary>
    public int MinPulseUs { get; init; } = 500;

    /// <summary>
    /// Maximum pulse width in microseconds corresponding to +90°.
    /// </summary>
    public int MaxPulseUs { get; init; } = 2500;

    /// <summary>
    /// Channel index for the PAN servo on the PCA9685 (0-15). Defaults to 0.
    /// </summary>
    public int PanChannel { get; init; } = 0;

    /// <summary>
    /// Channel index for the TILT servo on the PCA9685 (0-15). Defaults to 1.
    /// </summary>
    public int TiltChannel { get; init; } = 1;
}

/// <summary>
/// Driver for the Pimoroni Pan-Tilt HAT using a PCA9685 PWM controller over I2C.
/// Provides high-level methods to set pan and tilt angles, center the gimbal,
/// and enable/disable (sleep/wake) the PWM controller.
/// </summary>
public sealed class PanTiltHat : IDisposable
{
    /// <summary>
    /// Underlying PCA9685 PWM controller instance used to drive the pan and tilt servos.
    /// </summary>
    private readonly Pca9685 _pwm;
    /// <summary>
    /// Configuration settings controlling I2C bus/address, pulse range and channel mapping.
    /// </summary>
    private readonly PanTiltConfig _config;
    /// <summary>
    /// Indicates whether this instance created (and therefore owns) the I2C device.
    /// </summary>
    private readonly bool _ownsDevice;
    /// <summary>
    /// Tracks disposal state to prevent double-disposal of resources.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Cached last commanded pan angle in degrees.
    /// </summary>
    private double _currentPan;
    /// <summary>
    /// Cached last commanded tilt angle in degrees.
    /// </summary>
    private double _currentTilt;

    /// <summary>
    /// Initializes a new instance of the <see cref="PanTiltHat"/> class.
    /// </summary>
    /// <param name="i2cDevice">Optional I2C device. If not provided, one will be created using <paramref name="config"/>.</param>
    /// <param name="config">Configuration for the device. If null, defaults are used.</param>
    public PanTiltHat(I2cDevice? i2cDevice = null, PanTiltConfig? config = null)
    {
        _config = config ?? new PanTiltConfig();

        if (i2cDevice is null)
        {
            var settings = new I2cConnectionSettings(_config.I2cBusId, _config.I2cAddress);
            i2cDevice = I2cDevice.Create(settings);
            _ownsDevice = true;
        }

        _pwm = new Pca9685(i2cDevice);
        _pwm.SetPwmFrequency(_config.PwmFrequencyHz);

        // Initialize to center
        Center();
    }

    /// <summary>
    /// Gets the current pan angle in degrees.
    /// </summary>
    public double CurrentPanAngle => _currentPan;

    /// <summary>
    /// Gets the current tilt angle in degrees.
    /// </summary>
    public double CurrentTiltAngle => _currentTilt;

    /// <summary>
    /// Sets both pan and tilt angles in one operation.
    /// </summary>
    /// <param name="panDegrees">Pan angle in degrees (-90..90).</param>
    /// <param name="tiltDegrees">Tilt angle in degrees (-90..90).</param>
    public void SetAngles(double panDegrees, double tiltDegrees)
    {
        SetPanAngle(panDegrees);
        SetTiltAngle(tiltDegrees);
    }

    /// <summary>
    /// Sets the pan angle.
    /// </summary>
    /// <param name="degrees">Angle in degrees (-90..90).</param>
    public void SetPanAngle(double degrees)
    {
        degrees = ClampAngle(degrees);
        _currentPan = degrees;
        var pulseUs = AngleToPulseUs(degrees);
        SetPulseUs(_config.PanChannel, pulseUs);
    }

    /// <summary>
    /// Sets the tilt angle.
    /// </summary>
    /// <param name="degrees">Angle in degrees (-90..90).</param>
    public void SetTiltAngle(double degrees)
    {
        degrees = ClampAngle(degrees);
        _currentTilt = degrees;
        var pulseUs = AngleToPulseUs(degrees);
        SetPulseUs(_config.TiltChannel, pulseUs);
    }

    /// <summary>
    /// Centers both servos at 0°.
    /// </summary>
    public void Center()
    {
        SetAngles(0, 0);
    }

    /// <summary>
    /// Converts an angle in degrees (-90..90) to a pulse width in microseconds within configured limits.
    /// </summary>
    /// <param name="degrees">Angle in degrees.</param>
    /// <returns>Pulse width in microseconds.</returns>
    private int AngleToPulseUs(double degrees)
    {
        // Map [-90, 90] to [MinPulseUs, MaxPulseUs]
        var t = (degrees + 90.0) / 180.0; // 0..1
        var us = _config.MinPulseUs + t * (_config.MaxPulseUs - _config.MinPulseUs);
        return (int)Math.Round(us);
    }

    /// <summary>
    /// Sets the raw pulse width in microseconds on a given channel.
    /// </summary>
    /// <param name="channel">PCA9685 channel (0-15).</param>
    /// <param name="microseconds">Pulse length in microseconds.</param>
    private void SetPulseUs(int channel, int microseconds)
    {
        // Convert microseconds to 12-bit ticks based on current period
        var periodUs = 1_000_000.0 / _config.PwmFrequencyHz; // e.g., 20,000us for 50 Hz
        var ticks = (int)Math.Round((microseconds / periodUs) * 4096.0);
        ticks = Math.Clamp(ticks, 0, 4095);
        _pwm.SetPwm(channel, 0, ticks);
    }

    /// <summary>
    /// Ensures the provided angle is within -90..90.
    /// </summary>
    /// <param name="degrees">Angle in degrees.</param>
    private static double ClampAngle(double degrees)
        => Math.Clamp(degrees, -90.0, 90.0);

    /// <summary>
    /// Puts the PWM controller into a known centered state.
    /// </summary>
    public void Wake()
    {
        // No-op for minimal driver; ensured by constructor and SetPwmFrequency
        Center();
    }

    /// <summary>
    /// Releases resources used by the PanTilt HAT.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _pwm?.Dispose();
        // If we created the I2C device internally, it is disposed by _pwm.
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
 
