namespace Tobot.Maui;

/// <summary>
/// Application root; shows <see cref="MainPage"/>.
/// </summary>
public sealed class App : Application
{
	/// <inheritdoc />
	protected override Window CreateWindow(IActivationState? activationState) => new(new MainPage());
}
