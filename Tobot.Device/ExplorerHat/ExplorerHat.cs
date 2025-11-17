using System;
using System.Device.Gpio;
using System.Device.I2c;
using Tobot.Device.ExplorerHat.Analog;
using Tobot.Device.ExplorerHat.Digital;
using Tobot.Device.ExplorerHat.Led;
using Tobot.Device.ExplorerHat.Motor;
using Tobot.Device.ExplorerHat.Touch;

namespace Tobot.Device.ExplorerHat
{
    /// <summary>
    /// Represents the Pimoroni Explorer HAT for Raspberry Pi.
    /// Provides access to digital inputs, outputs, analog inputs, motors, and capacitive touch sensors.
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        #region Constants

        /// <summary>
        /// I2C address for the ADS1015 ADC chip.
        /// </summary>
        private const int ADS1015_ADDRESS = 0x48;

        /// <summary>
        /// I2C address for the CAP1208 capacitive touch sensor.
        /// </summary>
        private const int CAP1208_ADDRESS = 0x28;

        #endregion

        #region Fields

        /// <summary>
        /// Provides access to the GPIO controller used for managing general-purpose input/output operations.
        /// </summary>
        private readonly GpioController _gpioController;

        /// <summary>
        /// Provides access to the I2C device for analog-to-digital conversion.
        /// </summary>
        private readonly I2cDevice? _adcDevice;

        /// <summary>
        /// Provides access to the I2C device for capacitive touch sensing.
        /// </summary>
        private readonly I2cDevice? _capTouchDevice;

        /// <summary>
        /// Determines whether the object has been disposed.
        /// </summary>
        private bool _disposed;

        #endregion

        #region Pin Mappings

        /// <summary>
        /// GPIO pin numbers for digital inputs (BCM numbering).
        /// </summary>
        private static readonly int[] InputPins = { 23, 22, 24, 25 };

        /// <summary>
        /// GPIO pin numbers for digital outputs (BCM numbering).
        /// </summary>
        private static readonly int[] OutputPins = { 6, 12, 13, 16 };

        /// <summary>
        /// GPIO pin numbers for onboard LEDs (BCM numbering).
        /// </summary>
        private static readonly int[] LedPins = { 4, 17, 27, 5 };

        /// <summary>
        /// GPIO pin numbers for motor control.
        /// DRV8833PWP H-Bridge uses only forward/backward pins (no separate enable).
        /// </summary>
        private static readonly MotorPinMapping Motor1Pins = new(20, 19); // M1F=20, M1B=19
        private static readonly MotorPinMapping Motor2Pins = new(26, 21); // M2F=26, M2B=21

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of digital inputs.
        /// </summary>
        public DigitalInputCollection Input { get; }

        /// <summary>
        /// Gets the collection of digital outputs.
        /// </summary>
        public DigitalOutputCollection Output { get; }

        /// <summary>
        /// Gets the collection of analog inputs.
        /// </summary>
        public AnalogInputCollection Analog { get; }

        /// <summary>
        /// Gets the collection of motors.
        /// </summary>
        public MotorCollection Motor { get; }

        /// <summary>
        /// Gets the collection of capacitive touch sensors.
        /// </summary>
        public TouchCollection Touch { get; }

        /// <summary>
        /// Gets the collection of onboard LEDs.
        /// </summary>
        public LedCollection Light { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerHat"/> class.
        /// </summary>
        /// <param name="gpioController">Optional GPIO controller. If null, a new controller will be created.</param>
        /// <param name="i2cBusId">The I2C bus ID to use. Default is 1.</param>
        public ExplorerHat(GpioController? gpioController = null, int i2cBusId = 1)
        {
            _gpioController = gpioController ?? new GpioController();

            // Initialize I2C devices
            try
            {
                var adcSettings = new I2cConnectionSettings(i2cBusId, ADS1015_ADDRESS);
                _adcDevice = I2cDevice.Create(adcSettings);
            }
            catch
            {
                _adcDevice = null;
            }

            try
            {
                var capTouchSettings = new I2cConnectionSettings(i2cBusId, CAP1208_ADDRESS);
                _capTouchDevice = I2cDevice.Create(capTouchSettings);
                InitializeCapTouch();
            }
            catch
            {
                _capTouchDevice = null;
            }

            // Initialize collections
            Input = new DigitalInputCollection(_gpioController, InputPins);
            Output = new DigitalOutputCollection(_gpioController, OutputPins);
            Analog = new AnalogInputCollection(_adcDevice);
            Motor = new MotorCollection(_gpioController, Motor1Pins, Motor2Pins);
            Touch = new TouchCollection(_capTouchDevice);
            Light = new LedCollection(_gpioController, LedPins);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the CAP1208 capacitive touch sensor.
        /// </summary>
        private void InitializeCapTouch()
        {
            if (_capTouchDevice == null) return;

            // Enable all 8 touch inputs
            Span<byte> buffer1 = stackalloc byte[2];
            buffer1[0] = 0x21;
            buffer1[1] = 0xFF;
            _capTouchDevice.Write(buffer1);
            
            // Set sensitivity
            Span<byte> buffer2 = stackalloc byte[2];
            buffer2[0] = 0x1F;
            buffer2[1] = 0x2F;
            _capTouchDevice.Write(buffer2);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the <see cref="ExplorerHat"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Input?.Dispose();
            Output?.Dispose();
            Motor?.Dispose();
            Light?.Dispose();
            _adcDevice?.Dispose();
            _capTouchDevice?.Dispose();
            _gpioController?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
