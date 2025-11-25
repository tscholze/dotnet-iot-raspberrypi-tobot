using System.Device.I2c;

namespace Tobot.Device.PanTiltHat;

/// <summary>
/// Driver for the Pimoroni Pan-Tilt HAT.
/// Communicates with the HAT's microcontroller over I2C to control pan and tilt servos.
/// The HAT uses a microcontroller at I2C address 0x15 that handles servo PWM generation.
/// </summary>
/// <remarks>
/// This driver implements the same protocol as the official Pimoroni Python library.
/// Default servo pulse ranges are 575-2325 microseconds, corresponding to -90° to +90°.
/// Servos can be automatically disabled after a configurable idle timeout to save power.
/// </remarks>
public sealed class PanTiltHat : IDisposable
{
    #region Register Definitions
    
    /// <summary>
    /// Configuration register address. Controls servo enable states and light settings.
    /// </summary>
    private const byte REG_CONFIG = 0x00;
    
    /// <summary>
    /// Servo 1 (Pan) register address. Stores pulse width in microseconds as a 16-bit word.
    /// </summary>
    private const byte REG_SERVO1 = 0x03;
    
    /// <summary>
    /// Servo 2 (Tilt) register address. Stores pulse width in microseconds as a 16-bit word.
    /// </summary>
    private const byte REG_SERVO2 = 0x01;
    
    /// <summary>
    /// WS2812 LED data register base address (not implemented in this driver).
    /// </summary>
    private const byte REG_WS2812 = 0x05;
    
    /// <summary>
    /// Update register for triggering LED updates (not implemented in this driver).
    /// </summary>
    private const byte REG_UPDATE = 0x4E;
    
    #endregion

    #region Constants
    
    /// <summary>
    /// Default I2C bus number for Raspberry Pi.
    /// </summary>
    private const int DefaultI2cBus = 1;
    
    /// <summary>
    /// I2C address of the Pan-Tilt HAT microcontroller.
    /// </summary>
    private const int DefaultI2cAddress = 0x15;
    
    /// <summary>
    /// Default idle timeout in seconds. Servos are automatically disabled after this period of inactivity.
    /// </summary>
    private const double DefaultIdleTimeout = 2.0;
    
    /// <summary>
    /// Default minimum pulse width in microseconds for servos. Corresponds to -90°.
    /// </summary>
    private const int DefaultServoMin = 575;
    
    /// <summary>
    /// Default maximum pulse width in microseconds for servos. Corresponds to +90°.
    /// </summary>
    private const int DefaultServoMax = 2325;
    
    /// <summary>
    /// Maximum number of I2C operation retry attempts on communication failure.
    /// </summary>
    private const int I2cRetries = 10;
    
    /// <summary>
    /// Delay in milliseconds between I2C retry attempts.
    /// </summary>
    private const int I2cRetryDelayMs = 10;
    
    #endregion

    #region Private Fields
    
    /// <summary>
    /// The I2C device for communicating with the Pan-Tilt HAT microcontroller.
    /// </summary>
    private readonly I2cDevice _device;
    
    /// <summary>
    /// Indicates whether this instance owns the I2C device and should dispose it.
    /// </summary>
    private readonly bool _ownsDevice;
    
    /// <summary>
    /// Tracks whether this instance has been disposed.
    /// </summary>
    private bool _disposed;
    
    /// <summary>
    /// Idle timeout in seconds. Servos are automatically disabled after this period of inactivity.
    /// Set to 0 to disable automatic timeout.
    /// </summary>
    private double _idleTimeout = DefaultIdleTimeout;
    
    /// <summary>
    /// Timer for automatically disabling servo 1 after idle timeout.
    /// </summary>
    private Timer? _servo1Timer;
    
    /// <summary>
    /// Timer for automatically disabling servo 2 after idle timeout.
    /// </summary>
    private Timer? _servo2Timer;
    
