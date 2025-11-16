using System;
using System.Device.I2c;
using System.Threading;

namespace Tobot.Device.ExplorerHat.Analog
{
    /// <summary>
    /// Represents a single analog input channel.
    /// </summary>
    public class AnalogInput
    {
        private readonly I2cDevice? _device;
        private readonly int _channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogInput"/> class.
        /// </summary>
        /// <param name="device">The I2C device for the ADC.</param>
        /// <param name="channel">The ADC channel (0-3).</param>
        internal AnalogInput(I2cDevice? device, int channel)
        {
            _device = device;
            _channel = channel;
        }

        /// <summary>
        /// Reads the analog value from the input.
        /// </summary>
        /// <returns>A value between 0.0 and 5.0 representing the voltage.</returns>
        public double Read()
        {
            if (_device == null) return 0.0;

            try
            {
                // Configure ADS1015 for single-shot conversion
                ushort config = (ushort)(
                    0x8000 |  // Start single conversion
                    ((_channel + 4) << 12) | // Select channel (single-ended)
                    0x0200 |  // +/- 4.096V range
                    0x0000 |  // Continuous conversion mode
                    0x0080 |  // 1600 SPS
                    0x0003    // Disable comparator
                );

                Span<byte> writeBuffer = stackalloc byte[3];
                writeBuffer[0] = 0x01; // Config register
                writeBuffer[1] = (byte)(config >> 8);
                writeBuffer[2] = (byte)(config & 0xFF);
                _device.Write(writeBuffer);

                // Wait for conversion
                Thread.Sleep(1);

                // Read conversion result
                _device.WriteByte(0x00); // Point to conversion register
                Span<byte> readBuffer = stackalloc byte[2];
                _device.Read(readBuffer);

                short raw = (short)((readBuffer[0] << 8) | readBuffer[1]);
                raw >>= 4; // ADS1015 is 12-bit

                // Convert to voltage (assuming 4.096V range)
                return (raw * 4.096) / 2048.0;
            }
            catch
            {
                return 0.0;
            }
        }
    }
}
