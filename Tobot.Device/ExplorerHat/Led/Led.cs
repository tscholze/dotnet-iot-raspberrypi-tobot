using System.Device.Gpio;

namespace Tobot.Device.ExplorerHat.Led
{
    /// <summary>
    /// Represents a single LED.
    /// </summary>
    public class Led : IDisposable
    {
        private readonly GpioController _controller;
        private readonly int _pin;

        /// <summary>
        /// Gets the LED number (1-based).
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Led"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pin">The GPIO pin number.</param>
        /// <param name="number">The LED number.</param>
        internal Led(GpioController controller, int pin, int number)
        {
            _controller = controller;
            _pin = pin;
            Number = number;
            
            if (!_controller.IsPinOpen(_pin))
            {
                _controller.OpenPin(_pin, PinMode.Output);
                _controller.Write(_pin, PinValue.Low);
            }
        }

        /// <summary>
        /// Turns the LED on.
        /// </summary>
        public void On() => _controller.Write(_pin, PinValue.High);

        /// <summary>
        /// Turns the LED off.
        /// </summary>
        public void Off() => _controller.Write(_pin, PinValue.Low);

        /// <summary>
        /// Sets the LED state.
        /// </summary>
        /// <param name="on">True to turn on, false to turn off.</param>
        public void Set(bool on) => _controller.Write(_pin, on ? PinValue.High : PinValue.Low);

        /// <summary>
        /// Toggles the LED state.
        /// </summary>
        public void Toggle()
        {
            var current = _controller.Read(_pin);
            _controller.Write(_pin, current == PinValue.High ? PinValue.Low : PinValue.High);
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (_controller.IsPinOpen(_pin))
            {
                _controller.Write(_pin, PinValue.Low);
                _controller.ClosePin(_pin);
            }
        }
    }
}
