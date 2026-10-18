using Foundation;

namespace Tobot.Maui;

/// <summary>
/// Mac Catalyst app delegate.
/// </summary>
[Register("AppDelegate")]
public sealed class AppDelegate : MauiUIApplicationDelegate
{
	/// <inheritdoc />
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
