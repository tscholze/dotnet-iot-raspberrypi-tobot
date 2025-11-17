using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

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
        private const int PwmFrequency = 100; // 100 Hz PWM frequency
        private CancellationTokenSource? _pwmCancellation;
        private Task? _pwmTask;

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

            // Initialize pins for software PWM
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

            // Stop any existing PWM task
            _pwmCancellation?.Cancel();
            _pwmTask?.Wait();

            if (Math.Abs(_speed) < 0.01)
            {
                Stop();
                return;
            }

            // Start software PWM
            _pwmCancellation = new CancellationTokenSource();
            var dutyCycle = Math.Abs(_speed) / 100.0;
            var isForward = _speed > 0;
            
            _pwmTask = Task.Run(() => SoftwarePwm(isForward, dutyCycle, _pwmCancellation.Token));
        }

        /// <summary>
        /// Software PWM implementation.
        /// </summary>
        private void SoftwarePwm(bool forward, double dutyCycle, CancellationToken cancellationToken)
        {
            var periodMs = 1000.0 / PwmFrequency;
            var onTimeMs = periodMs * dutyCycle;
            var offTimeMs = periodMs - onTimeMs;

            var activePin = forward ? _pins.Forward : _pins.Backward;
            var inactivePin = forward ? _pins.Backward : _pins.Forward;

            // Keep inactive pin low
            _controller.Write(inactivePin, PinValue.Low);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (onTimeMs > 0)
                {
                    _controller.Write(activePin, PinValue.High);
                    Thread.Sleep(TimeSpan.FromMilliseconds(onTimeMs));
                }

                if (offTimeMs > 0 && !cancellationToken.IsCancellationRequested)
                {
                    _controller.Write(activePin, PinValue.Low);
                    Thread.Sleep(TimeSpan.FromMilliseconds(offTimeMs));
                }
            }

            // Ensure pin is low when stopped
            _controller.Write(activePin, PinValue.Low);
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
            _pwmCancellation?.Cancel();
            _pwmTask?.Wait();
            _controller.Write(_pins.Forward, PinValue.Low);
            _controller.Write(_pins.Backward, PinValue.Low);
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _pwmCancellation?.Dispose();
            
            if (_controller.IsPinOpen(_pins.Forward))
                _controller.ClosePin(_pins.Forward);
            if (_controller.IsPinOpen(_pins.Backward))
                _controller.ClosePin(_pins.Backward);
        }
    }
}
