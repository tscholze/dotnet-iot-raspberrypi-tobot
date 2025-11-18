using System;
using System.Device.I2c;

namespace Tobot.Device.PanTiltHat;

/// <summary>
/// Minimal PCA9685 PWM controller driver for controlling servo motors via I2C.
/// Provides frequency setup and per-channel PWM configuration.
/// </summary>
internal sealed class Pca9685 : IDisposable
{
    /// <summary>
    /// The underlying I2C device communicating with the PCA9685 controller.
    /// </summary>
    private readonly I2cDevice _device;

    /// <summary>
    /// Indicates whether this instance has been disposed and its resources released.
    /// </summary>
    private bool _disposed;

    // PCA9685 register addresses
    /// <summary>
    /// Register address for MODE1 (main mode register).
    /// </summary>
    private const byte MODE1 = 0x00;
   
    /// <summary>
    /// Register address for MODE2 (output configuration register).
    /// </summary>
    private const byte MODE2 = 0x01;
   
   /// <summary>
    /// Base register address for LED0 ON low byte (subsequent channels are offset by 4 bytes).
    /// </summary>
    private const byte LED0_ON_L = 0x06; // base for channel registers
    /// <summary>
    /// Register address for prescale configuration (sets PWM frequency).
    /// </summary>
    private const byte PRE_SCALE = 0xFE;

    // MODE1 bits
    /// <summary>
    /// MODE1 bit: Restart flag used to reinitialize the PWM controller.
    /// </summary>
    private const byte MODE1_RESTART = 0x80;
    /// <summary>
    /// MODE1 bit: Auto-increment enables sequential register writes.
    /// </summary>
    private const byte MODE1_AI = 0x20;      // Auto-increment
    
    /// <summary>
    /// MODE1 bit: Sleep flag puts the oscillator into low-power mode.
    /// </summary>
    private const byte MODE1_SLEEP = 0x10;

    // MODE2 bits
    /// <summary>
    /// MODE2 bit: Output drive configuration (totem pole when set).
    /// </summary>
    private const byte MODE2_OUTDRV = 0x04;  // Totem pole structure

    /// <summary>
    /// Initializes a new instance of the <see cref="Pca9685"/> class with the given I2C device.
    /// </summary>
    /// <param name="device">The I2C device representing the PCA9685 controller.</param>
    public Pca9685(I2cDevice device)
    {
        _device = device ?? throw new ArgumentNullException(nameof(device));
        Initialize();
    }

    /// <summary>
    /// Sets the PWM frequency for all channels.
    /// </summary>
    /// <param name="frequencyHz">The desired frequency in Hz (typically 50 Hz for servos).</param>
    public void SetPwmFrequency(double frequencyHz)
    {
        if (frequencyHz <= 0) throw new ArgumentOutOfRangeException(nameof(frequencyHz));

        // Per datasheet: prescale = round(osc_clock / (4096 * freq)) - 1
        const double oscClock = 25_000_000.0; // 25 MHz internal oscillator
        var prescaleVal = (oscClock / (4096.0 * frequencyHz)) - 1.0;
        var prescale = (byte)Math.Clamp(Math.Round(prescaleVal), 3, 255);

        // Enter sleep to set prescale
        var oldMode1 = ReadByte(MODE1);
        var sleepMode = (byte)((oldMode1 & ~MODE1_RESTART) | MODE1_SLEEP);
        WriteByte(MODE1, sleepMode);
        WriteByte(PRE_SCALE, prescale);
        // Restore previous mode (wake up)
        WriteByte(MODE1, oldMode1);
        // Allow oscillator to stabilize
        System.Threading.Thread.Sleep(1);
        // Set restart and AI
        WriteByte(MODE1, (byte)(oldMode1 | MODE1_RESTART | MODE1_AI));
    }

    /// <summary>
    /// Sets the PWM on/off counters for a given channel.
    /// </summary>
    /// <param name="channel">The PCA9685 channel (0-15).</param>
    /// <param name="on">The tick count when the signal goes high (0-4095).</param>
    /// <param name="off">The tick count when the signal goes low (0-4095).</param>
    public void SetPwm(int channel, int on, int off)
    {
        if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        on = Math.Clamp(on, 0, 4095);
        off = Math.Clamp(off, 0, 4095);

        var reg = (byte)(LED0_ON_L + 4 * channel);
        Span<byte> buffer = stackalloc byte[5];
        buffer[0] = reg;
        buffer[1] = (byte)(on & 0xFF);
        buffer[2] = (byte)((on >> 8) & 0x0F);
        buffer[3] = (byte)(off & 0xFF);
        buffer[4] = (byte)((off >> 8) & 0x0F);
        _device.Write(buffer);
    }

    /// <summary>
    /// Convenience for setting duty cycle as a fraction of the full 12-bit range.
    /// </summary>
    /// <param name="channel">The channel (0-15).</param>
    /// <param name="duty">Duty cycle between 0.0 and 1.0.</param>
    public void SetDutyCycle(int channel, double duty)
    {
        duty = Math.Clamp(duty, 0.0, 1.0);
        var off = (int)Math.Round(duty * 4095.0);
        SetPwm(channel, 0, off);
    }

    /// <summary>
    /// Performs initial controller configuration: enables auto-increment, sets output mode,
    /// and applies the default servo frequency.
    /// </summary>
    private void Initialize()
    {
        // Enable auto-increment and configure output driver mode.
        WriteByte(MODE1, MODE1_AI);
        WriteByte(MODE2, MODE2_OUTDRV);
        // Apply a default frequency suitable for typical hobby servos.
        SetPwmFrequency(50.0);
    }

    /// <summary>
    /// Writes a single byte value to a specified register on the PCA9685.
    /// </summary>
    /// <param name="register">Register address to write.</param>
    /// <param name="value">Byte value to store at the register.</param>
    private void WriteByte(byte register, byte value)
    {
        Span<byte> buffer = stackalloc byte[2];
        buffer[0] = register;
        buffer[1] = value;
        _device.Write(buffer);
    }

    /// <summary>
    /// Reads a single byte value from the specified register.
    /// </summary>
    /// <param name="register">Register address to read.</param>
    /// <returns>The byte value stored at the register.</returns>
    private byte ReadByte(byte register)
    {
        _device.WriteByte(register);
        return _device.ReadByte();
    }

    /// <summary>
    /// Releases the I2C device.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _device?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
