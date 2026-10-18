using Tobot.Shared.Models;

namespace Tobot.Shared;

/// <summary>
/// Contract of the Tobot API as seen by remote consumers (CLI, MAUI, tests).
/// </summary>
public interface ITobotApi
{
	/// <summary>
	/// Reads the distance once.
	/// </summary>
	/// <param name="samples">Number of samples to average, or <c>null</c> for the server default.</param>
	/// <param name="cancellationToken">Token to cancel the request.</param>
	/// <returns>The reading, or <c>null</c> if the sensor could not be read.</returns>
	Task<DistanceReading?> GetDistanceAsync(int? samples = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Reads the current Pi status.
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the request.</param>
	/// <returns>The status summary.</returns>
	Task<PiStatusSummary?> GetStatusAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the speed of both motors.
	/// </summary>
	/// <param name="command">The drive command.</param>
	/// <param name="cancellationToken">Token to cancel the request.</param>
	/// <returns><c>true</c> when the host accepted the command.</returns>
	Task<bool> DriveAsync(DriveCommand command, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stops both motors.
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the request.</param>
	/// <returns><c>true</c> when the host accepted the command.</returns>
	Task<bool> StopAsync(CancellationToken cancellationToken = default);
}
