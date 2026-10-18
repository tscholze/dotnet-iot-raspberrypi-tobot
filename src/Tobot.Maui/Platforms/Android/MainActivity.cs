using Android.App;
using Android.Content.PM;

namespace Tobot.Maui;

/// <summary>
/// Android entry activity.
/// </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public sealed class MainActivity : MauiAppCompatActivity;
