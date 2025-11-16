using System.Device.I2c;

namespace Tobot.Device.ExplorerHat.Analog
{
    /// <summary>
    /// Represents a collection of analog inputs using the ADS1015 ADC.
    /// </summary>
    public class AnalogInputCollection
    {
        private readonly I2cDevice? _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogInputCollection"/> class.
        /// </summary>
        /// <param name="device">The I2C device for the ADC.</param>
        internal AnalogInputCollection(I2cDevice? device)
        {
            _device = device;
        }

        /// <summary>
        /// Gets the analog input at the specified index (1-based).
        /// </summary>
        /// <param name="index">The 1-based index of the analog input (1-4).</param>
        public AnalogInput this[int index] => new(_device, index - 1);

        /// <summary>
        /// Gets analog input 1.
        /// </summary>
        public AnalogInput One => new(_device, 0);

        /// <summary>
        /// Gets analog input 2.
        /// </summary>
        public AnalogInput Two => new(_device, 1);

        /// <summary>
        /// Gets analog input 3.
        /// </summary>
        public AnalogInput Three => new(_device, 2);

        /// <summary>
        /// Gets analog input 4.
        /// </summary>
        public AnalogInput Four => new(_device, 3);
    }
}
