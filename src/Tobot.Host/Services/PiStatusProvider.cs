using Tobot.Device.Pi;
using Tobot.Shared.Models;

namespace Tobot.Host.Services;

/// <summary>
/// Builds a <see cref="PiStatusSummary"/> from <see cref="PiSystemInfo"/>.
/// </summary>
public static class PiStatusProvider
{
	/// <summary>
	/// Reads the current Wi-Fi, CPU and memory status of the Pi.
	/// </summary>
	/// <returns>The current status summary.</returns>
	public static PiStatusSummary GetSummary()
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
		if (totalKb is > 0 && availKb.HasValue)
		{
			memLoadPercent = Math.Clamp((totalKb.Value - availKb.Value) / (double)totalKb.Value * 100.0, 0.0, 100.0);
		}

		return new PiStatusSummary(ssid, ip, cpuLoadPercent, memLoadPercent);
	}
}
