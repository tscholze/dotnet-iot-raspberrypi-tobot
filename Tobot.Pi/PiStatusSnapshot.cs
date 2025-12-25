using System;
using System.Collections.Generic;
using System.Net;

namespace Tobot.Pi;

/// <summary>
/// Immutable snapshot of Raspberry Pi system status captured at a point in time.
/// </summary>
public sealed class PiStatusSnapshot
{
	/// <summary>
	/// Device host name, e.g., "raspberrypi".
	/// </summary>
	public string Hostname { get; init; } = string.Empty;

	/// <summary>
	/// Wi-Fi IP address, e.g., 192.168.1.42.
	/// </summary>
	public IPAddress? IpAddresses { get; init; }

	/// <summary>
	/// Connected Wi-Fi network name (SSID), e.g., "MyNetwork".
	/// </summary>
	public string? WifiSsid { get; init; }

	/// <summary>
	/// 1-minute load average, e.g., 0.42.
	/// </summary>
	public double? LoadAvg1Minute { get; init; }

	/// <summary>
	/// 5-minute load average, e.g., 0.35.
	/// </summary>
	public double? LoadAvg5Minutes { get; init; }

	/// <summary>
	/// 15-minute load average, e.g., 0.28.
	/// </summary>
	public double? LoadAvg15Minutes { get; init; }

	/// <summary>
	/// Total memory in kB, e.g., 918536.
	/// </summary>
	public long? MemTotalKb { get; init; }

	/// <summary>
	/// Available memory in kB, e.g., 512000.
	/// </summary>
	public long? MemAvailableKb { get; init; }

	/// <summary>
	/// Total disk size in GiB for the root mount, e.g., 29.8.
	/// </summary>
	public double? DiskTotalGiB { get; init; }

	/// <summary>
	/// Free disk space in GiB for the root mount, e.g., 11.2.
	/// </summary>
	public double? DiskFreeGiB { get; init; }

	/// <summary>
	/// System uptime in seconds, e.g., 12345.6.
	/// </summary>
	public double? UptimeSeconds { get; init; }

	/// <summary>
	/// CPU temperature in Celsius (rounded), e.g., 54.
	/// </summary>
	public int? CpuTempC { get; init; }

	/// <summary>
	/// CPU frequency in MHz, e.g., 1200.
	/// </summary>
	public int? CpuFreqMHz { get; init; }
}
