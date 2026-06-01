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
        var app = new Program();
        app.Run(args);
    }
}
