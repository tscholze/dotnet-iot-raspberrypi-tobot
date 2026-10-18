namespace Tobot.Shared.Models;

/// <summary>
/// Simple preset movements, e.g. triggered by a keypad remote.
/// </summary>
public enum DriveAction
{
	/// <summary>
	/// Stop both motors.
	/// </summary>
	Stop,

	/// <summary>
	/// Drive straight forward.
	/// </summary>
	Forward,

	/// <summary>
	/// Drive straight backward.
	/// </summary>
	Backward,

	/// <summary>
	/// Turn left on the spot.
	/// </summary>
	Left,

	/// <summary>
	/// Turn right on the spot.
	/// </summary>
	Right
}
