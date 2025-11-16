using System;
using System.Device.Gpio;
using System.Linq;

namespace Tobot.Device.ExplorerHat.Digital
{
    /// <summary>
    /// Represents a collection of digital inputs.
    /// </summary>
    public class DigitalInputCollection : IDisposable
    {
        private readonly GpioController _controller;
        private readonly DigitalInput[] _inputs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalInputCollection"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pins">Array of GPIO pin numbers.</param>
        internal DigitalInputCollection(GpioController controller, int[] pins)
        {
            _controller = controller;
            _inputs = pins.Select((pin, index) => new DigitalInput(controller, pin, index + 1)).ToArray();
        }

        /// <summary>
        /// Gets the input at the specified index (1-based).
        /// </summary>
        /// <param name="index">The 1-based index of the input (1-4).</param>
        public DigitalInput this[int index] => _inputs[index - 1];

        /// <summary>
        /// Gets input 1.
        /// </summary>
        public DigitalInput One => _inputs[0];

        /// <summary>
        /// Gets input 2.
        /// </summary>
        public DigitalInput Two => _inputs[1];

        /// <summary>
        /// Gets input 3.
        /// </summary>
        public DigitalInput Three => _inputs[2];

        /// <summary>
        /// Gets input 4.
        /// </summary>
        public DigitalInput Four => _inputs[3];

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var input in _inputs)
            {
                input?.Dispose();
            }
        }
    }
}
