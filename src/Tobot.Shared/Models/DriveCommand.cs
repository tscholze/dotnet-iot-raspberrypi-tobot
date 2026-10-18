namespace Tobot.Shared.Models;

/// <summary>
/// Differential drive command: one speed per side, each in the range -100 (full reverse) to +100 (full forward).
/// </summary>
/// <param name="Left">Speed of the left motor in percent, -100..100.</param>
/// <param name="Right">Speed of the right motor in percent, -100..100.</param>
public sealed record DriveCommand(double Left, double Right)
{
	/// <summary>
	/// Lowest allowed speed value.
	/// </summary>
	public const double MinSpeed = -100;

	/// <summary>
	/// Highest allowed speed value.
	/// </summary>
	public const double MaxSpeed = 100;

	/// <summary>
	/// Command that stops both motors.
	/// </summary>
	public static DriveCommand Stop { get; } = new(0, 0);

	/// <summary>
	/// Gets a value indicating whether both speeds are finite and within <see cref="MinSpeed"/>..<see cref="MaxSpeed"/>.
	/// </summary>
	public bool IsValid => IsValidSpeed(Left) && IsValidSpeed(Right);

	/// <summary>
	/// Translates a preset <see cref="DriveAction"/> into a command.
	/// </summary>
	/// <param name="action">The preset movement.</param>
	/// <param name="speed">Speed magnitude in percent, 0..100.</param>
	/// <returns>The matching drive command.</returns>
	/// <exception cref="ArgumentOutOfRangeException">The action is unknown.</exception>
	public static DriveCommand FromAction(DriveAction action, double speed) => action switch
	{
		DriveAction.Stop => Stop,
		DriveAction.Forward => new(speed, speed),
		DriveAction.Backward => new(-speed, -speed),
		DriveAction.Left => new(-speed, speed),
		DriveAction.Right => new(speed, -speed),
		_ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown drive action.")
	};

	/// <summary>
	/// Checks a single speed value.
	/// </summary>
	/// <param name="speed">Speed in percent.</param>
	/// <returns><c>true</c> when finite and within range.</returns>
	private static bool IsValidSpeed(double speed) => double.IsFinite(speed) && speed is >= MinSpeed and <= MaxSpeed;
}
