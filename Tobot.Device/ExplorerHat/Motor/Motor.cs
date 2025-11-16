using System;
using System.Device.Gpio;

namespace Tobot.Device.ExplorerHat.Motor
{
    /// <summary>
    /// Represents a single motor with H-bridge control.
    /// </summary>
    public class Motor : IDisposable
    {
        private readonly GpioController _controller;
        private readonly MotorPinMapping _pins;
        private double _speed;

        /// <summary>
        /// Gets the motor number (1-based).
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Motor"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pins">The pin mapping for this motor.</param>
        /// <param name="number">The motor number.</param>
        internal Motor(GpioController controller, MotorPinMapping pins, int number)
        {
            _controller = controller;
            _pins = pins;
            Number = number;

            // Initialize pins
            if (!_controller.IsPinOpen(_pins.Enable))
                _controller.OpenPin(_pins.Enable, PinMode.Output);
            if (!_controller.IsPinOpen(_pins.Forward))
                _controller.OpenPin(_pins.Forward, PinMode.Output);
            if (!_controller.IsPinOpen(_pins.Backward))
                _controller.OpenPin(_pins.Backward, PinMode.Output);

            Stop();
        }

        /// <summary>
        /// Sets the motor speed and direction.
        /// </summary>
        /// <param name="speed">Speed value from -100 (full backward) to +100 (full forward). 0 stops the motor.</param>
        public void SetSpeed(double speed)
        {
            _speed = Math.Clamp(speed, -100, 100);

            if (Math.Abs(_speed) < 0.01)
            {
                Stop();
                return;
            }

            bool forward = _speed > 0;
            _controller.Write(_pins.Forward, forward ? PinValue.High : PinValue.Low);
            _controller.Write(_pins.Backward, forward ? PinValue.Low : PinValue.High);
            
            // In a real implementation, you would use PWM on the enable pin
            // For now, just turn it on
            _controller.Write(_pins.Enable, PinValue.High);
        }

        /// <summary>
        /// Moves the motor forward at the specified speed.
        /// </summary>
        /// <param name="speed">Speed value from 0 to 100.</param>
        public void Forward(double speed = 100) => SetSpeed(Math.Abs(speed));

        /// <summary>
        /// Moves the motor backward at the specified speed.
        /// </summary>
        /// <param name="speed">Speed value from 0 to 100.</param>
        public void Backward(double speed = 100) => SetSpeed(-Math.Abs(speed));

        /// <summary>
        /// Stops the motor.
        /// </summary>
        public void Stop()
        {
            _speed = 0;
            _controller.Write(_pins.Enable, PinValue.Low);
            _controller.Write(_pins.Forward, PinValue.Low);
            _controller.Write(_pins.Backward, PinValue.Low);
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            if (_controller.IsPinOpen(_pins.Enable))
                _controller.ClosePin(_pins.Enable);
            if (_controller.IsPinOpen(_pins.Forward))
                _controller.ClosePin(_pins.Forward);
            if (_controller.IsPinOpen(_pins.Backward))
                _controller.ClosePin(_pins.Backward);
        }
    }
}
