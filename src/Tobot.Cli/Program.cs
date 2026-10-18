using Tobot.Client;

namespace Tobot.Cli;

/// <summary>
/// Entry point of the Tobot command-line client.
/// </summary>
internal static class Program
{
	/// <summary>
	/// Default host address, overridable via the <c>TOBOT_URL</c> environment variable.
	/// </summary>
	private const string DefaultUrl = "http://localhost:5247";

	/// <summary>
	/// Runs a single command from the arguments, or an interactive shell when none is given.
	/// </summary>
	/// <param name="args">Command line arguments, e.g. <c>distance</c>.</param>
	/// <returns>Process exit code.</returns>
	private static async Task<int> Main(string[] args)
	{
		var baseAddress = new Uri(Environment.GetEnvironmentVariable("TOBOT_URL") ?? DefaultUrl);
		using var api = new TobotApiClient(baseAddress);

		if (args.Length > 0)
		{
			return await RunAsync(string.Join(' ', args), api, baseAddress) ? 0 : 1;
		}

		Console.WriteLine($"Tobot CLI -> {baseAddress}");
		Console.WriteLine("Type 'help' to see available commands or 'exit' to quit.");

		while (true)
		{
			Console.Write("tobot> ");
			string? input = Console.ReadLine();
			if (input is null || input.Trim() is "exit" or "quit")
			{
				return 0;
			}

			await RunAsync(input, api, baseAddress);
		}
	}

	/// <summary>
	/// Executes one command.
	/// </summary>
	/// <param name="command">The command text.</param>
	/// <param name="api">API client.</param>
	/// <param name="baseAddress">Host address used for telemetry.</param>
	/// <returns><c>true</c> if the command succeeded.</returns>
	private static async Task<bool> RunAsync(string command, TobotApiClient api, Uri baseAddress)
	{
		try
		{
			switch (command.Trim().ToLowerInvariant())
			{
				case "":
					return true;
				case "help":
					Console.WriteLine("  distance  Read the distance once.");
					Console.WriteLine("  status    Show Pi status.");
					Console.WriteLine("  watch     Stream live distance (Ctrl+C to stop).");
					Console.WriteLine("  exit      Leave the shell.");
					return true;
				case "distance":
					var reading = await api.GetDistanceAsync();
					Console.WriteLine(reading is null ? "Sensor not readable." : $"{reading.DistanceCm:F1} cm");
					return reading is not null;
				case "status":
					Console.WriteLine(await api.GetStatusAsync());
					return true;
				case "watch":
					await WatchAsync(baseAddress);
					return true;
				default:
					Console.WriteLine("Unknown command. Type 'help'.");
					return false;
			}
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			Console.WriteLine($"Host not reachable: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Streams live distance updates until Ctrl+C is pressed.
	/// </summary>
	/// <param name="baseAddress">Host address.</param>
	/// <returns>A task that completes when streaming stops.</returns>
	private static async Task WatchAsync(Uri baseAddress)
	{
		await using var telemetry = new TobotTelemetryClient(baseAddress);
		telemetry.DistanceChanged += (_, cm) => Console.WriteLine($"{cm:F1} cm");
		await telemetry.StartAsync();

		using var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
		await Task.Delay(Timeout.Infinite, cts.Token).ContinueWith(_ => { });
	}
}