    /// <summary>
    /// Flag indicating whether servo 1 (pan) is currently enabled.
    /// </summary>
    private bool _enableServo1;
    
    /// <summary>
    /// Flag indicating whether servo 2 (tilt) is currently enabled.
    /// </summary>
    private bool _enableServo2;
    
    /// <summary>
    /// Last commanded angle for servo 1 (pan) in degrees.
    /// </summary>
    private int _lastPanAngle;
    
    /// <summary>
    /// Last commanded angle for servo 2 (tilt) in degrees.
    /// </summary>
    private int _lastTiltAngle;
    
    #endregion

    #region Constructor and Initialization
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PanTiltHat"/> class with default settings.
    /// Creates an I2C connection on bus 1 at address 0x15.
    /// Servos are configured with pulse range 575-2325 microseconds and 2 second idle timeout.
    /// </summary>
    public PanTiltHat()
    {
        var settings = new I2cConnectionSettings(DefaultI2cBus, DefaultI2cAddress);
        _device = I2cDevice.Create(settings);
        _ownsDevice = true;
        
        // Initialize: enable both servos
        _enableServo1 = true;
        _enableServo2 = true;
        SetConfig();
    }
    
    #endregion

    #region Public Properties
    
    /// <summary>
    /// Gets or sets the idle timeout in seconds.
    /// After this period of inactivity, servos are automatically disabled to save power.
    /// Set to 0 to disable automatic timeout.
    /// </summary>
    public double IdleTimeout
    {
        get => _idleTimeout;
        set => _idleTimeout = value;
    }
    
    /// <summary>
    /// Gets the last commanded pan angle in degrees (-90 to +90).
    /// </summary>
    public int CurrentPanAngle => _lastPanAngle;
    
    /// <summary>
    /// Gets the last commanded tilt angle in degrees (-90 to +90).
    /// </summary>
    public int CurrentTiltAngle => _lastTiltAngle;
    
    #endregion

    #region Public Servo Control Methods
    
    /// <summary>
    /// Sets the pan (servo 1) angle.
    /// </summary>
    /// <param name="angle">Angle in degrees from -90 to +90.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when angle is outside the valid range.</exception>
    public void Pan(int angle)
    {
        CheckAngleRange(angle);
        
        // Enable servo if not already enabled
        if (!_enableServo1)
        {
            _enableServo1 = true;
            SetConfig();

            Thread.Sleep(300);
        }
        
        var us = DegreesToMicroseconds(angle, DefaultServoMin, DefaultServoMax);
        WriteWord(REG_SERVO1, (ushort)us);
        _lastPanAngle = angle;
        
        // Reset idle timer
        if (_idleTimeout > 0)
        {
            _servo1Timer?.Dispose();
            _servo1Timer = new Timer(_ => DisableServo1(), null, 
                TimeSpan.FromSeconds(_idleTimeout), Timeout.InfiniteTimeSpan);
        }
    }
    
    /// <summary>
    /// Sets the tilt (servo 2) angle.
    /// </summary>
    /// <param name="angle">Angle in degrees from -90 to +90.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when angle is outside the valid range.</exception>
    public void Tilt(int angle)
    {
        CheckAngleRange(angle);
        
        // Enable servo if not already enabled
        if (!_enableServo2)
        {
            _enableServo2 = true;
            SetConfig();

            Thread.Sleep(300);
        }
        
        var us = DegreesToMicroseconds(angle, DefaultServoMin, DefaultServoMax);
        WriteWord(REG_SERVO2, (ushort)us);
        _lastTiltAngle = angle;
        
        // Reset idle timer
        if (_idleTimeout > 0)
        {
            _servo2Timer?.Dispose();
            _servo2Timer = new Timer(_ => DisableServo2(), null, 
                TimeSpan.FromSeconds(_idleTimeout), Timeout.InfiniteTimeSpan);
        }
    }
    
