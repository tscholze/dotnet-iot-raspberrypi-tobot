using System;
using System.Device.Gpio;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tobot.Device.ExplorerHat.Motor;
using Tobot.Device.HcSr04;
using ExplorerHatDevice = Tobot.Device.ExplorerHat.ExplorerHat;
using PanTiltHatDevice = Tobot.Device.PanTiltHat.PanTiltHat;

namespace Tobot.Device;

/// <summary>
/// Central orchestrator that wires up all Tobot peripherals (Explorer HAT, Pan-Tilt HAT, HC-SR04).
/// Provides shared GPIO access, lazy component initialization, and convenience helpers for higher-level apps.
/// </summary>
public sealed class TobotController : IDisposable
{
	/// <summary>
	/// Lazy-initialized GPIO controller shared by every attached peripheral.
	/// </summary>
	private readonly Lazy<GpioController> _gpioController = new(() => new GpioController());

	/// <summary>
	/// Cached Explorer HAT instance (created on first use).
	/// </summary>
	private ExplorerHatDevice? _explorerHat;

	/// <summary>
	/// Cached Pan-Tilt HAT instance (created on first use).
	/// </summary>
	private PanTiltHatDevice? _panTiltHat;

	/// <summary>
	/// Cached HC-SR04 distance sensor (created on first use).
	/// </summary>
	private HcSr04Sensor? _distanceSensor;

	/// <summary>
	/// Subject for broadcasting distance changes.
	/// </summary>
	private readonly Subject<double> _distanceSubject = new();

	/// <summary>
	/// Subscription for distance monitoring timer.
	/// </summary>
	private IDisposable? _distanceMonitoringSubscription;

	/// <summary>
	/// Last measured distance value for change detection.
	/// </summary>
	private double? _lastDistance;

	/// <summary>
	/// Tracks whether the controller has been disposed.
	/// </summary>
	private bool _disposed;

	/// <summary>
	/// Cancellation token source for the random drive loop.
	/// </summary>
	private CancellationTokenSource? _randomDriveCts;

	/// <summary>
	/// Random number generator for turn direction selection.
	/// </summary>
	private static readonly Random _random = new();

	/// <summary>
	/// Gets the shared <see cref="GpioController"/> used across all peripherals.
	/// </summary>
	public GpioController GpioController
	{
		get
		{
			EnsureNotDisposed();
			return _gpioController.Value;
		}
	}

	/// <summary>
	/// Gets the Explorer HAT abstraction, creating it on first use.
	/// </summary>
	private ExplorerHatDevice ExplorerHat
	{
		get
		{
			EnsureNotDisposed();
			return _explorerHat ??= new ExplorerHatDevice();
		}
	}

	/// <summary>
	/// Gets the Pan-Tilt HAT abstraction, creating it on first use.
	/// </summary>
	private PanTiltHatDevice PanTiltHat
	{
		get
		{
			EnsureNotDisposed();
			return _panTiltHat ??= new PanTiltHatDevice();
		}
	}

	/// <summary>
	/// Gets the HC-SR04 distance sensor abstraction, creating it on first use.
	/// </summary>
	private HcSr04Sensor UltrasonicSensor
	{
		get
		{
			EnsureNotDisposed();
			return _distanceSensor ??= new HcSr04Sensor(GpioController);
		}
	}

	/// <summary>
	/// Sets the specified LED on the Explorer HAT to the requested state.
	/// </summary>
	/// <param name="ledIndex">LED index (1-4).</param>
	/// <param name="isOn">Flag indicating whether the LED should be lit.</param>
	public void SetLedState(int ledIndex, bool isOn)
	{
		EnsureNotDisposed();
		ValidateChannel(ledIndex);
		if (isOn)
		{
			ExplorerHat.Light[ledIndex].On();
		}
		else
		{
			ExplorerHat.Light[ledIndex].Off();
		}
	}

	/// <summary>
	/// Sets all Explorer HAT LEDs to a single state.
	/// </summary>
	/// <param name="isOn">Flag indicating whether every LED should be lit.</param>
	public void SetAllLeds(bool isOn)
	{
		EnsureNotDisposed();
		if (isOn)
		{
			ExplorerHat.Light.On();
		}
		else
		{
			ExplorerHat.Light.Off();
		}
	}

