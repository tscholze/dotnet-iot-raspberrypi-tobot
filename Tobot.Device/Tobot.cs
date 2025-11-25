using System;
using System.Device.Gpio;
using Tobot.Device.ExplorerHat.Motor;
using Tobot.Device.HcSr04;
using Tobot.Device.PanTiltHat;
using ExplorerHatDevice = Tobot.Device.ExplorerHat.ExplorerHat;
using PanTiltHatDevice = Tobot.Device.PanTiltHat.PanTiltHat;

namespace Tobot.Device;

/// <summary>
/// Central orchestrator that wires up all Tobot peripherals (Explorer HAT, Pan-Tilt HAT, HC-SR04).
/// Provides shared GPIO access, lazy component initialization, and convenience helpers for higher-level apps.
/// </summary>
public sealed class TobotController : IDisposable
{
	/// <summary>
	/// Lazy-initialized GPIO controller shared by every attached peripheral.
	/// </summary>
	private readonly Lazy<GpioController> _gpioController = new(() => new GpioController());

	/// <summary>
	/// Cached Explorer HAT instance (created on first use).
	/// </summary>
	private ExplorerHatDevice? _explorerHat;

	/// <summary>
	/// Cached Pan-Tilt HAT instance (created on first use).
	/// </summary>
	private PanTiltHatDevice? _panTiltHat;

	/// <summary>
	/// Cached HC-SR04 distance sensor (created on first use).
	/// </summary>
	private HcSr04Sensor? _distanceSensor;

	/// <summary>
	/// Tracks whether the controller has been disposed.
	/// </summary>
	private bool _disposed;

	/// <summary>
	/// Gets the shared <see cref="GpioController"/> used across all peripherals.
	/// </summary>
	public GpioController GpioController
	{
		get
		{
			EnsureNotDisposed();
			return _gpioController.Value;
		}
	}

	/// <summary>
	/// Gets the Explorer HAT abstraction, creating it on first use.
	/// </summary>
	public ExplorerHatDevice ExplorerHat
	{
		get
		{
			EnsureNotDisposed();
			return _explorerHat ??= new ExplorerHatDevice();
		}
	}

	/// <summary>
	/// Gets the Pan-Tilt HAT abstraction, creating it on first use.
	/// </summary>
	public PanTiltHatDevice PanTiltHat
	{
		get
		{
			EnsureNotDisposed();
			return _panTiltHat ??= new PanTiltHatDevice();
		}
	}

	/// <summary>
	/// Gets the HC-SR04 distance sensor abstraction, creating it on first use.
	/// </summary>
	public HcSr04Sensor UltrasonicSensor
	{
		get
		{
			EnsureNotDisposed();
			return _distanceSensor ??= new HcSr04Sensor(GpioController);
		}
	}

	/// <summary>
	/// Sets the specified LED on the Explorer HAT to the requested state.
	/// </summary>
	/// <param name="ledIndex">LED index (1-4).</param>
	/// <param name="isOn">Flag indicating whether the LED should be lit.</param>
	public void SetLedState(int ledIndex, bool isOn)
	{
		EnsureNotDisposed();
		ValidateChannel(ledIndex);
		if (isOn)
		{
			ExplorerHat.Light[ledIndex].On();
		}
		else
		{
			ExplorerHat.Light[ledIndex].Off();
		}
	}

	/// <summary>
	/// Sets all Explorer HAT LEDs to a single state.
	/// </summary>
	/// <param name="isOn">Flag indicating whether every LED should be lit.</param>
	public void SetAllLeds(bool isOn)
	{
		EnsureNotDisposed();
		if (isOn)
		{
			ExplorerHat.Light.On();
		}
		else
		{
			ExplorerHat.Light.Off();
		}
	}

	/// <summary>
	/// Drives a digital output line on the Explorer HAT.
	/// </summary>
	/// <param name="outputIndex">Output index (1-4).</param>
	/// <param name="isOn">Flag indicating whether the output should be high.</param>
	public void SetDigitalOutput(int outputIndex, bool isOn)
	{
		EnsureNotDisposed();
		ValidateChannel(outputIndex);
		if (isOn)
		{
			ExplorerHat.Output[outputIndex].On();
		}
		else
		{
			ExplorerHat.Output[outputIndex].Off();
		}
	}

	/// <summary>
	/// Reads the current state of all Explorer HAT digital inputs.
	/// </summary>
	/// <returns>Array of four booleans representing inputs 1-4.</returns>
	public bool[] ReadDigitalInputs()
	{
		EnsureNotDisposed();
		return new[]
		{
			ExplorerHat.Input.One.Read(),
			ExplorerHat.Input.Two.Read(),
			ExplorerHat.Input.Three.Read(),
			ExplorerHat.Input.Four.Read()
		};
	}

