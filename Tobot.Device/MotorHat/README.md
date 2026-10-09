# MotorKit for .NET

`Tobot.Device.MotorHat.MotorKit` is a C# port of Adafruit's MotorKit helper
for PCA9685-based DC and stepper motor boards, including the Adafruit DC &
Stepper Motor HAT. It uses the PCA9685 driver from `Iot.Device.Bindings`,
already referenced by `Tobot.Device`.

## Requirements

- Raspberry Pi or another supported .NET IoT platform with I2C enabled.
- An Adafruit PCA9685-based Motor HAT/Shield/FeatherWing and a compatible
  motor power supply. The Pi's I2C connection alone does not power the motors.
- The I2C address, which is `0x60` by default. Change it to match the HAT's
  address jumpers if needed.

On Raspberry Pi OS, enable I2C with `sudo raspi-config` under **Interface
Options → I2C**, then reboot if prompted.

## DC motor

Use a separate HAT instance or board for each example. The `using` declaration
disposes the controller at the end of its scope.

```csharp
using Tobot.Device.MotorHat;

using var kit = new MotorKit();
var motor = kit.Motor1;

try
{
    motor.Forward(0.6); // Forward at 60% throttle
    Thread.Sleep(TimeSpan.FromSeconds(1));

    motor.Backward(0.4); // Reverse at 40% throttle
    Thread.Sleep(TimeSpan.FromSeconds(1));

    motor.Brake(); // Actively brake
    Thread.Sleep(TimeSpan.FromMilliseconds(250));
}
finally
{
    motor.Stop(); // Disable the H-bridge outputs (coast)
}
```

The Adafruit-style `Throttle` property is also available:

```csharp
motor.Throttle = 1.0;  // Full forward
motor.Throttle = -0.5; // Half speed backward
motor.Throttle = 0;    // Brake: both motor inputs high
motor.Throttle = null; // Coast: both motor inputs off
```

`Forward` and `Backward` accept speeds from `0.0` to `1.0` and throw
`ArgumentOutOfRangeException` for values outside that range. `Stop()` is
equivalent to setting `Throttle` to `null`; `Brake()` is equivalent to setting
it to `0`.

Select any DC motor with `kit.Motor1` through `kit.Motor4`. To change the
current-decay behavior, set its `DecayMode` to `DecayMode.Fast` (coasting,
the default) or `DecayMode.Slow` (braking-style decay):

```csharp
using Tobot.Device.MotorHat.Motor;

kit.Motor1.DecayMode = DecayMode.Slow;
```

## Stepper motor

Stepper motors share H-bridge channels with pairs of DC motors: stepper 1
shares with motors 1 and 2, and stepper 2 with motors 3 and 4. Once a DC motor
from a pair has been accessed, accessing its stepper throws
`InvalidOperationException`, and vice versa. Use only one motor type per pair
for the lifetime of a `MotorKit` instance.

```csharp
using Tobot.Device.MotorHat;
using Tobot.Device.MotorHat.Motor;

using var kit = new MotorKit();
var stepper = kit.Stepper1;

try
{
    for (var i = 0; i < 100; i++)
    {
        stepper.Onestep(StepperDirection.Forward, StepperStyle.Double);
        Thread.Sleep(10); // Set the pace appropriate for your motor and load.
    }
}
finally
{
    stepper.Release(); // De-energize the coils.
}
```

Available styles are `Single`, `Double`, `Interleave`, and `Microstep`.
Stepper 2 is available as `kit.Stepper2`. The default is 16 microsteps per
full step; configure a different even value of at least 2 when constructing
the kit:

```csharp
using var kit = new MotorKit(steppersMicrosteps: 8);
```

`Release()` de-energizes the coils so the motor can move freely. Dispose the
kit when finished; it releases initialized motors and closes the I2C device.

## Configuration

The defaults match Adafruit MotorKit: I2C bus 1, address `0x60`, and PWM
frequency 1600 Hz. For a board at another address or a different bus:

```csharp
using var kit = new MotorKit(address: 0x61, i2cBusId: 1);
kit.Frequency = 1000;
```

`Frequency` gets or sets the PCA9685's shared PWM frequency. Motors are
initialized lazily when their properties are first accessed.

## API overview

| Member | Description |
| --- | --- |
| `Motor1`–`Motor4` | DC motor controls (`Throttle`, `Forward`, `Backward`, `Brake`, `Stop`, `DecayMode`) |
| `Stepper1`, `Stepper2` | Stepper controls (`Onestep`, `Release`, `CurrentMicrostep`) |
| `Frequency` | Shared PCA9685 PWM frequency in hertz |
| `SteppersMicrosteps` | Configured microsteps per full step |

For upstream library and hardware details, see
[Adafruit_CircuitPython_MotorKit](https://github.com/adafruit/Adafruit_CircuitPython_MotorKit).
Ported MotorKit and motor-control behavior is MIT-licensed; see
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
