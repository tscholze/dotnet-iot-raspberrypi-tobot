using Microsoft.AspNetCore.SignalR;
using Tobot.Pi;
using Tobot.Web.Hubs;

namespace Tobot.Web.Services;

/// <summary>
/// Hosted service that periodically reads Raspberry Pi status and broadcasts it to clients.
/// </summary>
public sealed class PiStatusBroadcastService : IHostedService, IDisposable
{
    private readonly IHubContext<TobotHub> _hubContext;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public PiStatusBroadcastService(IHubContext<TobotHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = RunLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        if (_loopTask != null)
        {
            try { await _loopTask.ConfigureAwait(false); } catch { }
            _loopTask = null;
        }
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (!ct.IsCancellationRequested && await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                try
                {
                    string? ssid = PiSystemInfo.GetWifiSsid();
                    var wifiIps = PiSystemInfo.GetIpAddresses(includeIPv6: false, wifiOnly: true);
                    string? ip = wifiIps.Count > 0 ? wifiIps[0].ToString() : null;

                    double cpuLoadPercent = 0.0;
                    double? load1 = PiSystemInfo.ReadLoadAverage(0);
                    int cores = Environment.ProcessorCount;
                    if (load1.HasValue && cores > 0)
                    {
                        cpuLoadPercent = Math.Clamp(load1.Value / cores * 100.0, 0.0, 100.0);
                    }

                    double memLoadPercent = 0.0;
                    long? totalKb = PiSystemInfo.ReadMemInfo("MemTotal");
                    long? availKb = PiSystemInfo.ReadMemInfo("MemAvailable");
                    if (totalKb.HasValue && availKb.HasValue && totalKb.Value > 0)
                    {
                        double usedKb = totalKb.Value - availKb.Value;
                        memLoadPercent = Math.Clamp(usedKb / totalKb.Value * 100.0, 0.0, 100.0);
                    }

                    await _hubContext.Clients.All.SendAsync(
                        TobotHubEvents.PiStatusUpdated,
                        ssid,
                        ip,
                        cpuLoadPercent,
                        memLoadPercent,
                        ct);
                }
                catch
                {
                    // swallow to keep loop alive
                }
            }
        }
        catch
        {
            // cancellation or unexpected exit
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
