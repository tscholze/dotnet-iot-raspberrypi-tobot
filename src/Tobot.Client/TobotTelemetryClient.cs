using Microsoft.AspNetCore.SignalR.Client;
using Tobot.Shared;
using Tobot.Shared.Models;

namespace Tobot.Client;

/// <summary>
/// Receives live telemetry pushed by the host's SignalR hub.
/// </summary>
public sealed class TobotTelemetryClient : IAsyncDisposable
{
	#region Private Fields

	/// <summary>
	/// The underlying hub connection.
	/// </summary>
	private readonly HubConnection _connection;

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TobotTelemetryClient"/> class.
	/// </summary>
	/// <param name="baseAddress">Base address of the host, e.g. <c>http://pi41:5247</c>.</param>
	public TobotTelemetryClient(Uri baseAddress)
	{
		_connection = new HubConnectionBuilder()
			.WithUrl(new Uri(baseAddress, TobotRoutes.TelemetryHub))
			.WithAutomaticReconnect()
			.Build();

		_connection.On<double>(TobotHubEvents.DistanceChanged, cm => DistanceChanged?.Invoke(this, cm));
		_connection.On<PiStatusSummary>(TobotHubEvents.PiStatusUpdated, status => PiStatusUpdated?.Invoke(this, status));
	}

	#endregion

	#region Events

	/// <summary>
	/// Raised when the measured distance (in centimeters) changed.
	/// </summary>
	public event EventHandler<double>? DistanceChanged;

	/// <summary>
	/// Raised when a new Pi status was pushed.
	/// </summary>
	public event EventHandler<PiStatusSummary>? PiStatusUpdated;

	#endregion

	#region Connection

	/// <summary>
	/// Gets a value indicating whether the hub connection is established.
	/// </summary>
	public bool IsConnected => _connection.State == HubConnectionState.Connected;

	/// <summary>
	/// Connects to the hub.
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the connect attempt.</param>
	/// <returns>A task that completes when connected.</returns>
	public Task StartAsync(CancellationToken cancellationToken = default) => _connection.StartAsync(cancellationToken);

	/// <summary>
	/// Disconnects from the hub.
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation.</param>
	/// <returns>A task that completes when disconnected.</returns>
	public Task StopAsync(CancellationToken cancellationToken = default) => _connection.StopAsync(cancellationToken);

	#endregion

	#region IAsyncDisposable

	/// <inheritdoc />
	public ValueTask DisposeAsync() => _connection.DisposeAsync();

	#endregion
}
