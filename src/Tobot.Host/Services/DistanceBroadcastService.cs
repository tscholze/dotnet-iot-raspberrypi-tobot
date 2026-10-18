using Microsoft.AspNetCore.SignalR;
using Tobot.Device;
using Tobot.Host.Hubs;
using Tobot.Shared;

namespace Tobot.Host.Services;

/// <summary>
/// Hosted service that subscribes to <see cref="TobotController.ObserveDistance"/>
/// and pushes distance changes to all SignalR clients.
/// </summary>
/// <param name="controller">The shared hardware controller.</param>
/// <param name="hubContext">Context used to reach connected telemetry clients.</param>
public sealed class DistanceBroadcastService(TobotController controller, IHubContext<TelemetryHub> hubContext)
	: IHostedService, IDisposable
{
	#region Private Fields

	/// <summary>
	/// Active Rx subscription, if any.
	/// </summary>
	private IDisposable? _subscription;

	#endregion

	#region IHostedService

	/// <inheritdoc />
	public Task StartAsync(CancellationToken cancellationToken)
	{
		_subscription = controller
			.ObserveDistance()
			.Subscribe(async distanceCm =>
			{
				try
				{
					await hubContext.Clients.All.SendAsync(TobotHubEvents.DistanceChanged, distanceCm);
				}
				catch
				{
					// A failed broadcast must not terminate the subscription.
				}
			});

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task StopAsync(CancellationToken cancellationToken)
	{
		Dispose();
		return Task.CompletedTask;
	}

	#endregion

	#region IDisposable

	/// <inheritdoc />
	public void Dispose()
	{
		_subscription?.Dispose();
		_subscription = null;
	}

	#endregion
}
