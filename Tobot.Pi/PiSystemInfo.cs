using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Tobot.Pi;

/// <summary>
/// Provides Raspberry Pi system information such as host name, IP addresses, and CPU temperature.
/// </summary>
public static class PiSystemInfo
{
	/// <summary>
	/// Periodic timer used to poll CPU temperature.
	/// </summary>
	private static Timer? _temperatureTimer;

	/// <summary>
	/// Most recently published raw temperature value in Celsius.
	/// </summary>
	private static double? _lastPublishedTemperature;

	/// <summary>
	/// Latest published status snapshot.
	/// </summary>
	private static PiStatusSnapshot? _lastStatus;

	/// <summary>
	/// Raised when the CPU temperature changes by at least the configured threshold during publishing.
	/// Payload is the rounded temperature in Celsius.
	/// </summary>
	public static event EventHandler<int>? TemperatureChanged;

	/// <summary>
	/// Raised on each poll with a full system status snapshot.
	/// </summary>
	public static event EventHandler<PiStatusSnapshot>? StatusChanged;

	/// <summary>
	/// Gets the current host name of the device.
	/// </summary>
	public static string GetHostName() => Dns.GetHostName();

	/// <summary>
	/// Gets the connected Wi-Fi network SSID.
	/// Returns null if not connected to Wi-Fi or unable to determine.
	/// </summary>
	public static string? GetWifiSsid()
	{
		// 1) Try "iwgetid -r" (most reliable on Raspberry Pi OS)
		string? ssid = TryRunCommand("iwgetid", "-r");
		if (!string.IsNullOrWhiteSpace(ssid))
		{
			return ssid.Trim();
		}

		// 2) Fallback: use "iw dev <iface> link" and parse the SSID line
		try
		{
			foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (ni.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}

				bool isWifi = ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.Name.Contains("wlan", StringComparison.OrdinalIgnoreCase);
				if (!isWifi)
				{
					continue;
				}

				string? linkInfo = TryRunCommand("iw", $"dev {ni.Name} link");
				if (!string.IsNullOrWhiteSpace(linkInfo))
				{
					foreach (string line in linkInfo.Split('\n'))
					{
						string trimmed = line.Trim();
						if (trimmed.StartsWith("SSID ", StringComparison.OrdinalIgnoreCase))
						{
							string candidate = trimmed.Substring(5).Trim();
							if (!string.IsNullOrEmpty(candidate))
							{
								return candidate;
							}
						}
					}
				}
			}
		}
		catch
		{
			// ignore
		}

