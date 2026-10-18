using Android.App;
using Android.Runtime;

namespace Tobot.Maui;

/// <summary>
/// Android application class.
/// </summary>
/// <param name="handle">Native handle.</param>
/// <param name="ownership">Handle ownership.</param>
[Application(UsesCleartextTraffic = true)]
public sealed class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
	/// <inheritdoc />
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
