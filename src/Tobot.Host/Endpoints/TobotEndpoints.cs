using Tobot.Device;
using Tobot.Device.HcSr04;
using Tobot.Host.Services;
using Tobot.Shared;
using Tobot.Shared.Models;

namespace Tobot.Host.Endpoints;

/// <summary>
/// Maps the REST API consumed by the CLI, MAUI app and remote firmware.
/// </summary>
public static class TobotEndpoints
{
	/// <summary>
	/// Maps all Tobot API endpoints (see <see cref="TobotRoutes"/>).
	/// </summary>
	/// <param name="app">The endpoint route builder.</param>
	/// <returns>The same builder for chaining.</returns>
	public static IEndpointRouteBuilder MapTobotApi(this IEndpointRouteBuilder app)
	{
		app.MapGet(TobotRoutes.Distance, (TobotController controller, int? samples) =>
			controller.TryReadDistance(out double cm, samples ?? HcSr04Sensor.DefaultSamplesPerReading)
				? Results.Ok(new DistanceReading(cm))
				: Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

		app.MapGet(TobotRoutes.Status, () => Results.Ok(PiStatusProvider.GetSummary()));

		app.MapPost(TobotRoutes.Drive, (TobotController controller, DriveCommand command, ILogger<TobotController> log) =>
			command.IsValid
				? Execute(() => controller.Drive(command.Left, command.Right), log)
				: Results.BadRequest($"Speeds must be between {DriveCommand.MinSpeed} and {DriveCommand.MaxSpeed}."));

		app.MapPost(TobotRoutes.DriveStop, (TobotController controller, ILogger<TobotController> log) =>
			Execute(controller.Stop, log));

		app.MapGet(TobotRoutes.Remote, (TobotController controller, IConfiguration config, ILogger<TobotController> log, string? action) =>
		{
			double speed = config.GetValue("Tobot:RemoteSpeed", 60.0);

			return action?.ToLowerInvariant() switch
			{
				"ping" or "light-on" or "light-off" => Results.Ok(),
				"stop" => Execute(controller.Stop, log),
				"forward" => Drive(controller, DriveAction.Forward, speed, log),
				"backwards" or "backward" => Drive(controller, DriveAction.Backward, speed, log),
				"left" => Drive(controller, DriveAction.Left, speed, log),
				"right" => Drive(controller, DriveAction.Right, speed, log),
				_ => Results.BadRequest("Unknown action.")
			};
		});

		return app;
	}

	/// <summary>
	/// Executes a preset <see cref="DriveAction"/>.
	/// </summary>
	/// <param name="controller">The hardware controller.</param>
	/// <param name="action">The preset movement.</param>
	/// <param name="speed">Speed in percent.</param>
	/// <param name="log">Logger for hardware failures.</param>
	/// <returns>The HTTP result.</returns>
	private static IResult Drive(TobotController controller, DriveAction action, double speed, ILogger log)
	{
		var command = DriveCommand.FromAction(action, speed);
		return Execute(() => controller.Drive(command.Left, command.Right), log);
	}

	/// <summary>
	/// Runs a hardware operation and maps failures (e.g. missing I2C bus) to 503.
	/// </summary>
	/// <param name="operation">The hardware call.</param>
	/// <param name="log">Logger for failures.</param>
	/// <returns>200 on success, 503 otherwise.</returns>
	private static IResult Execute(Action operation, ILogger log)
	{
		try
		{
			operation();
			return Results.Ok();
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException or InvalidOperationException)
		{
			log.LogError(ex, "Motor hardware not available.");
			return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
		}
	}
}
