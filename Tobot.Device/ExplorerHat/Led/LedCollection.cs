using System;
using System.Device.Gpio;
using System.Linq;

namespace Tobot.Device.ExplorerHat.Led
{
    /// <summary>
    /// Represents a collection of onboard LEDs.
    /// </summary>
    public class LedCollection : IDisposable
    {
        private readonly Led[] _leds;

        /// <summary>
        /// Initializes a new instance of the <see cref="LedCollection"/> class.
        /// </summary>
        /// <param name="controller">The GPIO controller.</param>
        /// <param name="pins">Array of GPIO pin numbers.</param>
        internal LedCollection(GpioController controller, int[] pins)
        {
            _leds = pins.Select((pin, index) => new Led(controller, pin, index + 1)).ToArray();
        }

        /// <summary>
        /// Gets the LED at the specified index (1-based).
        /// </summary>
        /// <param name="index">The 1-based index of the LED (1-4).</param>
        public Led this[int index] => _leds[index - 1];

        /// <summary>
        /// Gets LED 1.
        /// </summary>
        public Led One => _leds[0];

        /// <summary>
        /// Gets LED 2.
        /// </summary>
        public Led Two => _leds[1];

        /// <summary>
        /// Gets LED 3.
        /// </summary>
        public Led Three => _leds[2];

        /// <summary>
        /// Gets LED 4.
        /// </summary>
        public Led Four => _leds[3];

        /// <summary>
        /// Turns on all LEDs.
        /// </summary>
        public void On()
        {
            foreach (var led in _leds)
            {
                led.On();
            }
        }

        /// <summary>
        /// Turns off all LEDs.
        /// </summary>
        public void Off()
        {
            foreach (var led in _leds)
            {
                led.Off();
            }
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var led in _leds)
            {
                led?.Dispose();
            }
        }
    }
}
