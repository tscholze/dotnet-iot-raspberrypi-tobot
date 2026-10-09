using Microsoft.AspNetCore.SignalR;
using Tobot.Device.HcSr04;

namespace Tobot.Web.Hubs;

/// <summary>
/// SignalR hub for reading and monitoring the HC-SR04 distance sensor.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TobotHub"/> class.
/// </remarks>
/// <param name="controller">The TobotController instance.</param>
public class TobotHub(Device.TobotController controller) : Hub
{
	#region Private Fields

	/// <summary>
	/// The TobotController instance for distance measurements.
	/// </summary>
	private readonly Device.TobotController _controller = controller;

	/// <summary>
	/// Subscription for reactive distance monitoring.
	/// </summary>
	private IDisposable? _distanceSubscription;

	#endregion

	#region Distance

	/// <summary>
	/// Reads distance once; returns null if measurement fails.
	/// </summary>
	/// <param name="samples">Number of samples to average.</param>
	/// <returns>Measured distance in centimeters, or null if measurement fails.</returns>
	public double? ReadDistance(int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		return _controller.TryReadDistance(out double distanceCm, samples) ? distanceCm : null;
	}

	/// <summary>
	/// Starts reactive distance monitoring and broadcasts updates.
	/// </summary>
	/// <param name="thresholdCm">Minimum change in centimeters to trigger an update.</param>
	/// <param name="samples">Number of samples to average per reading.</param>
	public void StartDistanceMonitoring(double thresholdCm = 1.0, int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		_distanceSubscription?.Dispose();
		_distanceSubscription = _controller
			.ObserveDistance(thresholdCm, samples)
			.Subscribe(async distanceCm =>
				await Clients.All.SendAsync(TobotHubEvents.DistanceChanged, distanceCm));
	}

	/// <summary>
	/// Stops reactive distance monitoring for this connection.
	/// </summary>
	public void StopDistanceMonitoring()
	{
		_distanceSubscription?.Dispose();
		_distanceSubscription = null;
	}

	#endregion
}
