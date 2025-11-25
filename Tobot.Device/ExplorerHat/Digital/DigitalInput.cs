using System.Device.Gpio;

namespace Tobot.Device.ExplorerHat.Digital
{
    /// <summary>
    /// Represents a single digital input.
    /// </summary>
    public class DigitalInput : IDisposable
    {
        private readonly GpioController _controller;
        private readonly int _pin;

        /// <summary>
        /// Gets the input number (1-based).
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalInput"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pin">The GPIO pin number.</param>
        /// <param name="number">The input number.</param>
        internal DigitalInput(GpioController controller, int pin, int number)
        {
            _controller = controller;
            _pin = pin;
            Number = number;
            
            if (!_controller.IsPinOpen(_pin))
            {
                _controller.OpenPin(_pin, PinMode.Input);
            }
        }

        /// <summary>
        /// Reads the current state of the input.
        /// </summary>
        /// <returns>True if the input is high, false otherwise.</returns>
        public bool Read() => _controller.Read(_pin) == PinValue.High;

        /// <summary>
        /// Gets or sets the event handler for when the input changes.
        /// </summary>
        public event PinChangeEventHandler? Changed
        {
            add => _controller.RegisterCallbackForPinValueChangedEvent(_pin, PinEventTypes.Rising | PinEventTypes.Falling, value);
            remove => _controller.UnregisterCallbackForPinValueChangedEvent(_pin, value);
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (_controller.IsPinOpen(_pin))
            {
                _controller.ClosePin(_pin);
            }
        }
    }
}