	/// <summary>
	/// Toggles the specified LED.
	/// </summary>
	/// <param name="ledIndex">LED index (1-4).</param>
	public void ToggleLed(int ledIndex)
	{
		EnsureNotDisposed();
		ValidateChannel(ledIndex);
		ExplorerHat.Light[ledIndex].Toggle();
	}

	/// <summary>
	/// Drives a digital output line on the Explorer HAT.
	/// </summary>
	/// <param name="outputIndex">Output index (1-4).</param>
	/// <param name="isOn">Flag indicating whether the output should be high.</param>
	public void SetDigitalOutput(int outputIndex, bool isOn)
	{
		EnsureNotDisposed();
		ValidateChannel(outputIndex);
		if (isOn)
		{
			ExplorerHat.Output[outputIndex].On();
		}
		else
		{
			ExplorerHat.Output[outputIndex].Off();
		}
	}

	/// <summary>
	/// Sets all digital outputs to a single state.
	/// </summary>
	/// <param name="isOn">Flag indicating whether every output should be high.</param>
	public void SetAllDigitalOutputs(bool isOn)
	{
		EnsureNotDisposed();
		if (isOn)
		{
			ExplorerHat.Output.On();
		}
		else
		{
			ExplorerHat.Output.Off();
		}
	}

	/// <summary>
	/// Toggles the specified digital output.
	/// </summary>
	/// <param name="outputIndex">Output index (1-4).</param>
	public void ToggleDigitalOutput(int outputIndex)
	{
		EnsureNotDisposed();
		ValidateChannel(outputIndex);
		ExplorerHat.Output[outputIndex].Toggle();
	}

	/// <summary>
	/// Reads a single digital input.
	/// </summary>
	/// <param name="inputIndex">Input index (1-4).</param>
	/// <returns><c>true</c> when the input is high; otherwise <c>false</c>.</returns>
	public bool ReadDigitalInput(int inputIndex)
	{
		EnsureNotDisposed();
		ValidateChannel(inputIndex);
		return ExplorerHat.Input[inputIndex].Read();
	}

	/// <summary>
	/// Registers a callback for changes on a digital input.
	/// </summary>
	/// <param name="inputIndex">Input index (1-4).</param>
	/// <param name="handler">Handler to invoke on pin changes.</param>
	public void RegisterInputChangedHandler(int inputIndex, PinChangeEventHandler handler)
	{
		EnsureNotDisposed();
		ArgumentNullException.ThrowIfNull(handler);
		ValidateChannel(inputIndex);
		ExplorerHat.Input[inputIndex].Changed += handler;
	}

	/// <summary>
	/// Unregisters a previously attached input change handler.
	/// </summary>
	/// <param name="inputIndex">Input index (1-4).</param>
	/// <param name="handler">Handler to remove.</param>
	public void UnregisterInputChangedHandler(int inputIndex, PinChangeEventHandler handler)
	{
		EnsureNotDisposed();
		ArgumentNullException.ThrowIfNull(handler);
		ValidateChannel(inputIndex);
		ExplorerHat.Input[inputIndex].Changed -= handler;
	}

	/// <summary>
	/// Reads the current state of all Explorer HAT digital inputs.
	/// </summary>
	/// <returns>Array of four booleans representing inputs 1-4.</returns>
	public bool[] ReadDigitalInputs()
	{
		EnsureNotDisposed();
		return new[]
		{
			ReadDigitalInput(1),
			ReadDigitalInput(2),
			ReadDigitalInput(3),
			ReadDigitalInput(4)
		};
	}

	/// <summary>
	/// Reads the latest analog values from the Explorer HAT ADC.
	/// </summary>
	/// <returns>Array of four voltage readings.</returns>
	public double[] ReadAnalogValues()
	{
		EnsureNotDisposed();
		return new[]
		{
			ReadAnalogValue(1),
			ReadAnalogValue(2),
			ReadAnalogValue(3),
			ReadAnalogValue(4)
		};
	}

	/// <summary>
	/// Reads a single analog channel.
	/// </summary>
	/// <param name="channel">Analog channel (1-4).</param>
	/// <returns>Voltage reading.</returns>
	public double ReadAnalogValue(int channel)
	{
		EnsureNotDisposed();
		ValidateChannel(channel);
		return ExplorerHat.Analog[channel].Read();
	}

