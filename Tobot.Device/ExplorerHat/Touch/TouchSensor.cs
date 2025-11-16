using System.Device.I2c;

namespace Tobot.Device.ExplorerHat.Touch
{
    /// <summary>
    /// Represents a single capacitive touch sensor.
    /// </summary>
    public class TouchSensor
    {
        private readonly I2cDevice? _device;
        private readonly int _channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchSensor"/> class.
        /// </summary>
        /// <param name="device">The I2C device for the capacitive touch sensor.</param>
        /// <param name="channel">The touch sensor channel (0-7).</param>
        internal TouchSensor(I2cDevice? device, int channel)
        {
            _device = device;
            _channel = channel;
        }

        /// <summary>
        /// Reads whether the touch sensor is currently touched.
        /// </summary>
        /// <returns>True if touched, false otherwise.</returns>
        public bool IsTouched()
        {
            if (_device == null) return false;

            try
            {
                _device.WriteByte(0x03); // Touch status register
                byte status = _device.ReadByte();
                return (status & (1 << _channel)) != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
