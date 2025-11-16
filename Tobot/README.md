# Tobot - Explorer HAT Demo Application

A comprehensive demonstration application for the Pimoroni Explorer HAT on Raspberry Pi using .NET 9.

## Overview

This application provides an interactive menu-driven interface to explore all features of the Explorer HAT including LEDs, digital I/O, analog inputs, motors, and capacitive touch sensors.

## Features

### ?? LED Light Show
- Sequential LED activation
- Synchronized patterns
- Knight Rider chase effect
- Customizable light shows

### ?? Digital Input Monitor
- Real-time input monitoring
- Event-driven change detection
- Timestamped logging
- All 4 inputs supported

### ? Digital Output Control
- Individual output control
- Batch operations
- Toggle functionality
- Pattern generation

### ?? Analog Sensor Reader
- 4-channel ADC reading (0-5V)
- Continuous sampling
- Real-time voltage display
- 12-bit resolution

### ?? Motor Control
- H-bridge motor control
- Variable speed (0-100%)
- Forward/backward movement
- Synchronized dual motor control

### ?? Touch Sensor Demo
- 8 capacitive touch sensors
- Real-time touch detection
- Visual LED feedback
- Batch sensor reading

### ?? Robot Control System
- Complete robot control
- Obstacle detection
- Multi-input control
- LED status indicators

### ?? System Check
- Comprehensive component testing
- Automatic diagnostics
- Status reporting
- Quick verification

## Usage

### Interactive Mode

Simply run the application to access the interactive menu:

```bash
dotnet run --project Tobot
```

### Command Line Mode

Run specific demos directly:

```bash
# LED demo
dotnet run --project Tobot led

# Input monitoring
dotnet run --project Tobot input

# Output control
dotnet run --project Tobot output

# Analog reading
dotnet run --project Tobot analog

# Motor control
dotnet run --project Tobot motor

# Touch sensors
dotnet run --project Tobot touch

# Robot control
dotnet run --project Tobot robot

# System check
dotnet run --project Tobot check
```

## Requirements

- Raspberry Pi (any model with 40-pin GPIO header)
- Pimoroni Explorer HAT
- .NET 9 Runtime
- I2C enabled on Raspberry Pi

## Enable I2C

Before running, ensure I2C is enabled:

```bash
sudo raspi-config
# Navigate to: Interface Options -> I2C -> Enable
sudo reboot
```

## Installation

1. Clone the repository
2. Navigate to the project directory
3. Build the solution:

```bash
dotnet build
```

4. Run the application:

```bash
dotnet run --project Tobot
```

## Code Examples

### Simple LED Control

```csharp
using var hat = new ExplorerHat();

// Individual LED control
hat.Light.One.On();
Thread.Sleep(500);
hat.Light.One.Off();

// All LEDs
hat.Light.On();
```

### Motor Control

```csharp
using var hat = new ExplorerHat();

// Forward movement
hat.Motor.One.Forward(100);
Thread.Sleep(2000);

// Variable speed
hat.Motor.One.SetSpeed(75);  // 75% forward
hat.Motor.One.SetSpeed(-50); // 50% backward

// Stop
hat.Motor.Stop();
```

### Digital I/O

```csharp
using var hat = new ExplorerHat();

// Read input
bool state = hat.Input.One.Read();

// Control output
hat.Output.One.On();
hat.Output.One.Toggle();

// Events
hat.Input.One.Changed += (s, e) => 
    Console.WriteLine($"Input changed: {e.ChangeType}");
```

### Analog Reading

```csharp
using var hat = new ExplorerHat();

// Read voltage
double voltage = hat.Analog.One.Read();
Console.WriteLine($"Voltage: {voltage:F2}V");
```

### Touch Sensors

```csharp
using var hat = new ExplorerHat();

// Check touch
if (hat.Touch[1].IsTouched())
{
    Console.WriteLine("Sensor 1 touched!");
}

// Read all sensors
byte touchState = hat.Touch.ReadAll();
```

## Project Structure

```
Tobot/
??? Program.cs              Main demo application
??? Tobot.csproj           Project configuration
??? README.md              This file
```

## Dependencies

- **Tobot.Device** - Explorer HAT driver library
- **System.Device.Gpio** - GPIO access
- **Iot.Device.Bindings** - I2C device support

## Hardware Pin Mapping

### Digital Inputs (5V tolerant)
- Input 1: GPIO 23
- Input 2: GPIO 22
- Input 3: GPIO 24
- Input 4: GPIO 25

### Digital Outputs
- Output 1: GPIO 6
- Output 2: GPIO 12
- Output 3: GPIO 13
- Output 4: GPIO 16

### LEDs
- LED 1: GPIO 4
- LED 2: GPIO 17
- LED 3: GPIO 27
- LED 4: GPIO 5

### Motors
- Motor 1: Enable GPIO 19, Forward GPIO 20, Backward GPIO 21
- Motor 2: Enable GPIO 26, Forward GPIO 7, Backward GPIO 8

### I2C Devices
- ADS1015 ADC: 0x48
- CAP1208 Touch: 0x28

## Safety Notes

?? **Important Safety Information**

- Ensure motors are properly connected before running motor demos
- Digital outputs can sink up to 500mA per channel
- Analog inputs are 0-5V range only
- Always use proper power supply for motors
- Stop all motors before disconnecting power

## Troubleshooting

### I2C Device Not Found
```bash
# Check I2C devices
i2cdetect -y 1

# Expected devices:
# 0x28 - CAP1208 Touch Controller
# 0x48 - ADS1015 ADC
```

### Permission Denied
```bash
# Add user to GPIO group
sudo usermod -a -G gpio $USER
sudo usermod -a -G i2c $USER

# Logout and login again
```

### GPIO Errors
```bash
# Ensure GPIO permissions
sudo chmod 666 /dev/gpiomem
```

## Contributing

This is a demonstration application. For driver improvements, see the `Tobot.Device` project.

## License

This project uses the Pimoroni Explorer HAT driver for .NET, which is based on the original Python library.

## Support

For issues, questions, or contributions, please refer to the main Tobot.Device documentation.

## Resources

- [Explorer HAT Documentation](https://github.com/pimoroni/explorer-hat)
- [.NET IoT Documentation](https://docs.microsoft.com/en-us/dotnet/iot/)
- [Raspberry Pi GPIO](https://www.raspberrypi.org/documentation/hardware/raspberrypi/)

---

**Tobot Robotics Platform** - Built with ?? for makers and robotics enthusiasts
