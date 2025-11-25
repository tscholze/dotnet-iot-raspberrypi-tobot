using System.Device.Gpio;

namespace Tobot.Device.ExplorerHat.Motor
{
    /// <summary>
    /// Represents a collection of motors.
    /// </summary>
    public class MotorCollection : IDisposable
    {
        private readonly Motor[] _motors;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotorCollection"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="motor1Pins">Pin mapping for motor 1.</param>
        /// <param name="motor2Pins">Pin mapping for motor 2.</param>
        internal MotorCollection(GpioController controller, MotorPinMapping motor1Pins, MotorPinMapping motor2Pins)
        {
            _motors =
            [
                new Motor(controller, motor1Pins, 1),
                new Motor(controller, motor2Pins, 2)
            ];
        }

        /// <summary>
        /// Gets the motor at the specified index (1-based).
        /// </summary>
        /// <param name="index">The 1-based index of the motor (1-2).</param>
        public Motor this[int index] => _motors[index - 1];

        /// <summary>
        /// Gets motor 1.
        /// </summary>
        public Motor One => _motors[0];

        /// <summary>
        /// Gets motor 2.
        /// </summary>
        public Motor Two => _motors[1];

        /// <summary>
        /// Stops all motors.
        /// </summary>
        public void Stop()
        {
            foreach (var motor in _motors)
            {
                motor.Stop();
            }
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var motor in _motors)
            {
                motor?.Dispose();
            }
        }
    }
}
