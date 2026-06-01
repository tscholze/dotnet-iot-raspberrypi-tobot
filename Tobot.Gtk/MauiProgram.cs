using Microsoft.Maui.Platforms.Linux.Gtk4.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Hosting;
using System.Runtime.Versioning;
using System.Reflection;

namespace Tobot.Gtk;

[SupportedOSPlatform("linux")]
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiAppLinuxGtk4<App>();

        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddGtk(gtk =>
            {
                gtk.OnWindowCreated(window =>
                {
                    var windowType = window.GetType();

                    InvokeIfExists(window, windowType, "Fullscreen");
                    InvokeIfExists(window, windowType, "Maximize");
                    InvokeIfExists(window, windowType, "Present");
                });
            });
        });

        return builder.Build();
    }

    private static void InvokeIfExists(object instance, Type instanceType, string methodName)
    {
        var method = instanceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        method?.Invoke(instance, null);
    }
}
