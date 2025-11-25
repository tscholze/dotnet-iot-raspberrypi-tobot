using System.Device.Gpio;

namespace Tobot.Device.ExplorerHat.Digital
{
    /// <summary>
    /// Represents a collection of digital outputs.
    /// </summary>
    public class DigitalOutputCollection : IDisposable
    {
        private readonly DigitalOutput[] _outputs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalOutputCollection"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pins">Array of GPIO pin numbers.</param>
        internal DigitalOutputCollection(GpioController controller, int[] pins)
        {
            _outputs = pins.Select((pin, index) => new DigitalOutput(controller, pin, index + 1)).ToArray();
        }

        /// <summary>
        /// Gets the output at the specified index (1-based).
        /// </summary>
        /// <param name="index">The 1-based index of the output (1-4).</param>
        public DigitalOutput this[int index] => _outputs[index - 1];

        /// <summary>
        /// Gets output 1.
        /// </summary>
        public DigitalOutput One => _outputs[0];

        /// <summary>
        /// Gets output 2.
        /// </summary>
        public DigitalOutput Two => _outputs[1];

        /// <summary>
        /// Gets output 3.
        /// </summary>
        public DigitalOutput Three => _outputs[2];

        /// <summary>
        /// Gets output 4.
        /// </summary>
        public DigitalOutput Four => _outputs[3];

        /// <summary>
        /// Turns on all outputs.
        /// </summary>
        public void On()
        {
            foreach (var output in _outputs)
            {
                output.On();
            }
        }

        /// <summary>
        /// Turns off all outputs.
        /// </summary>
        public void Off()
        {
            foreach (var output in _outputs)
            {
                output.Off();
            }
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var output in _outputs)
            {
                output?.Dispose();
            }
        }
    }
}
