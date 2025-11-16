# Explorer HAT for .NET 9

A modern C# implementation of the Pimoroni Explorer HAT driver for Raspberry Pi.

## Overview

The Explorer HAT is a versatile add-on board for Raspberry Pi that provides:

- **4 Buffered Inputs** - 5V tolerant digital inputs
- **4 Digital Outputs** - Sink-to-ground outputs capable of driving small loads
- **4 Analog Inputs** - 0-5V analog inputs via ADS1015 ADC
- **2 H-Bridge Motor Drivers** - Control two DC motors
- **8 Capacitive Touch Sensors** - Via CAP1208 controller
- **4 Onboard LEDs** - For status indication

## Installation

The library requires the following NuGet packages:
```bash
dotnet add package System.Device.Gpio --version 4.0.1
dotnet add package Iot.Device.Bindings --version 4.0.1
```

## Usage

### Basic Initialization

```csharp
using Tobot.Device.ExplorerHat;

// Create a new Explorer HAT instance
using var hat = new ExplorerHat();
```

### LED Control

```csharp
// Control individual LEDs
hat.Light.One.On();
hat.Light.Two.Off();
hat.Light.Three.Toggle();

// Control all LEDs at once
hat.Light.On();
hat.Light.Off();

// Access by index (1-based)
hat.Light[1].On();
```

### Digital Inputs

```csharp
// Read input state
bool state = hat.Input.One.Read();

// Register event handler
hat.Input.One.Changed += (sender, args) =>
{
    Console.WriteLine($"Input changed: {args.ChangeType}");
};

// Access all inputs
var input1 = hat.Input.One;
var input2 = hat.Input.Two;
var input3 = hat.Input.Three;
var input4 = hat.Input.Four;
```

### Digital Outputs

```csharp
// Control individual outputs
hat.Output.One.On();
hat.Output.Two.Off();
hat.Output.Three.Toggle();

// Write boolean value
hat.Output.One.Write(true);

// Control all outputs
hat.Output.On();
hat.Output.Off();
```

### Analog Inputs

```csharp
// Read analog voltage (0-5V)
double voltage = hat.Analog.One.Read();
Console.WriteLine($"Voltage: {voltage:F2}V");

// Read multiple channels
double v1 = hat.Analog.One.Read();
double v2 = hat.Analog.Two.Read();
double v3 = hat.Analog.Three.Read();
double v4 = hat.Analog.Four.Read();
```

### Motor Control

```csharp
// Move forward at full speed
hat.Motor.One.Forward(100);

// Move backward at half speed
hat.Motor.Two.Backward(50);

// Precise speed control (-100 to +100)
hat.Motor.One.SetSpeed(75);   // Forward at 75%
hat.Motor.Two.SetSpeed(-50);  // Backward at 50%

// Stop motors
hat.Motor.One.Stop();
hat.Motor.Stop(); // Stop all motors
```

### Capacitive Touch

```csharp
// Read individual touch sensor
if (hat.Touch[1].IsTouched())
{
    Console.WriteLine("Sensor 1 touched!");
}

// Read all sensors at once
byte touchState = hat.Touch.ReadAll();
```

## Hardware Pin Mappings

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
**Motor 1:**
- Enable: GPIO 19
- Forward: GPIO 20
- Backward: GPIO 21

**Motor 2:**
- Enable: GPIO 26
- Forward: GPIO 7
- Backward: GPIO 8

### I2C Devices
- ADS1015 ADC: Address 0x48
- CAP1208 Touch Sensor: Address 0x28

## API Reference

### Main Class: ExplorerHat

#### Properties
- `Input` - Collection of 4 digital inputs
- `Output` - Collection of 4 digital outputs
- `Analog` - Collection of 4 analog inputs
- `Motor` - Collection of 2 motors
- `Touch` - Collection of 8 touch sensors
- `Light` - Collection of 4 LEDs

#### Constructor
```csharp
public ExplorerHat(
    GpioController? gpioController = null,
    int i2cBusId = 1)
```

### Collections

All collections support:
- Index access (1-based): `collection[1]`
- Named properties: `collection.One`, `collection.Two`, etc.

## Examples

See `ExplorerHatExample.cs` for complete examples including:
- LED control
- Input reading with events
- Output control
- Analog input reading
- Motor control
- Touch sensor reading
- Complete robot control example

## Requirements

- .NET 9.0 or later
- Raspberry Pi with Explorer HAT hardware
- Linux OS (typically Raspberry Pi OS)
- I2C must be enabled on the Raspberry Pi

## Enable I2C on Raspberry Pi

```bash
sudo raspi-config
# Navigate to: Interface Options -> I2C -> Enable
# Reboot the Pi
```

## License

This implementation is based on the Pimoroni Explorer HAT Python library.

## Notes

- All pin numbering uses BCM (Broadcom) GPIO numbering
- Motor PWM control is simplified in this implementation
- Proper disposal is important - always use `using` statements
- The library is thread-safe for individual component access
