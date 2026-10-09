using System;
using System.Device.Gpio;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tobot.Device.HcSr04;

namespace Tobot.Device;

/// <summary>
/// Central orchestrator for the HC-SR04 distance sensor.
/// Provides shared GPIO access and a reactive distance observable.
/// </summary>
public sealed class TobotController : IDisposable
{
	#region Private Fields

	/// <summary>
	/// Lazy-initialized GPIO controller shared by attached peripherals.
	/// </summary>
	private readonly Lazy<GpioController> _gpioController = new(() => new GpioController());

	/// <summary>
	/// Cached HC-SR04 distance sensor (created on first use).
	/// </summary>
	private HcSr04Sensor? _distanceSensor;

	/// <summary>
	/// Subject for broadcasting distance changes.
	/// </summary>
	private readonly Subject<double> _distanceSubject = new();

	/// <summary>
	/// Subscription for distance monitoring timer.
	/// </summary>
	private IDisposable? _distanceMonitoringSubscription;

	/// <summary>
	/// Last measured distance value for change detection.
	/// </summary>
	private double? _lastDistance;

	/// <summary>
	/// Tracks whether the controller has been disposed.
	/// </summary>
	private bool _disposed;

	#endregion

	#region Public Properties

	/// <summary>
	/// Gets the shared <see cref="GpioController"/> used across attached peripherals.
	/// </summary>
	public GpioController GpioController
	{
		get
		{
			EnsureNotDisposed();
			return _gpioController.Value;
		}
	}

	#endregion

	#region Distance Measurement

	/// <summary>
	/// Gets the HC-SR04 distance sensor abstraction, creating it on first use.
	/// </summary>
	private HcSr04Sensor UltrasonicSensor
	{
		get
		{
			EnsureNotDisposed();
			return _distanceSensor ??= new HcSr04Sensor(GpioController);
		}
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
	/// Gets an observable sequence that emits distance measurements when they change by more than the specified threshold.
	/// The sensor is polled every 500ms and changes greater than 1cm trigger a notification.
	/// </summary>
	/// <param name="thresholdCm">Minimum change in centimeters to trigger notification (default 1.0cm).</param>
	/// <param name="samples">Number of samples to average per reading.</param>
	/// <returns>Observable sequence of distance measurements in centimeters.</returns>
	public IObservable<double> ObserveDistance(double thresholdCm = 1.0, int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		EnsureNotDisposed();

		if (thresholdCm <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(thresholdCm), "Threshold must be greater than zero.");
		}

		if (_distanceMonitoringSubscription == null)
		{
			StartDistanceMonitoring(thresholdCm, samples);
		}

		return _distanceSubject.AsObservable();
	}

	/// <summary>
	/// Starts the background distance monitoring timer.
	/// </summary>
	/// <param name="thresholdCm">Minimum change threshold.</param>
	/// <param name="samples">Number of samples per reading.</param>
	private void StartDistanceMonitoring(double thresholdCm, int samples)
	{
		_distanceMonitoringSubscription = Observable
			.Interval(TimeSpan.FromMilliseconds(500))
			.Subscribe(_ =>
			{
				if (_disposed)
				{
					return;
				}

				try
				{
					if (TryReadDistance(out double currentDistance, samples)
						&& (_lastDistance == null || Math.Abs(currentDistance - _lastDistance.Value) >= thresholdCm))
					{
						_lastDistance = currentDistance;
						_distanceSubject.OnNext(currentDistance);
					}
				}
				catch (Exception ex)
				{
					_distanceSubject.OnError(ex);
				}
			});
	}

	#endregion

	#region Disposal

	/// <summary>
	/// Releases all managed resources associated with the controller and its sensor.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_distanceMonitoringSubscription?.Dispose();
		_distanceSubject.OnCompleted();
		_distanceSubject.Dispose();
		_distanceSensor?.Dispose();
		if (_gpioController.IsValueCreated)
		{
			_gpioController.Value.Dispose();
		}

		_disposed = true;
		GC.SuppressFinalize(this);
	}

	#endregion

	#region Private Helpers

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

	#endregion
}
