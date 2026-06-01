using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;
using Microsoft.Maui.Hosting;
using System.Runtime.Versioning;

namespace Tobot.Gtk;

[SupportedOSPlatform("linux")]
public class Program : GtkMauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public static void Main(string[] args)
    {
        Environment.SetEnvironmentVariable("GTK_A11Y", "none");
        Environment.SetEnvironmentVariable("NO_AT_BRIDGE", "1");

        var app = new Program();
        app.Run(args);
    }
}
