using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;
using Microsoft.Maui.Hosting;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace Tobot.Gtk;

internal static class GtkStartupEnvironment
{
    [ModuleInitializer]
    internal static void Configure()
    {
        Environment.SetEnvironmentVariable("GTK_A11Y", "none");
        Environment.SetEnvironmentVariable("NO_AT_BRIDGE", "1");
    }
}

[SupportedOSPlatform("linux")]
public class Program : GtkMauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
