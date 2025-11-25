using System.Device.Gpio;
using Iot.Device.Hcsr04;
using UnitsNet;

namespace Tobot.Device.HcSr04;

/// <summary>
/// High-level manager for HC-SR04 ultrasonic distance sensors.
/// Wraps the <see cref="Iot.Device.Hcsr04.Hcsr04"/> driver and exposes
/// a simplified API for measuring distances in centimeters.
/// </summary>
public sealed class HcSr04Sensor : IDisposable
{
	/// <summary>
	/// Default BCM pin driving the sensor's trigger input.
	/// </summary>
	public const int DefaultTriggerPin = 12;

	/// <summary>
	/// Default BCM pin connected to the sensor's echo output.
	/// </summary>
	public const int DefaultEchoPin = 22;

	/// <summary>
	/// Default number of samples to average per reading.
	/// </summary>
	public const int DefaultSamplesPerReading = 5;

	/// <summary>
	/// Underlying HC-SR04 driver from <c>Iot.Device.Bindings</c>.
	/// </summary>
	private readonly Hcsr04 _sensor;

	/// <summary>
	/// Tracks whether this instance has been disposed.
	/// </summary>
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="HcSr04Sensor"/> class using the logical pin numbering scheme.
	/// </summary>
	/// <param name="controller">
	/// Optional <see cref="GpioController"/> to use for pin access. When omitted, a controller is created and disposed with the sensor.
	/// </param>
	public HcSr04Sensor(GpioController? controller = null)
	{
		_sensor = controller is null
			? new Hcsr04(DefaultTriggerPin, DefaultEchoPin)
			: new Hcsr04(controller, DefaultTriggerPin, DefaultEchoPin, shouldDispose: false);
	}

	/// <summary>
	/// Attempts to read the current distance and returns the result in centimeters.
	/// </summary>
	/// <param name="distanceCm">Measured distance in centimeters when the method succeeds.</param>
	/// <param name="samples">Number of consecutive measurements to average. Must be greater than zero.</param>
	/// <returns><c>true</c> when at least one measurement succeeded; otherwise <c>false</c>.</returns>
	public bool TryReadDistance(out double distanceCm, int samples = DefaultSamplesPerReading)
	{
		EnsureNotDisposed();

		if (samples < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(samples), "Samples must be at least 1.");
		}

		double sum = 0;
		int validSamples = 0;

		for (int i = 0; i < samples; i++)
		{
			if (_sensor.TryGetDistance(out Length measurement))
			{
				sum += measurement.Centimeters;
				validSamples++;
			}
		}

		if (validSamples == 0)
		{
			distanceCm = 0;
			return false;
		}

		distanceCm = sum / validSamples;
		return true;
	}

	/// <summary>
	/// Reads the current distance in centimeters, averaging the specified number of samples, and throws if no measurement succeeds.
	/// </summary>
	/// <param name="samples">Number of consecutive measurements to average. Must be greater than zero.</param>
	/// <returns>Average distance in centimeters.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the sensor fails to provide any measurement.</exception>
	public double ReadDistance(int samples = DefaultSamplesPerReading)
	{
		if (!TryReadDistance(out double distance, samples))
		{
			throw new InvalidOperationException("Unable to read distance from HC-SR04 sensor.");
		}

		return distance;
	}

	/// <summary>
	/// Releases the managed resources associated with the sensor.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_sensor.Dispose();
		_disposed = true;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Throws an <see cref="ObjectDisposedException"/> when the sensor has been disposed.
	/// </summary>
	/// <exception cref="ObjectDisposedException">Thrown when called after disposal.</exception>
	private void EnsureNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(HcSr04Sensor));
		}
	}
}

