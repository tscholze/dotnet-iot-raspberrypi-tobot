using Microsoft.Maui.Controls;

namespace Tobot.Gtk;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage())
        {
            Width = 720,
            Height = 1280,
            MinimumWidth = 720,
            MinimumHeight = 1280,
            MaximumWidth = 720,
            MaximumHeight = 1280
        };
    }
}
