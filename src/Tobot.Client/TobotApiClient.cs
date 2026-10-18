using System.Net;
using System.Net.Http.Json;
using Tobot.Shared;
using Tobot.Shared.Models;

namespace Tobot.Client;

/// <summary>
/// <see cref="ITobotApi"/> implementation that talks to <c>Tobot.Host</c> over HTTP.
/// </summary>
public sealed class TobotApiClient : ITobotApi, IDisposable
{
	#region Private Fields

	/// <summary>
	/// Underlying HTTP client with the host base address.
	/// </summary>
	private readonly HttpClient _http;

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TobotApiClient"/> class.
	/// </summary>
	/// <param name="baseAddress">Base address of the host, e.g. <c>http://pi41:5247</c>.</param>
	public TobotApiClient(Uri baseAddress)
	{
		_http = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(10) };
	}

	#endregion

	#region ITobotApi

	/// <inheritdoc />
	public async Task<DistanceReading?> GetDistanceAsync(int? samples = null, CancellationToken cancellationToken = default)
	{
		string url = samples is null ? TobotRoutes.Distance : $"{TobotRoutes.Distance}?samples={samples}";
		using var response = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);

		if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
		{
			return null;
		}

		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<DistanceReading>(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public Task<PiStatusSummary?> GetStatusAsync(CancellationToken cancellationToken = default) =>
		_http.GetFromJsonAsync<PiStatusSummary>(TobotRoutes.Status, cancellationToken);

	#endregion

	#region IDisposable

	/// <inheritdoc />
	public void Dispose() => _http.Dispose();

	#endregion
}