		return null;
	}

	/// <summary>
	/// Runs a system command and returns trimmed stdout, or null on failure.
	/// </summary>
	private static string? TryRunCommand(string fileName, string arguments)
	{
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using var proc = Process.Start(psi);
			if (proc == null)
			{
				return null;
			}

			string output = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit(1000);
			return string.IsNullOrWhiteSpace(output) ? null : output.Trim();
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Gets non-loopback IP addresses for the device.
	/// </summary>
	/// <param name="includeIPv6">When true, includes IPv6 addresses; otherwise IPv4 only.</param>
	/// <param name="wifiOnly">When true, returns only addresses from wireless interfaces.</param>
	public static IReadOnlyList<IPAddress> GetIpAddresses(bool includeIPv6 = false, bool wifiOnly = false)
	{
		var results = new List<IPAddress>();

		foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
		{
			if (ni.OperationalStatus != OperationalStatus.Up)
			{
				continue;
			}

			bool isWifi = ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.Name.Contains("wlan", StringComparison.OrdinalIgnoreCase);
			if (wifiOnly && !isWifi)
			{
				continue;
			}

			var props = ni.GetIPProperties();
			foreach (UnicastIPAddressInformation addr in props.UnicastAddresses)
			{
				if (IPAddress.IsLoopback(addr.Address))
				{
					continue;
				}

				if (!includeIPv6 && addr.Address.AddressFamily == AddressFamily.InterNetworkV6)
				{
					continue;
				}

				results.Add(addr.Address);
			}
		}

		return results;
	}

	/// <summary>
	/// Attempts to read the CPU temperature in Celsius from the standard Linux thermal zone path.
	/// Returns null when the value cannot be read or parsed.
	/// </summary>
	public static double? GetCpuTemperatureCelsius()
	{
		try
		{
			const string thermalPath = "/sys/class/thermal/thermal_zone0/temp";
			if (!File.Exists(thermalPath))
			{
				return null;
			}

			string raw = File.ReadAllText(thermalPath).Trim();
			if (double.TryParse(raw, out double milliDegrees))
			{
				return milliDegrees / 1000.0;
			}
		}
		catch
		{
			// Intentionally swallow exceptions; caller can handle null.
		}

		return null;
	}

	/// <summary>
	/// Gets the CPU temperature rounded to the nearest whole number in Celsius.
	/// Returns null when unavailable.
	/// </summary>
	public static int? GetCpuTemperatureCelsiusRounded()
	{
		double? temp = GetCpuTemperatureCelsius();
		return temp.HasValue ? (int)Math.Round(temp.Value) : null;
	}

	/// <summary>
	/// Reads the 1/5/15 minute load averages from /proc/loadavg by index (0,1,2).
	/// Returns null when unavailable or parse fails.
	/// </summary>
	/// <param name="index">0 for 1-minute, 1 for 5-minute, 2 for 15-minute average.</param>
	public static double? ReadLoadAverage(int index)
	{
		try
		{
			string path = "/proc/loadavg";
			if (!File.Exists(path))
			{
				return null;
			}

			string content = File.ReadAllText(path).Trim();
			string[] parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (index < 0 || index >= 3 || parts.Length < 3)
			{
				return null;
			}

			if (double.TryParse(parts[index], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
			{
				return value;
			}
		}
		catch
		{
			// ignore
		}

		return null;
	}

	/// <summary>
	/// Reads a memory info value in kB from /proc/meminfo by key, e.g., MemTotal or MemAvailable.
	/// Returns null when unavailable or parse fails.
	/// </summary>
	public static long? ReadMemInfo(string key)
	{
		try
		{
			string path = "/proc/meminfo";
			if (!File.Exists(path))
			{
				return null;
			}

			foreach (string line in File.ReadLines(path))
			{
				if (!line.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length >= 2 && long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long kb))
				{
					return kb;
				}
			}
		}
		catch
		{
			// ignore
		}

		return null;
	}

	/// <summary>
	/// Reads system uptime in seconds from /proc/uptime.
	/// Returns null when unavailable or parse fails.
	/// </summary>
	public static double? ReadUptimeSeconds()
	{
		try
		{
			string path = "/proc/uptime";
			if (!File.Exists(path))
			{
				return null;
			}

			string content = File.ReadAllText(path).Trim();
			string[] parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
			{
				return null;
			}

			if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
			{
				return seconds;
			}
		}
		catch
		{
			// ignore
		}

		return null;
	}

	/// <summary>
	/// Reads current CPU frequency in MHz from scaling_cur_freq (cpu0). Returns null when unavailable.
	/// </summary>
	public static int? ReadCpuFreqMHz()
	{
		try
		{
			const string path = "/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq";
			if (!File.Exists(path))
			{
				return null;
			}

			string raw = File.ReadAllText(path).Trim();
			if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double khz))
			{
				return (int)Math.Round(khz / 1000.0);
			}
		}
		catch
		{
			// ignore
		}

		return null;
	}

	/// <summary>
	/// Reads total disk bytes for the specified mount point. Returns null on failure.
	/// </summary>
	public static long? ReadDiskTotalBytes(string mountPoint)
	{
		try
		{
			var drive = new DriveInfo(mountPoint);
			return drive.TotalSize;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Reads free disk bytes for the specified mount point. Returns null on failure.
	/// </summary>
	public static long? ReadDiskFreeBytes(string mountPoint)
	{
		try
		{
			var drive = new DriveInfo(mountPoint);
			return drive.AvailableFreeSpace;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Starts publishing CPU temperature every 5 seconds by default.
	/// An event is raised only when the temperature changes by at least the given threshold (default 2°C).
	/// </summary>
	/// <param name="interval">Optional poll interval; defaults to 5 seconds.</param>
	/// <param name="changeThresholdC">Minimum change in Celsius required to publish; defaults to 2°C.</param>
	public static void StartTemperaturePublishing(TimeSpan? interval = null, double changeThresholdC = 2.0)
	{
		TimeSpan pollInterval = interval ?? TimeSpan.FromSeconds(5);

		if (_temperatureTimer != null)
		{
			return; // already running
		}

		_temperatureTimer = new Timer(_ => PublishStatus(changeThresholdC), state: null, dueTime: pollInterval, period: pollInterval);
	}

	/// <summary>
	/// Stops publishing CPU temperature updates and clears the last published value.
	/// </summary>
	public static void StopTemperaturePublishing()
	{
		_temperatureTimer?.Dispose();
		_temperatureTimer = null;
		_lastPublishedTemperature = null;
	}

	/// <summary>
	/// Polls system status and raises events for temperature change (thresholded) and full status snapshots.
	/// </summary>
	/// <param name="tempChangeThresholdC">Minimum temperature delta required to publish TemperatureChanged.</param>
	private static void PublishStatus(double tempChangeThresholdC)
	{
		// Temperature
		double? currentTemp = GetCpuTemperatureCelsius();
		if (currentTemp.HasValue)
		{
			int rounded = (int)Math.Round(currentTemp.Value);

			if (!_lastPublishedTemperature.HasValue || Math.Abs(currentTemp.Value - _lastPublishedTemperature.Value) >= tempChangeThresholdC)
			{
				_lastPublishedTemperature = currentTemp.Value;
				TemperatureChanged?.Invoke(null, rounded);
			}
		}

		// Snapshot
		const double bytesPerGiB = 1024 * 1024 * 1024d;
		long? diskTotalBytes = ReadDiskTotalBytes("/");
		long? diskFreeBytes = ReadDiskFreeBytes("/");
		var wifiAddresses = GetIpAddresses(includeIPv6: false, wifiOnly: true);
		var status = new PiStatusSnapshot
		{
			Hostname = GetHostName(),
			IpAddresses = wifiAddresses.Count > 0 ? wifiAddresses[0] : null,
			WifiSsid = GetWifiSsid(),
			LoadAvg1Minute = ReadLoadAverage(0),
			LoadAvg5Minutes = ReadLoadAverage(1),
			LoadAvg15Minutes = ReadLoadAverage(2),
			MemTotalKb = ReadMemInfo("MemTotal"),
			MemAvailableKb = ReadMemInfo("MemAvailable"),
			DiskTotalGiB = diskTotalBytes.HasValue ? diskTotalBytes.Value / bytesPerGiB : null,
			DiskFreeGiB = diskFreeBytes.HasValue ? diskFreeBytes.Value / bytesPerGiB : null,
			UptimeSeconds = ReadUptimeSeconds(),
			CpuTempC = currentTemp.HasValue ? (int?)Math.Round(currentTemp.Value) : null,
			CpuFreqMHz = ReadCpuFreqMHz()
		};

		_lastStatus = status;
		StatusChanged?.Invoke(null, status);
	}
}
