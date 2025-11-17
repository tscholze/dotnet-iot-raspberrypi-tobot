using System;
using System.Device.Gpio;
using System.Device.Pwm;

namespace Tobot.Device.ExplorerHat.Motor
{
    /// <summary>
    /// Represents a single motor with H-bridge control.
    /// </summary>
    public class Motor : IDisposable
    {
        private readonly GpioController _controller;
        private readonly MotorPinMapping _pins;
        private readonly PwmChannel _pwmForward;
        private readonly PwmChannel _pwmBackward;
        private double _speed;
        private const int PwmFrequency = 1000; // 1kHz PWM frequency

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

            // Initialize PWM channels for both forward and backward pins
            // Using chip 0 (hardware PWM on Raspberry Pi)
            _pwmForward = PwmChannel.Create(0, _pins.Forward, PwmFrequency);
            _pwmBackward = PwmChannel.Create(0, _pins.Backward, PwmFrequency);
            
            _pwmForward.Start();
            _pwmBackward.Start();

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

            // DRV8833PWP H-Bridge control with PWM for speed control
            // Convert speed percentage to duty cycle (0.0 to 1.0)
            double dutyCycle = Math.Abs(_speed) / 100.0;
            
            if (_speed > 0)
            {
                // Forward: PWM on forward pin, backward pin off
                _pwmBackward.DutyCycle = 0;
                _pwmForward.DutyCycle = dutyCycle;
            }
            else
            {
                // Backward: PWM on backward pin, forward pin off
                _pwmForward.DutyCycle = 0;
                _pwmBackward.DutyCycle = dutyCycle;
            }
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
            _pwmForward.DutyCycle = 0;
            _pwmBackward.DutyCycle = 0;
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _pwmForward.Stop();
            _pwmBackward.Stop();
            _pwmForward.Dispose();
            _pwmBackward.Dispose();
        }
    }
}
