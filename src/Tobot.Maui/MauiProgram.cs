using Microsoft.Maui.Hosting;

namespace Tobot.Maui;

/// <summary>
/// Builds the MAUI application.
/// </summary>
public static class MauiProgram
{
	/// <summary>
	/// Creates the <see cref="MauiApp"/>.
	/// </summary>
	/// <returns>The configured app.</returns>
	public static MauiApp CreateMauiApp() =>
		MauiApp.CreateBuilder().UseMauiApp<App>().Build();
}
