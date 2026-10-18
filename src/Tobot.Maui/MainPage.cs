using Tobot.Client;

namespace Tobot.Maui;

/// <summary>
/// Minimal page that connects to <c>Tobot.Host</c> and shows live distance.
/// </summary>
public sealed class MainPage : ContentPage
{
	#region Private Fields

	/// <summary>
	/// Host address; replace with a settings page later (e.g. http://pi41:5247).
	/// </summary>
	private static readonly Uri HostAddress = new("http://pi41:5247");

	/// <summary>
	/// Label showing the latest distance.
	/// </summary>
	private readonly Label _distance = new() { Text = "--", FontSize = 48, HorizontalOptions = LayoutOptions.Center };

	/// <summary>
	/// Live telemetry connection.
	/// </summary>
	private readonly TobotTelemetryClient _telemetry = new(HostAddress);

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="MainPage"/> class.
	/// </summary>
	public MainPage()
	{
		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = { new Label { Text = "Distance (cm)", HorizontalOptions = LayoutOptions.Center }, _distance }
		};

		_telemetry.DistanceChanged += (_, cm) =>
			MainThread.BeginInvokeOnMainThread(() => _distance.Text = $"{cm:F1}");
	}

	#endregion

	#region Lifecycle

	/// <inheritdoc />
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		try { await _telemetry.StartAsync(); }
		catch (HttpRequestException) { _distance.Text = "offline"; }
	}

	#endregion
}
