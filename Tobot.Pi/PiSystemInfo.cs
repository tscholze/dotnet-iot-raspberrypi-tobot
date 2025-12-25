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
	/// Raised when the CPU temperature changes by at least the configured threshold during publishing.
	/// Payload is the rounded temperature in Celsius.
	/// </summary>
	public static event EventHandler<int>? TemperatureChanged;

	/// <summary>
	/// Gets the current host name of the device.
	/// </summary>
	public static string GetHostName() => Dns.GetHostName();

	/// <summary>
	/// Gets all non-loopback IP addresses for the device.
	/// </summary>
	/// <param name="includeIPv6">When true, includes IPv6 addresses; otherwise IPv4 only.</param>
	public static IReadOnlyList<IPAddress> GetIpAddresses(bool includeIPv6 = false)
	{
		var results = new List<IPAddress>();

		foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
		{
			if (ni.OperationalStatus != OperationalStatus.Up)
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

		_temperatureTimer = new Timer(_ => PublishTemperature(changeThresholdC), state: null, dueTime: pollInterval, period: pollInterval);
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
	/// Polls the CPU temperature and raises <see cref="TemperatureChanged"/> when the change exceeds the threshold.
	/// </summary>
	/// <param name="changeThresholdC">Minimum temperature delta required to publish.</param>
	private static void PublishTemperature(double changeThresholdC)
	{
		// Read current temperature in Celsius; bail if unavailable.
		double? current = GetCpuTemperatureCelsius();
		if (!current.HasValue)
		{
			return;
		}

		int rounded = (int)Math.Round(current.Value);

		if (_lastPublishedTemperature.HasValue && Math.Abs(current.Value - _lastPublishedTemperature.Value) < changeThresholdC)
		{
			return;
		}

		_lastPublishedTemperature = current.Value;

		TemperatureChanged?.Invoke(null, rounded);
	}
}
