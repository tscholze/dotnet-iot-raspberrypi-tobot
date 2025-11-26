using Microsoft.AspNetCore.SignalR;
using Tobot.Device;
using Tobot.Web.Hubs;

namespace Tobot.Web.Services;

/// <summary>
/// Hosted service that subscribes to TobotController.ObserveDistance()
/// and broadcasts changes to all SignalR clients via the TobotHub.
/// </summary>
public sealed class DistanceBroadcastService : IHostedService, IDisposable
{
    private readonly TobotController _controller;
    private readonly IHubContext<TobotHub> _hubContext;
    private IDisposable? _subscription;

    public DistanceBroadcastService(TobotController controller, IHubContext<TobotHub> hubContext)
    {
        _controller = controller;
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Subscribe to reactive distance updates; threshold defaults to 1.0 cm and polling every 500 ms.
        _subscription = _controller
            .ObserveDistance()
            .Subscribe(async distanceCm =>
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync(TobotHubEvents.DistanceChanged, distanceCm, cancellationToken);
                }
                catch
                {
                    // Swallow broadcast exceptions to keep subscription alive.
                }
            },
            async ex =>
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("DistanceError", ex.Message, cancellationToken);
                }
                catch
                {
                }
            });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();
        _subscription = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