	/// <summary>
	/// Reads the specified touch sensor.
	/// </summary>
	/// <param name="sensorIndex">Sensor index (1-4).</param>
	/// <returns><c>true</c> when touched; otherwise <c>false</c>.</returns>
	public bool ReadTouchSensor(int sensorIndex)
	{
		EnsureNotDisposed();
		ValidateChannel(sensorIndex);
		return ExplorerHat.Touch[sensorIndex].IsTouched();
	}

	/// <summary>
	/// Reads the raw touch state bitmap.
	/// </summary>
	/// <returns>Bitmask representing active touch sensors.</returns>
	public byte ReadTouchState()
	{
		EnsureNotDisposed();
		return ExplorerHat.Touch.ReadAll();
	}

	/// <summary>
	/// Drives both Explorer HAT motors using percentage-based speed commands.
	/// </summary>
	/// <param name="motorOnePercent">Speed for motor one (-100 to 100).</param>
	/// <param name="motorTwoPercent">Speed for motor two (-100 to 100).</param>
	public void DriveMotors(int motorOnePercent, int motorTwoPercent)
	{
		EnsureNotDisposed();
		DriveMotor(1, motorOnePercent);
		DriveMotor(2, motorTwoPercent);
	}

	/// <summary>
	/// Drives a single motor using a percentage-based speed command.
	/// </summary>
	/// <param name="motorIndex">Motor index (1 or 2).</param>
	/// <param name="speedPercent">Speed between -100 and 100.</param>
	public void DriveMotor(int motorIndex, double speedPercent)
	{
		EnsureNotDisposed();
		var motor = GetMotor(motorIndex);
		ApplyMotorSpeed(motor, (int)Math.Round(speedPercent));
	}

	/// <summary>
	/// Stops both Explorer HAT motors.
	/// </summary>
	public void StopMotors()
	{
		EnsureNotDisposed();
		ExplorerHat.Motor.Stop();
	}

	/// <summary>
	/// Stops a single Explorer HAT motor.
	/// </summary>
	/// <param name="motorIndex">Motor index (1 or 2).</param>
	public void StopMotor(int motorIndex)
	{
		EnsureNotDisposed();
		GetMotor(motorIndex).Stop();
	}

	/// <summary>
	/// Moves the Pan-Tilt HAT to the desired pan/tilt angles.
	/// </summary>
	/// <param name="panDegrees">Pan angle in degrees (-90 to 90 typical).</param>
	/// <param name="tiltDegrees">Tilt angle in degrees (-45 to 45 typical).</param>
	public void PanTilt(double panDegrees, double tiltDegrees)
	{
		EnsureNotDisposed();
		PanTiltHat.Pan((int)Math.Round(panDegrees));
		PanTiltHat.Tilt((int)Math.Round(tiltDegrees));
	}

	/// <summary>
	/// Gets the current pan and tilt angles as reported by the servo controller.
	/// </summary>
	/// <returns>Tuple containing the current pan and tilt angles.</returns>
	public (double Pan, double Tilt) GetPanTiltAngles()
	{
		EnsureNotDisposed();
		return (PanTiltHat.GetPan(), PanTiltHat.GetTilt());
	}

	/// <summary>
	/// Sets only the pan axis to the desired angle.
	/// </summary>
	/// <param name="panDegrees">Pan angle in degrees.</param>
	public void SetPanAngle(double panDegrees)
	{
		EnsureNotDisposed();
		PanTiltHat.Pan((int)Math.Round(panDegrees));
	}

	/// <summary>
	/// Sets only the tilt axis to the desired angle.
	/// </summary>
	/// <param name="tiltDegrees">Tilt angle in degrees.</param>
	public void SetTiltAngle(double tiltDegrees)
	{
		EnsureNotDisposed();
		PanTiltHat.Tilt((int)Math.Round(tiltDegrees));
	}

	/// <summary>
	/// Gets the current Pan-Tilt HAT idle timeout value.
	/// </summary>
	/// <returns>Timeout in seconds.</returns>
	public double GetPanTiltIdleTimeout()
	{
		EnsureNotDisposed();
		return PanTiltHat.IdleTimeout;
	}