    /// <summary>
    /// Gets the current pan (servo 1) angle by reading back from the device.
    /// </summary>
    /// <returns>Current angle in degrees, or 0 if unable to read.</returns>
    public int GetPan()
    {
        try
        {
            var us = ReadWord(REG_SERVO1);
            return MicrosecondsToAngles(us, DefaultServoMin, DefaultServoMax);
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Gets the current tilt (servo 2) angle by reading back from the device.
    /// </summary>
    /// <returns>Current angle in degrees, or 0 if unable to read.</returns>
    public int GetTilt()
    {
        try
        {
            var us = ReadWord(REG_SERVO2);
            return MicrosecondsToAngles(us, DefaultServoMin, DefaultServoMax);
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Enables or disables a servo.
    /// Disabling a servo turns off the drive signal to save power.
    /// </summary>
    /// <param name="servoIndex">Servo index: 1 for pan, 2 for tilt.</param>
    /// <param name="enable">True to enable, false to disable.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when servoIndex is not 1 or 2.</exception>
    public void ServoEnable(int servoIndex, bool enable)
    {
        if (servoIndex != 1 && servoIndex != 2)
            throw new ArgumentOutOfRangeException(nameof(servoIndex), "Servo index must be 1 or 2");
        
        if (servoIndex == 1)
            _enableServo1 = enable;
        else
            _enableServo2 = enable;
        
        SetConfig();
    }
    

    
    #endregion

    #region Private Helper Methods
    
    /// <summary>
    /// Disables servo 1 (pan) by clearing its enable flag and updating the configuration register.
    /// Called automatically by the idle timeout timer.
    /// </summary>
    private void DisableServo1()
    {
        _enableServo1 = false;
        SetConfig();
    }
    
    /// <summary>
    /// Disables servo 2 (tilt) by clearing its enable flag and updating the configuration register.
    /// Called automatically by the idle timeout timer.
    /// </summary>
    private void DisableServo2()
    {
        _enableServo2 = false;
        SetConfig();
    }
    
    /// <summary>
    /// Writes the configuration byte to the HAT's config register.
    /// The config byte encodes servo enable states and light settings.
    /// Bit 0: Servo 1 enable, Bit 1: Servo 2 enable, Bit 2: Lights enable (not used here).
    /// </summary>
    private void SetConfig()
    {
        byte config = 0;
        if (_enableServo1) config |= 0x01;
        if (_enableServo2) config |= 0x02;
        // Bit 2 (lights enable) left at 0
        
        WriteByte(REG_CONFIG, config);
    }
    
    /// <summary>
    /// Converts an angle in degrees to a pulse width in microseconds.
    /// </summary>
    /// <param name="angle">Angle in degrees from -90 to +90.</param>
    /// <param name="minUs">Minimum pulse width in microseconds (corresponds to -90°).</param>
    /// <param name="maxUs">Maximum pulse width in microseconds (corresponds to +90°).</param>
    /// <returns>Pulse width in microseconds.</returns>
    private static int DegreesToMicroseconds(int angle, int minUs, int maxUs)
    {
        // Map angle from [-90, 90] to [0, 180]
        var normalized = angle + 90;
        var range = maxUs - minUs;
        var us = minUs + (range * normalized / 180);
        return us;
    }
    
    /// <summary>
    /// Converts a pulse width in microseconds to an angle in degrees.
    /// </summary>
    /// <param name="us">Pulse width in microseconds.</param>
    /// <param name="minUs">Minimum pulse width in microseconds (corresponds to -90°).</param>
    /// <param name="maxUs">Maximum pulse width in microseconds (corresponds to +90°).</param>
    /// <returns>Angle in degrees from -90 to +90.</returns>
    private static int MicrosecondsToAngles(int us, int minUs, int maxUs)
    {
        var range = maxUs - minUs;
        var angle = (us - minUs) * 180 / range;
        return angle - 90;
    }
    
    /// <summary>
    /// Validates that an angle is within the valid range of -90 to +90 degrees.
    /// </summary>
    /// <param name="angle">Angle to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when angle is outside valid range.</exception>
    private static void CheckAngleRange(int angle)
    {
        if (angle < -90 || angle > 90)
            throw new ArgumentOutOfRangeException(nameof(angle), 
                $"Angle must be between -90 and 90, got {angle}");
    }
    
    #endregion

    #region I2C Communication Methods
    
    /// <summary>
    /// Writes a single byte to a register on the HAT with retry logic.
    /// </summary>
    /// <param name="register">Register address.</param>
    /// <param name="value">Byte value to write.</param>
    /// <exception cref="IOException">Thrown when all retry attempts fail.</exception>
    private void WriteByte(byte register, byte value)
    {
        for (int attempt = 0; attempt < I2cRetries; attempt++)
        {
            try
            {
                Span<byte> buffer = stackalloc byte[2];
                buffer[0] = register;
                buffer[1] = value;
                _device.Write(buffer);
                return;
            }
            catch (Exception)
            {
                if (attempt == I2cRetries - 1)
                    throw new System.IO.IOException($"Failed to write byte to register 0x{register:X2} after {I2cRetries} attempts");
                Thread.Sleep(I2cRetryDelayMs);
            }
        }
    }
    
    /// <summary>
    /// Writes a 16-bit word (little-endian) to a register on the HAT with retry logic.
    /// This matches the SMBus write_word_data behavior: low byte first, then high byte.
    /// </summary>
    /// <param name="register">Register address.</param>
    /// <param name="value">16-bit value to write.</param>
    /// <exception cref="IOException">Thrown when all retry attempts fail.</exception>
    private void WriteWord(byte register, ushort value)
    {
        for (int attempt = 0; attempt < I2cRetries; attempt++)
        {
            try
            {
                Span<byte> buffer = stackalloc byte[3];
                buffer[0] = register;
                buffer[1] = (byte)(value & 0xFF);        // Low byte
                buffer[2] = (byte)((value >> 8) & 0xFF); // High byte
                _device.Write(buffer);
                return;
            }
            catch (Exception)
            {
                if (attempt == I2cRetries - 1)
                    throw new System.IO.IOException($"Failed to write word to register 0x{register:X2} after {I2cRetries} attempts");
                Thread.Sleep(I2cRetryDelayMs);
            }
        }
    }
    
    /// <summary>
    /// Reads a 16-bit word (little-endian) from a register on the HAT with retry logic.
    /// </summary>
    /// <param name="register">Register address.</param>
    /// <returns>16-bit value read from the register.</returns>
    /// <exception cref="IOException">Thrown when all retry attempts fail.</exception>
    private ushort ReadWord(byte register)
    {
        for (int attempt = 0; attempt < I2cRetries; attempt++)
        {
            try
            {
                _device.WriteByte(register);
                Span<byte> buffer = stackalloc byte[2];
                _device.Read(buffer);
                return (ushort)(buffer[0] | (buffer[1] << 8));
            }
            catch (Exception)
            {
                if (attempt == I2cRetries - 1)
                    throw new System.IO.IOException($"Failed to read word from register 0x{register:X2} after {I2cRetries} attempts");
                Thread.Sleep(I2cRetryDelayMs);
            }
        }
        
        return 0; // unreachable
    }
    
    #endregion

    #region IDisposable Implementation
    
    /// <summary>
    /// Releases all resources used by the <see cref="PanTiltHat"/> instance.
    /// Disables both servos and disposes timers and the I2C device if owned.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        
        // Cancel idle timers
        _servo1Timer?.Dispose();
        _servo2Timer?.Dispose();
        
        // Disable servos
        _enableServo1 = false;
        _enableServo2 = false;
        try
        {
            SetConfig();
        }
        catch
        {
            // Ignore errors during disposal
        }
        
        // Dispose I2C device if we own it
        if (_ownsDevice)
        {
            _device?.Dispose();
        }
        
        _disposed = true;
    }
    
    #endregion
}
