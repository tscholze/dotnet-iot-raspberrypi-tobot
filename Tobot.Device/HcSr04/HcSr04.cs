using System.Device.Gpio;
using Iot.Device.Hcsr04;
using UnitsNet;

namespace Tobot.Device.HcSr04;

/// <summary>
/// Represents the detected direction of an object relative to the sensor's center position.
/// </summary>
public enum ObjectDirection
{
	/// <summary>
	/// Object is centered or within the neutral zone.
	/// </summary>
	Center,

	/// <summary>
	/// Object is to the left of center.
	/// </summary>
	Left,

	/// <summary>
	/// Object is to the right of center.
	/// </summary>
	Right
}

/// <summary>
/// Represents a detected object with distance and angular information.
/// </summary>
public class DetectedObject
{
	/// <summary>
	/// Gets the measured distance to the object in centimeters.
	/// </summary>
	public double Distance { get; set; }

	/// <summary>
	/// Gets the pan angle at which the object was detected, in degrees (-90 to +90).
	/// </summary>
	public int PanAngle { get; set; }

	/// <summary>
	/// Gets the direction of the object relative to center (left, center, or right).
	/// Objects within ±5° of center are classified as centered.
	/// </summary>
	public ObjectDirection Direction
	{
		get
		{
			if (Math.Abs(PanAngle) <= 5)
			{
				return ObjectDirection.Center;
			}

			return PanAngle < 0 ? ObjectDirection.Left : ObjectDirection.Right;
		}
	}
}

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
	/// Performs an autonomous pan sweep to detect the closest object within the field of view.
	/// </summary>
	/// <param name="panTilt">The Pan-Tilt HAT instance to control the sensor's direction.</param>
	/// <param name="samples">Number of samples to average per pan angle. Default is 5.</param>
	/// <param name="sweepIncrement">Degrees to increment between pan measurements. Default is 5° (smaller = finer resolution).</param>
	/// <returns>
	/// A <see cref="DetectedObject"/> containing the distance and direction of the closest object,
	/// or <c>null</c> if no object is detected during the sweep.
	/// </returns>
	/// <remarks>
	/// This method sweeps from -45° (left) to +45° (right) by default. The sensor dwells for 200ms
	/// at each pan angle to allow the servo to settle before taking a measurement. Objects are detected
	/// left, center, or right based on the pan angle where the closest distance is found.
	/// </remarks>
	public DetectedObject? FindClosestObject(PanTiltHat.PanTiltHat panTilt, int samples = DefaultSamplesPerReading, int sweepIncrement = 5)
	{
		EnsureNotDisposed();

		if (panTilt == null)
		{
			throw new ArgumentNullException(nameof(panTilt));
		}

		if (sweepIncrement < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(sweepIncrement), "Sweep increment must be at least 1°.");
		}

		DetectedObject? closest = null;
		const int LeftBound = -45;
		const int RightBound = 45;
		const int ServoSettleMs = 200;

		// Sweep from left to right
		for (int angle = LeftBound; angle <= RightBound; angle += sweepIncrement)
		{
			panTilt.Pan(angle);
			Thread.Sleep(ServoSettleMs);

			if (TryReadDistance(out double distance, samples))
			{
				if (closest == null || distance < closest.Distance)
				{
					closest = new DetectedObject
					{
						Distance = distance,
						PanAngle = angle
					};
				}
			}
		}

		return closest;
	}

	/// <summary>
	/// Determines the direction of a detected object based on the current pan angle.
	/// This is a lightweight alternative to <see cref="FindClosestObject"/> when the pan angle
	/// is already being controlled externally (e.g., by user input or other navigation logic).
	/// </summary>
	/// <param name="panAngleDegrees">Current pan angle in degrees (-90 to +90). Values at center (±5°) are classified as centered.</param>
	/// <returns>The direction classification of the object (Left, Center, or Right).</returns>
	/// <remarks>
	/// This method only performs mathematical classification without any sensor movement or measurement.
	/// Objects within ±5° of center are considered centered; anything beyond that is left or right.
	/// Typical usage: Pan the sensor externally, take a distance reading, then call this method
	/// to classify the direction based on the pan angle where the object was detected.
	/// </remarks>
	public ObjectDirection GetObjectDirection(int panAngleDegrees)
	{
		EnsureNotDisposed();

		if (Math.Abs(panAngleDegrees) <= 5)
		{
			return ObjectDirection.Center;
		}

		return panAngleDegrees < 0 ? ObjectDirection.Left : ObjectDirection.Right;
	}

	/// <summary>
	/// Reads the distance at the current pan angle and classifies the object direction.
	/// This combines distance reading with direction classification in a single method call.
	/// </summary>
	/// <param name="panAngleDegrees">Current pan angle in degrees (-90 to +90).</param>
	/// <param name="distanceCm">Measured distance in centimeters when the method succeeds.</param>
	/// <param name="direction">Direction classification of the object at the current pan angle.</param>
	/// <param name="samples">Number of consecutive measurements to average. Must be greater than zero.</param>
	/// <returns><c>true</c> when at least one measurement succeeded; otherwise <c>false</c>.</returns>
	public bool TryReadDistanceWithDirection(int panAngleDegrees, out double distanceCm, out ObjectDirection direction, int samples = DefaultSamplesPerReading)
	{
		EnsureNotDisposed();

		direction = GetObjectDirection(panAngleDegrees);
		return TryReadDistance(out distanceCm, samples);
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

