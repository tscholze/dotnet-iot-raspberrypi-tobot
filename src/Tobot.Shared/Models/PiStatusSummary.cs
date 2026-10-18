namespace Tobot.Shared.Models;

/// <summary>
/// Condensed Raspberry Pi status that is cheap to transmit to UI clients.
/// </summary>
/// <param name="WifiSsid">Connected Wi-Fi SSID, or <c>null</c> when unknown.</param>
/// <param name="WifiIp">Primary Wi-Fi IPv4 address, or <c>null</c> when unknown.</param>
/// <param name="CpuLoadPercent">CPU load (1-minute average relative to core count) in percent, 0..100.</param>
/// <param name="MemLoadPercent">Used memory in percent, 0..100.</param>
public sealed record PiStatusSummary(string? WifiSsid, string? WifiIp, double CpuLoadPercent, double MemLoadPercent);