	/// <summary>
	/// Reads the latest analog values from the Explorer HAT ADC.
	/// </summary>
	/// <returns>Array of four voltage readings.</returns>
	public double[] ReadAnalogValues()
	{
		EnsureNotDisposed();
		return new[]
		{
			ExplorerHat.Analog.One.Read(),
			ExplorerHat.Analog.Two.Read(),
			ExplorerHat.Analog.Three.Read(),
			ExplorerHat.Analog.Four.Read()
		};
	}

	/// <summary>
	/// Drives both Explorer HAT motors using percentage-based speed commands.
	/// </summary>
	/// <param name="motorOnePercent">Speed for motor one (-100 to 100).</param>
	/// <param name="motorTwoPercent">Speed for motor two (-100 to 100).</param>
	public void DriveMotors(int motorOnePercent, int motorTwoPercent)
	{
		EnsureNotDisposed();
		ApplyMotorSpeed(ExplorerHat.Motor.One, motorOnePercent);
		ApplyMotorSpeed(ExplorerHat.Motor.Two, motorTwoPercent);
	}

	/// <summary>
	/// Stops both Explorer HAT motors.
	/// </summary>
	public void StopMotors()
	{
		EnsureNotDisposed();
		ExplorerHat.Motor.Stop();
	}

	/// <summary>
	/// Moves the Pan-Tilt HAT to the desired pan/tilt angles.
	/// </summary>
	/// <param name="panDegrees">Pan angle in degrees (-90 to 90 typical).</param>
	/// <param name="tiltDegrees">Tilt angle in degrees (-45 to 45 typical).</param>
	public void PanTilt(double panDegrees, double tiltDegrees)
	{
		EnsureNotDisposed();
		PanTiltHat.Pan((int)Math.Round(panDegrees));
		PanTiltHat.Tilt((int)Math.Round(tiltDegrees));
	}

	/// <summary>
	/// Gets the current pan and tilt angles as reported by the servo controller.
	/// </summary>
	/// <returns>Tuple containing the current pan and tilt angles.</returns>
	public (double Pan, double Tilt) GetPanTiltAngles()
	{
		EnsureNotDisposed();
		return (PanTiltHat.GetPan(), PanTiltHat.GetTilt());
	}

	/// <summary>
	/// Attempts to read the HC-SR04 distance sensor and returns the latest value in centimeters.
	/// </summary>
	/// <param name="distanceCm">Distance output in centimeters.</param>
	/// <param name="samples">Number of samples to average.</param>
	/// <returns>True when a measurement succeeds; otherwise false.</returns>
	public bool TryReadDistance(out double distanceCm, int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		EnsureNotDisposed();
		return UltrasonicSensor.TryReadDistance(out distanceCm, samples);
	}

	/// <summary>
	/// Reads the HC-SR04 distance sensor and throws if no measurement succeeds.
	/// </summary>
	/// <param name="samples">Number of samples to average.</param>
	/// <returns>Measured distance in centimeters.</returns>
	public double ReadDistance(int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		EnsureNotDisposed();
		return UltrasonicSensor.ReadDistance(samples);
	}

	/// <summary>
	/// Releases all managed resources associated with the controller and its peripherals.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_distanceSensor?.Dispose();
		_panTiltHat?.Dispose();
		_explorerHat?.Dispose();
		if (_gpioController.IsValueCreated)
		{
			_gpioController.Value.Dispose();
		}

		_disposed = true;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Applies a signed speed value to the provided Explorer HAT motor.
	/// </summary>
	/// <param name="motor">Motor to drive.</param>
	/// <param name="speedPercent">Requested speed between -100 (full reverse) and 100 (full forward).</param>
	private static void ApplyMotorSpeed(Motor motor, int speedPercent)
	{
		int clamped = Math.Clamp(speedPercent, -100, 100);
		if (clamped > 0)
		{
			motor.Forward(clamped);
		}
		else if (clamped < 0)
		{
			motor.Backward(Math.Abs(clamped));
		}
		else
		{
			motor.Stop();
		}
	}

	/// <summary>
	/// Ensures the controller has not been disposed before servicing requests.
	/// </summary>
	private void EnsureNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(TobotController));
		}
	}

	/// <summary>
	/// Validates that a 1-based Explorer HAT channel index is in range.
	/// </summary>
	/// <param name="channel">Channel index to validate.</param>
	private static void ValidateChannel(int channel)
	{
		if (channel is < 1 or > 4)
		{
			throw new ArgumentOutOfRangeException(nameof(channel), "Explorer HAT exposes four channels (1-4).");
		}
	}
}
