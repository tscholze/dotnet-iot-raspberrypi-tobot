using System.Device.I2c;

namespace Tobot.Device.ExplorerHat.Touch
{
    /// <summary>
    /// Represents a collection of capacitive touch sensors.
    /// </summary>
    public class TouchCollection
    {
        private readonly I2cDevice? _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchCollection"/> class.
        /// </summary>
        /// <param name="device">The I2C device for the capacitive touch sensor.</param>
        internal TouchCollection(I2cDevice? device)
        {
            _device = device;
        }

        /// <summary>
        /// Gets the touch sensor at the specified index (1-based).
        /// </summary>
        /// <param name="index">The 1-based index of the touch sensor (1-8).</param>
        public TouchSensor this[int index] => new(_device, index - 1);

        /// <summary>
        /// Reads the state of all touch sensors.
        /// </summary>
        /// <returns>A byte where each bit represents a touch sensor state.</returns>
        public byte ReadAll()
        {
            if (_device == null) return 0;

            try
            {
                _device.WriteByte(0x03); // Touch status register
                return _device.ReadByte();
            }
            catch
            {
                return 0;
            }
        }
    }
}
