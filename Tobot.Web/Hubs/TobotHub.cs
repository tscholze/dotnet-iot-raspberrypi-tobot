using Microsoft.AspNetCore.SignalR;
using Tobot.Device.ExplorerHat;

namespace Tobot.Web.Hubs;

/// <summary>
/// SignalR hub for controlling the Tobot ExplorerHat device.
/// Exposes all public features of ExplorerHat to remote clients.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TobotHub"/> class.
/// </remarks>
/// <param name="explorerHat">The ExplorerHat instance.</param>
/// <param name="controller">The TobotController instance.</param>
public class TobotHub(ExplorerHat explorerHat, Device.TobotController controller) : Hub
{
    #region Private Fields

    /// <summary>
    /// The ExplorerHat instance to control.
    /// </summary>
    private readonly ExplorerHat _explorerHat = explorerHat;

    /// <summary>
    /// The TobotController instance for higher-level operations.
    /// </summary>
    private readonly Device.TobotController _controller = controller;

    #endregion

    #region Motor Control

    /// <summary>
    /// Sets the speed of a specific motor.
    /// </summary>
    /// <param name="motorNumber">Motor number (1 or 2).</param>
    /// <param name="speed">Speed value from -100 (full backward) to +100 (full forward).</param>
    public async Task SetMotorSpeed(int motorNumber, double speed)
    {
        _explorerHat.Motor[motorNumber].SetSpeed(speed);
        await Clients.All.SendAsync(TobotHubEvents.MotorSpeedChanged, motorNumber, speed);
    }

    /// <summary>
    /// Moves a motor forward.
    /// </summary>
    /// <param name="motorNumber">Motor number (1 or 2).</param>
    /// <param name="speed">Speed value from 0 to 100.</param>
    public async Task MoveMotorForward(int motorNumber, double speed = 100)
    {
        _explorerHat.Motor[motorNumber].Forward(speed);
        await Clients.All.SendAsync(TobotHubEvents.MotorSpeedChanged, motorNumber, speed);
    }

    /// <summary>
    /// Moves a motor backward.
    /// </summary>
    /// <param name="motorNumber">Motor number (1 or 2).</param>
    /// <param name="speed">Speed value from 0 to 100.</param>
    public async Task MoveMotorBackward(int motorNumber, double speed = 100)
    {
        _explorerHat.Motor[motorNumber].Backward(speed);
        await Clients.All.SendAsync(TobotHubEvents.MotorSpeedChanged, motorNumber, -speed);
    }

    /// <summary>
    /// Stops a specific motor.
    /// </summary>
    /// <param name="motorNumber">Motor number (1 or 2).</param>
    public async Task StopMotor(int motorNumber)
    {
        _explorerHat.Motor[motorNumber].Stop();
        await Clients.All.SendAsync(TobotHubEvents.MotorSpeedChanged, motorNumber, 0);
    }

    /// <summary>
    /// Stops all motors.
    /// </summary>
    public async Task StopAllMotors()
    {
        _explorerHat.Motor.Stop();
        await Clients.All.SendAsync(TobotHubEvents.AllMotorsStopped);
    }

    #endregion

    #region LED Control

    /// <summary>
    /// Turns an LED on.
    /// </summary>
    /// <param name="ledNumber">LED number (1-4).</param>
    public async Task TurnLedOn(int ledNumber)
    {
        _explorerHat.Light[ledNumber].On();
        await Clients.All.SendAsync(TobotHubEvents.LedStateChanged, ledNumber, true);
    }

    /// <summary>
    /// Turns an LED off.
    /// </summary>
    /// <param name="ledNumber">LED number (1-4).</param>
    public async Task TurnLedOff(int ledNumber)
    {
        _explorerHat.Light[ledNumber].Off();
        await Clients.All.SendAsync(TobotHubEvents.LedStateChanged, ledNumber, false);
    }

    /// <summary>
    /// Toggles an LED.
    /// </summary>
    /// <param name="ledNumber">LED number (1-4).</param>
    public async Task ToggleLed(int ledNumber)
    {
        _explorerHat.Light[ledNumber].Toggle();
        await Clients.All.SendAsync(TobotHubEvents.LedToggled, ledNumber);
    }

    #endregion

    #region Digital Output Control

    /// <summary>
    /// Turns a digital output on.
    /// </summary>
    /// <param name="outputNumber">Output number (1-4).</param>
    public async Task TurnOutputOn(int outputNumber)
    {
        _explorerHat.Output[outputNumber].On();
        await Clients.All.SendAsync(TobotHubEvents.OutputStateChanged, outputNumber, true);
    }

    /// <summary>
    /// Turns a digital output off.
    /// </summary>
    /// <param name="outputNumber">Output number (1-4).</param>
    public async Task TurnOutputOff(int outputNumber)
    {
        _explorerHat.Output[outputNumber].Off();
        await Clients.All.SendAsync(TobotHubEvents.OutputStateChanged, outputNumber, false);
    }

    /// <summary>
    /// Toggles a digital output.
    /// </summary>
    /// <param name="outputNumber">Output number (1-4).</param>
    public async Task ToggleOutput(int outputNumber)
    {
        _explorerHat.Output[outputNumber].Toggle();
        await Clients.All.SendAsync(TobotHubEvents.OutputToggled, outputNumber);
    }

    #endregion

    #region Digital Input Reading

    /// <summary>
    /// Reads the state of a digital input.
    /// </summary>
    /// <param name="inputNumber">Input number (1-4).</param>
    /// <returns>True if input is high, false if low.</returns>
    public bool ReadInput(int inputNumber)
    {
        return _explorerHat.Input[inputNumber].Read();
    }

    #endregion

    #region Analog Input Reading

    /// <summary>
    /// Reads an analog input value.
    /// </summary>
    /// <param name="inputNumber">Analog input number (1-4).</param>
    /// <returns>Analog value (typically 0-5V range depending on ADC).</returns>
    public double ReadAnalogInput(int inputNumber)
    {
        return _explorerHat.Analog[inputNumber].Read();
    }

    #endregion

    #region Touch Sensor Reading

    /// <summary>
    /// Checks if a touch sensor is pressed.
    /// </summary>
    /// <param name="touchNumber">Touch sensor number (1-8).</param>
    /// <returns>True if pressed, false otherwise.</returns>
    public bool IsTouchPressed(int touchNumber)
    {
        return _explorerHat.Touch[touchNumber].IsTouched();
    }

    #endregion

    #region Light Show

    /// <summary>
    /// Plays a light show by sequentially lighting each LED in a pattern.
    /// </summary>
    /// <param name="cycleCount">Number of complete cycles to perform (default 10).</param>
    public async Task PlayLightShow(int cycleCount = 10)
    {
        await Clients.All.SendAsync(TobotHubEvents.LightShowStarted, cycleCount);
        await Task.Run(() => _controller.PlayLightShow(cycleCount));
    }

    #endregion
}
