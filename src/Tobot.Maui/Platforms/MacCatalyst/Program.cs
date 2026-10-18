using UIKit;

namespace Tobot.Maui;

/// <summary>
/// Mac Catalyst entry point.
/// </summary>
public static class Program
{
	/// <summary>
	/// Starts the UIKit application.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	public static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}
