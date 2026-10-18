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

	/// <summary>
	/// Whether this instance owns <see cref="_http"/> and must dispose it.
	/// </summary>
	private readonly bool _ownsHttp;

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TobotApiClient"/> class.
	/// </summary>
	/// <param name="baseAddress">Base address of the host, e.g. <c>http://pi41:5247</c>.</param>
	public TobotApiClient(Uri baseAddress)
	{
		_http = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(10) };
		_ownsHttp = true;
	}

	/// <summary>
	/// Initializes a new instance using an externally managed <see cref="HttpClient"/> (e.g. from DI or tests).
	/// </summary>
	/// <param name="httpClient">A client with <see cref="HttpClient.BaseAddress"/> set.</param>
	public TobotApiClient(HttpClient httpClient)
	{
		_http = httpClient;
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

	/// <inheritdoc />
	public async Task<bool> DriveAsync(DriveCommand command, CancellationToken cancellationToken = default)
	{
		using var response = await _http.PostAsJsonAsync(TobotRoutes.Drive, command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode;
	}

	/// <inheritdoc />
	public async Task<bool> StopAsync(CancellationToken cancellationToken = default)
	{
		using var response = await _http.PostAsync(TobotRoutes.DriveStop, null, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode;
	}

	#endregion

	#region IDisposable

	/// <inheritdoc />
	public void Dispose()
	{
		if (_ownsHttp)
		{
			_http.Dispose();
		}
	}

	#endregion
}