	/// <summary>
	/// Attempts to read the HC-SR04 distance sensor and returns the latest value in centimeters.
	/// </summary>
	/// <param name="distanceCm">Distance output in centimeters.</param>
	/// <param name="samples">Number of samples to average.</param>
	/// <returns>True when a measurement succeeds; otherwise false.</returns>
	public bool TryReadDistance(out double distanceCm, int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		EnsureNotDisposed();
		return UltrasonicSensor.TryReadDistance(out distanceCm, samples);
	}

	/// <summary>
	/// Reads the HC-SR04 distance sensor and throws if no measurement succeeds.
	/// </summary>
	/// <param name="samples">Number of samples to average.</param>
	/// <returns>Measured distance in centimeters.</returns>
	public double ReadDistance(int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		EnsureNotDisposed();
		return UltrasonicSensor.ReadDistance(samples);
	}

	/// <summary>
	/// Gets an observable sequence that emits distance measurements when they change by more than the specified threshold.
	/// The sensor is polled every 500ms and changes greater than 1cm trigger a notification.
	/// </summary>
	/// <param name="thresholdCm">Minimum change in centimeters to trigger notification (default 1.0cm).</param>
	/// <param name="samples">Number of samples to average per reading.</param>
	/// <returns>Observable sequence of distance measurements in centimeters.</returns>
	public IObservable<double> ObserveDistance(double thresholdCm = 1.0, int samples = HcSr04Sensor.DefaultSamplesPerReading)
	{
		EnsureNotDisposed();

		if (thresholdCm <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(thresholdCm), "Threshold must be greater than zero.");
		}

		// Start monitoring if not already active
		if (_distanceMonitoringSubscription == null)
		{
			StartDistanceMonitoring(thresholdCm, samples);
		}

