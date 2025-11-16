using System;
using System.Device.Gpio;

namespace Tobot.Device.ExplorerHat.Digital
{
    /// <summary>
    /// Represents a single digital output.
    /// </summary>
    public class DigitalOutput : IDisposable
    {
        private readonly GpioController _controller;
        private readonly int _pin;

        /// <summary>
        /// Gets the output number (1-based).
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalOutput"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pin">The GPIO pin number.</param>
        /// <param name="number">The output number.</param>
        internal DigitalOutput(GpioController controller, int pin, int number)
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
        /// Turns the output on (high).
        /// </summary>
        public void On() => _controller.Write(_pin, PinValue.High);

        /// <summary>
        /// Turns the output off (low).
        /// </summary>
        public void Off() => _controller.Write(_pin, PinValue.Low);

        /// <summary>
        /// Writes a value to the output.
        /// </summary>
        /// <param name="value">True for high, false for low.</param>
        public void Write(bool value) => _controller.Write(_pin, value ? PinValue.High : PinValue.Low);

        /// <summary>
        /// Toggles the output state.
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
