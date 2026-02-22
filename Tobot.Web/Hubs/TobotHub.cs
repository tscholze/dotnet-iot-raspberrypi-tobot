using Microsoft.AspNetCore.SignalR;
using Tobot.Device.ExplorerHat;
using Tobot.Device.HcSr04;

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

    /// <summary>
    /// Subscription for reactive distance monitoring.
    /// </summary>
    private IDisposable? _distanceSubscription;

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

    #region Pan/Tilt Control

    /// <summary>
    /// Sets both pan and tilt.
    /// </summary>
    public async Task SetPanTilt(double panDegrees, double tiltDegrees)
    {
        _controller.PanTilt(panDegrees, tiltDegrees);
        var (pan, tilt) = _controller.GetPanTiltAngles();
        await Clients.All.SendAsync(TobotHubEvents.PanTiltChanged, pan, tilt);
    }

    /// <summary>
    /// Sets pan angle only.
    /// </summary>
    public async Task SetPan(double panDegrees)
    {
        _controller.SetPanAngle(panDegrees);
        var (pan, tilt) = _controller.GetPanTiltAngles();
        await Clients.All.SendAsync(TobotHubEvents.PanTiltChanged, pan, tilt);
    }

    /// <summary>
    /// Sets tilt angle only.
    /// </summary>
    public async Task SetTilt(double tiltDegrees)
    {
        _controller.SetTiltAngle(tiltDegrees);
        var (pan, tilt) = _controller.GetPanTiltAngles();
        await Clients.All.SendAsync(TobotHubEvents.PanTiltChanged, pan, tilt);
    }

    /// <summary>
    /// Gets current pan and tilt angles.
    /// </summary>
    public (double pan, double tilt) GetPanTilt()
    {
        return _controller.GetPanTiltAngles();
    }

    /// <summary>
    /// Gets Pan-Tilt idle timeout.
    /// </summary>
    public double GetPanTiltIdleTimeout()
    {
        return _controller.GetPanTiltIdleTimeout();
    }

    #endregion

    #region Distance & Detection

    /// <summary>
    /// Reads distance once; returns null if measurement fails.
    /// </summary>
    public double? ReadDistance(int samples = HcSr04Sensor.DefaultSamplesPerReading)
    {
        return _controller.TryReadDistance(out double distanceCm, samples) ? distanceCm : (double?)null;
    }

    /// <summary>
    /// Starts reactive distance monitoring and broadcasts updates.
    /// </summary>
    public void StartDistanceMonitoring(double thresholdCm = 1.0, int samples = HcSr04Sensor.DefaultSamplesPerReading)
    {
        _distanceSubscription?.Dispose();
        _distanceSubscription = _controller
            .ObserveDistance(thresholdCm, samples)
            .Subscribe(async d => await Clients.All.SendAsync(TobotHubEvents.DistanceChanged, d));
    }

    /// <summary>
    /// Stops reactive distance monitoring.
    /// </summary>
    public void StopDistanceMonitoring()
    {
        _distanceSubscription?.Dispose();
        _distanceSubscription = null;
    }

    /// <summary>
    /// Performs a detection sweep and broadcasts the result.
    /// </summary>
    public async Task<DetectedObject?> FindClosestObject(int samples = HcSr04Sensor.DefaultSamplesPerReading, int sweepIncrement = 5, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var result = _controller.FindClosestObject(samples, sweepIncrement, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if (result != null)
        {
            await Clients.All.SendAsync(TobotHubEvents.ObjectDetectionCompleted, result.Distance, result.PanAngle, result.Direction.ToString(), cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Reads distance and classifies direction at a given pan angle; broadcasts result.
    /// </summary>
    public async Task<(double? distanceCm, string direction)> TryReadDistanceWithDirection(int panAngleDegrees, int samples = HcSr04Sensor.DefaultSamplesPerReading)
    {
        if (_controller.TryReadDistanceWithDirection(panAngleDegrees, out double distanceCm, out ObjectDirection direction, samples))
        {
            await Clients.All.SendAsync(TobotHubEvents.DirectionClassified, panAngleDegrees, distanceCm, direction.ToString());
            return (distanceCm, direction.ToString());
        }

        await Clients.All.SendAsync(TobotHubEvents.DirectionClassified, panAngleDegrees, null, ObjectDirection.Center.ToString());
        return (null, ObjectDirection.Center.ToString());
    }

    #endregion

    #region Random Drive

    /// <summary>
    /// Starts autonomous random drive and notifies clients.
    /// </summary>
    public async Task StartRandomDrive(int forwardSpeed = 50, int turnSpeed = 40, double obstacleDistanceCm = 20.0, double clearDistanceCm = 30.0)
    {
        await Clients.All.SendAsync(TobotHubEvents.RandomDriveStarted, forwardSpeed, turnSpeed, obstacleDistanceCm, clearDistanceCm);
        _ = _controller.StartRandomDrive(forwardSpeed, turnSpeed, obstacleDistanceCm, clearDistanceCm);
    }

    /// <summary>
    /// Stops autonomous random drive and notifies clients.
    /// </summary>
    public async Task StopRandomDrive()
    {
        _controller.StopRandomDrive();
        await Clients.All.SendAsync(TobotHubEvents.RandomDriveStopped);
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