		return _distanceSubject.AsObservable();
	}

	/// <summary>
	/// Starts autonomous driving mode where the robot drives forward until an obstacle is detected,
	/// then turns randomly left or right until clear, and continues forward.
	/// </summary>
	/// <param name="forwardSpeed">Speed for forward movement (0-100, default 50).</param>
	/// <param name="turnSpeed">Speed for turning (0-100, default 40).</param>
	/// <param name="obstacleDistanceCm">Distance threshold to trigger obstacle avoidance (default 20cm).</param>
	/// <param name="clearDistanceCm">Distance threshold to resume forward movement (default 30cm).</param>
	/// <returns>Task that completes when autonomous driving is stopped via <see cref="StopRandomDrive"/>.</returns>
	public Task StartRandomDrive(
		int forwardSpeed = 50,
		int turnSpeed = 40,
		double obstacleDistanceCm = 20.0,
		double clearDistanceCm = 30.0)
	{
		EnsureNotDisposed();

		if (forwardSpeed is < 0 or > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(forwardSpeed), "Forward speed must be between 0 and 100.");
		}

		if (turnSpeed is < 0 or > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(turnSpeed), "Turn speed must be between 0 and 100.");
		}

		if (obstacleDistanceCm <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(obstacleDistanceCm), "Obstacle distance must be greater than zero.");
		}

		if (clearDistanceCm <= obstacleDistanceCm)
		{
			throw new ArgumentOutOfRangeException(nameof(clearDistanceCm), "Clear distance must be greater than obstacle distance.");
		}

		// Stop any existing random drive
		StopRandomDrive();

		_randomDriveCts = new CancellationTokenSource();
		return RunRandomDriveLoopAsync(forwardSpeed, turnSpeed, obstacleDistanceCm, clearDistanceCm, _randomDriveCts.Token);
	}

	/// <summary>
	/// Stops the autonomous driving mode and halts all motors.
	/// </summary>
	public void StopRandomDrive()
	{
		_randomDriveCts?.Cancel();
		_randomDriveCts?.Dispose();
		_randomDriveCts = null;
		StopMotors();
	}

	/// <summary>
	/// Stops all robot actuators including motors, LEDs, and digital outputs.
	/// Useful for emergency stop or cleanup operations.
	/// </summary>
	public void Stop()
	{
		EnsureNotDisposed();
		StopRandomDrive();
		StopMotors();
		SetAllLeds(false);
		SetAllDigitalOutputs(false);
	}

	/// <summary>
	/// Executes the autonomous driving loop.
	/// </summary>
	private async Task RunRandomDriveLoopAsync(
		int forwardSpeed,
		int turnSpeed,
		double obstacleDistanceCm,
		double clearDistanceCm,
		CancellationToken cancellationToken)
	{
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				// Drive forward until obstacle detected
				DriveMotors(forwardSpeed, forwardSpeed);

				while (!cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(100, cancellationToken).ConfigureAwait(false);

					if (TryReadDistance(out double distance) && distance < obstacleDistanceCm)
					{
						break;
					}
				}

				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				// Obstacle detected - choose random turn direction
				bool turnLeft = _random.Next(2) == 0;

				// Turn until clear
				if (turnLeft)
				{
					DriveMotors(-turnSpeed, turnSpeed); // Left motor backward, right forward
				}
				else
				{
					DriveMotors(turnSpeed, -turnSpeed); // Left motor forward, right backward
				}

				while (!cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(100, cancellationToken).ConfigureAwait(false);

					if (TryReadDistance(out double distance) && distance > clearDistanceCm)
					{
						break;
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when stopping
		}
		finally
		{
			StopMotors();
		}
	}

	/// <summary>
	/// Starts the background distance monitoring timer.
	/// </summary>
	/// <param name="thresholdCm">Minimum change threshold.</param>
	/// <param name="samples">Number of samples per reading.</param>
	private void StartDistanceMonitoring(double thresholdCm, int samples)
	{
		_distanceMonitoringSubscription = Observable
			.Interval(TimeSpan.FromMilliseconds(500))
			.Subscribe(_ =>
			{
				if (_disposed)
				{
					return;
				}

				try
				{
					if (TryReadDistance(out double currentDistance, samples))
					{
						if (_lastDistance == null || Math.Abs(currentDistance - _lastDistance.Value) >= thresholdCm)
						{
							_lastDistance = currentDistance;
							_distanceSubject.OnNext(currentDistance);
						}
					}
				}
				catch (Exception ex)
				{
					_distanceSubject.OnError(ex);
				}
			});
	}

	/// <summary>
	/// Releases all managed resources associated with the controller and its peripherals.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		StopRandomDrive();
		_distanceMonitoringSubscription?.Dispose();
		_distanceSubject.OnCompleted();
		_distanceSubject.Dispose();
		_distanceSensor?.Dispose();
		_panTiltHat?.Dispose();
		_explorerHat?.Dispose();
		if (_gpioController.IsValueCreated)
		{
			_gpioController.Value.Dispose();
		}

		_disposed = true;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Applies a signed speed value to the provided Explorer HAT motor.
	/// </summary>
	/// <param name="motor">Motor to drive.</param>
	/// <param name="speedPercent">Requested speed between -100 (full reverse) and 100 (full forward).</param>
	private static void ApplyMotorSpeed(Motor motor, int speedPercent)
	{
		int clamped = Math.Clamp(speedPercent, -100, 100);
		if (clamped > 0)
		{
			motor.Forward(clamped);
		}
		else if (clamped < 0)
		{
			motor.Backward(Math.Abs(clamped));
		}
		else
		{
			motor.Stop();
		}
	}

	/// <summary>
	/// Gets a single Explorer HAT motor by index.
	/// </summary>
	/// <param name="motorIndex">Motor index (1 or 2).</param>
	/// <returns>Requested motor instance.</returns>
	private Motor GetMotor(int motorIndex)
	{
		ValidateMotorIndex(motorIndex);
		return motorIndex == 1 ? ExplorerHat.Motor.One : ExplorerHat.Motor.Two;
	}

	/// <summary>
	/// Ensures the controller has not been disposed before servicing requests.
	/// </summary>
	private void EnsureNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(TobotController));
		}
	}

	/// <summary>
	/// Validates that a 1-based Explorer HAT channel index is in range.
	/// </summary>
	/// <param name="channel">Channel index to validate.</param>
	private static void ValidateChannel(int channel)
	{
		if (channel is < 1 or > 4)
		{
			throw new ArgumentOutOfRangeException(nameof(channel), "Explorer HAT exposes four channels (1-4).");
		}
	}

	/// <summary>
	/// Validates that a motor index is within the supported range.
	/// </summary>
	/// <param name="motorIndex">Motor index to validate.</param>
	private static void ValidateMotorIndex(int motorIndex)
	{
		if (motorIndex is < 1 or > 2)
		{
			throw new ArgumentOutOfRangeException(nameof(motorIndex), "Explorer HAT exposes two motors (1-2).");
		}
	}
}
