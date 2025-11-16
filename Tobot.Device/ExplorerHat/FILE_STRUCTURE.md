# Explorer HAT File Structure

The Explorer HAT implementation has been organized into context-related sub-packages for better maintainability and logical grouping.

## Package Organization

### Root Level (`Tobot.Device.ExplorerHat`)

#### `ExplorerHat.cs`
The main entry point class that orchestrates all hardware components.
- Manages I2C devices (ADS1015 ADC and CAP1208 touch sensor)
- Manages GPIO controller
- Initializes all component collections
- Implements IDisposable for proper resource cleanup

#### `ExplorerHatExample.cs`
Comprehensive usage examples for all components.

### Motor Package (`Tobot.Device.ExplorerHat.Motor`)

Contains all motor-related functionality:

#### `Motor.cs`
Represents a single H-bridge motor driver.
- Forward/Backward control
- Speed control (-100 to +100)
- H-bridge pin management

#### `MotorCollection.cs`
Manages the 2 motor drivers.
- Index-based access (1-2)
- Named properties (One, Two)
- Emergency stop all motors

#### `MotorPinMapping.cs`
Internal record type for motor pin configuration.
- Enable pin (PWM)
- Forward direction pin
- Backward direction pin

### LED Package (`Tobot.Device.ExplorerHat.Led`)

Contains all LED-related functionality:

#### `Led.cs`
Represents a single onboard LED.
- On/Off control
- Toggle functionality
- State management

#### `LedCollection.cs`
Manages the 4 onboard LEDs.
- Index-based access (1-4)
- Named properties (One, Two, Three, Four)
- Batch operations (On All/Off All)

### Analog Package (`Tobot.Device.ExplorerHat.Analog`)

Contains all analog input functionality:

#### `AnalogInput.cs`
Represents a single analog input channel (0-5V).
- Uses ADS1015 12-bit ADC
- Returns voltage readings
- I2C communication handling

#### `AnalogInputCollection.cs`
Manages the 4 analog input channels.
- Index-based access (1-4)
- Named properties (One, Two, Three, Four)

### Digital Package (`Tobot.Device.ExplorerHat.Digital`)

Contains all digital input and output functionality:

#### `DigitalInput.cs`
Represents a single 5V-tolerant digital input.
- Read input state
- Event support for pin changes
- GPIO resource management

#### `DigitalInputCollection.cs`
Manages the 4 digital inputs.
- Index-based access (1-4)
- Named properties (One, Two, Three, Four)

#### `DigitalOutput.cs`
Represents a single sink-to-ground digital output.
- On/Off control
- Toggle functionality
- GPIO resource management

#### `DigitalOutputCollection.cs`
Manages the 4 digital outputs.
- Index-based access (1-4)
- Named properties (One, Two, Three, Four)
- Batch operations (On All/Off All)

### Touch Package (`Tobot.Device.ExplorerHat.Touch`)

Contains all capacitive touch sensor functionality:

#### `TouchSensor.cs`
Represents a single capacitive touch sensor.
- Touch state detection
- CAP1208 communication

#### `TouchCollection.cs`
Manages the 8 capacitive touch sensors.
- Index-based access (1-8)
- Read all sensors at once
- CAP1208 controller management

### Documentation Files

#### `README.md`
Complete documentation covering:
- Hardware overview
- Installation instructions
- API reference
- Pin mappings
- Usage examples
- Requirements and setup

## Directory Structure

```
ExplorerHat/
??? ExplorerHat.cs                    (Main controller)
??? ExplorerHatExample.cs             (Usage examples)
??? README.md                         (Documentation)
??? FILE_STRUCTURE.md                 (This file)
?
??? Motor/                            (Motor package)
?   ??? Motor.cs
?   ??? MotorCollection.cs
?   ??? MotorPinMapping.cs
?
??? Led/                              (LED package)
?   ??? Led.cs
?   ??? LedCollection.cs
?
??? Analog/                           (Analog input package)
?   ??? AnalogInput.cs
?   ??? AnalogInputCollection.cs
?
??? Digital/                          (Digital I/O package)
?   ??? DigitalInput.cs
?   ??? DigitalInputCollection.cs
?   ??? DigitalOutput.cs
?   ??? DigitalOutputCollection.cs
?
??? Touch/                            (Touch sensor package)
    ??? TouchSensor.cs
    ??? TouchCollection.cs
```

## Benefits of This Structure

1. **Logical Grouping**: Related components are grouped together by functionality
2. **Clear Separation of Concerns**: Each package has a single, well-defined purpose
3. **Easier Navigation**: Developers can quickly find functionality by category
4. **Better IDE Support**: IntelliSense groups related types together
5. **Namespace Organization**: Clear namespace hierarchy mirrors physical structure
6. **Scalability**: Easy to add new functionality within existing packages
7. **Reduced Cognitive Load**: Smaller, focused packages are easier to understand
8. **Test Organization**: Tests can be organized by package

## Package Dependencies

```
Tobot.Device.ExplorerHat (Main)
??? uses: Motor package
??? uses: Led package
??? uses: Analog package
??? uses: Digital package
??? uses: Touch package

Each package is independent and self-contained with no cross-package dependencies.
```

## Usage Example

```csharp
using Tobot.Device.ExplorerHat;

// Main namespace import is sufficient for all functionality
using var hat = new ExplorerHat();

// All sub-components are accessible through the main object
hat.Motor.One.Forward(100);
hat.Light.On();
hat.Input.One.Read();
hat.Analog.One.Read();
hat.Touch[1].IsTouched();
```

The sub-namespaces are used internally for organization but don't require explicit imports for normal usage.

## Total Files

- **1** Main controller class (root)
- **1** Examples file (root)
- **2** Documentation files (root)
- **3** Motor package files
- **2** LED package files
- **2** Analog package files
- **4** Digital package files
- **2** Touch package files

**Total: 17 files** organized into 6 logical packages

Each file is focused, maintainable, and averages ~50-100 lines of code. Related functionality is grouped together making the codebase intuitive to navigate.
