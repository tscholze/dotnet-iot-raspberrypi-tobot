using Tobot.Device;
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
			controller.TryReadDistance(out double cm, samples ?? Device.HcSr04.HcSr04Sensor.DefaultSamplesPerReading)
				? Results.Ok(new DistanceReading(cm))
				: Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

		app.MapGet(TobotRoutes.Status, () => Results.Ok(PiStatusProvider.GetSummary()));

		return app;
	}
}
