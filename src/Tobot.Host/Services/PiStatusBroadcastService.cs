using Microsoft.AspNetCore.SignalR;
using Tobot.Host.Hubs;
using Tobot.Shared;

namespace Tobot.Host.Services;

/// <summary>
/// Background service that periodically pushes the Pi status to all SignalR clients.
/// </summary>
/// <param name="hubContext">Context used to reach connected telemetry clients.</param>
public sealed class PiStatusBroadcastService(IHubContext<TelemetryHub> hubContext) : BackgroundService
{
	/// <summary>
	/// Interval between two status broadcasts.
	/// </summary>
	private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(Interval);

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				try
				{
					await hubContext.Clients.All.SendAsync(
						TobotHubEvents.PiStatusUpdated,
						PiStatusProvider.GetSummary(),
						stoppingToken);
				}
				catch (Exception) when (!stoppingToken.IsCancellationRequested)
				{
					// Keep the loop alive on transient failures.
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Normal shutdown.
		}
	}
}
