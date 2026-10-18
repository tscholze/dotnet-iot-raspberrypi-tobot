using Tobot.Shared.Models;

namespace Tobot.Tests;

/// <summary>
/// Tests for <see cref="DriveCommand"/>.
/// </summary>
public class DriveCommandTests
{
	/// <summary>
	/// Presets map to the expected differential speeds.
	/// </summary>
	[Theory]
	[InlineData(DriveAction.Stop, 0, 0)]
	[InlineData(DriveAction.Forward, 50, 50)]
	[InlineData(DriveAction.Backward, -50, -50)]
	[InlineData(DriveAction.Left, -50, 50)]
	[InlineData(DriveAction.Right, 50, -50)]
	public void FromAction_MapsSpeeds(DriveAction action, double left, double right)
	{
		Assert.Equal(new DriveCommand(left, right), DriveCommand.FromAction(action, 50));
	}

	/// <summary>
	/// Out-of-range and non-finite values are invalid.
	/// </summary>
	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(-100, 100, true)]
	[InlineData(100.1, 0, false)]
	[InlineData(0, -101, false)]
	[InlineData(double.NaN, 0, false)]
	public void IsValid_ChecksRange(double left, double right, bool expected)
	{
		Assert.Equal(expected, new DriveCommand(left, right).IsValid);
	}
}
