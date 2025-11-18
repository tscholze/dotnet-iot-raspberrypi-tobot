# 🤖 Tobot - Explorer HAT for .NET

> **Modern robotics meets modern .NET** - A comprehensive C# driver and demo platform for the Pimoroni Explorer HAT on Raspberry Pi

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 13](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Raspberry Pi](https://img.shields.io/badge/Raspberry%20Pi-Compatible-C51A4A?logo=raspberry-pi)](https://www.raspberrypi.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## ? What is Tobot?

**Tobot** is a complete .NET robotics platform that brings the power of modern C# to the Raspberry Pi and the beloved Pimoroni Explorer HAT. Whether you're building your first robot, teaching programming, or creating a sophisticated autonomous system, Tobot provides everything you need in a clean, intuitive API.

### ✨ Why Tobot?

- **⚡ Modern C#** - Leverage C# 13 and .NET 9 features for robotics
- **📦 Package-Based Architecture** - Logical organization by functionality
- **📚 Comprehensive Documentation** - XML docs on every member, extensive guides
- **🎯 Ready-to-Run Demos** - Interactive examples for every feature
- **✅ Production Ready** - Robust error handling and resource management
- **🎓 Educational** - Perfect for learning robotics and C# together

---

## ⚡ Quick Demo

```csharp
using Tobot.Device.ExplorerHat;

// Initialize the Explorer HAT
using var hat = new ExplorerHat();

// Light show!
hat.Light.On();

// Drive forward
hat.Motor.One.Forward(100);
hat.Motor.Two.Forward(100);

// React to sensors
if (hat.Input.One.Read())
{
    hat.Motor.Stop();
    hat.Light.Off();
}

// Read analog sensors
double voltage = hat.Analog.One.Read();
Console.WriteLine($"Sensor: {voltage:F2}V");

// Touch detection
if (hat.Touch[1].IsTouched())
{
    Console.WriteLine("Button pressed!");
}
```

---

## 🎯 What's Included?

### 📦 Tobot.Device Library

A professional-grade driver library for the Explorer HAT with:

| Package | Components | Description |
|---------|-----------|-------------|
| **🚗 Motor** | `Motor`, `MotorCollection` | H-bridge motor control with variable speed |
| **💡 LED** | `Led`, `LedCollection` | Onboard LED control and patterns |
| **📊 Analog** | `AnalogInput`, `AnalogInputCollection` | 0-5V analog input via ADS1015 ADC |
| **🔌 Digital** | `DigitalInput/Output`, Collections | Digital I/O with event support |
| **👆 Touch** | `TouchSensor`, `TouchCollection` | Capacitive touch via CAP1208 |

### 🎮 Tobot Console Application

An interactive showcase featuring:

- **LED Light Show** - Mesmerizing patterns and effects
- **Input Monitor** - Real-time digital input tracking
- **Output Control** - Power external devices
- **Analog Reader** - Sensor voltage monitoring
- **Motor Control** - Precision movement and speed
- **Touch Demo** - Capacitive touch detection
- **Robot System** - Complete autonomous control
- **System Check** - Hardware diagnostics

### 🌐 Tobot.Web Application

A modern web-based control interface featuring:

- **SignalR Integration** - Real-time bidirectional communication
- **Remote Control** - Control your robot from any device on the network
- **Live Updates** - Receive real-time feedback from all sensors and actuators
- **Interactive UI** - Clean, responsive Blazor interface
- **Event Monitoring** - Track all robot actions in real-time
- **Multi-Device Support** - Access from phones, tablets, or computers

Access the web interface at `http://[raspberry-pi-ip]:5247/simple`

---

## 🚀 Quick Start

### Prerequisites

- Raspberry Pi (any model with 40-pin GPIO)
- Pimoroni Explorer HAT
- .NET 9 SDK

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/tobot.git
cd tobot

# Build the solution
dotnet build

# Run the interactive demo
dotnet run --project Tobot
```

### Your First Robot in 30 Seconds

```bash
# Quick system check
dotnet run --project Tobot check

# LED light show
dotnet run --project Tobot led

# Full robot control
dotnet run --project Tobot robot
```

📖 **Detailed instructions:** See [Tobot/QUICKSTART.md](Tobot/QUICKSTART.md)

---

## 🧰 Scripts

All helper scripts live in `scripts/` at the project root.

- `scripts/run-tobot-web-kiosk.sh`: Starts the `Tobot.Web` Blazor app and opens it in Firefox kiosk mode on the Raspberry Pi at `http://localhost:5247/simple`.

Usage:

```bash
chmod +x scripts/run-tobot-web-kiosk.sh
./scripts/run-tobot-web-kiosk.sh
```

Notes:
- Requires `firefox` (or `firefox-esr`) installed on the Raspberry Pi.
- Binds the web app to `0.0.0.0:5247` so it’s reachable on your LAN.
- Adjust the script if you prefer Chromium (`chromium-browser --kiosk`).

---

## 🏛️ Architecture

Tobot follows a clean, modular architecture:

```
Tobot/
??? ?? Tobot/                          Console demo application
?   ??? Program.cs                     Interactive demos
?   ??? README.md                      Usage guide
?   ??? QUICKSTART.md                  5-minute setup
?
??? ?? Tobot.Device/                   Hardware driver library
    ??? ExplorerHat/                   Explorer HAT components
        ??? ExplorerHat.cs             Main controller
        ??? Motor/                     Motor control package
        ??? Led/                       LED control package
        ??? Analog/                    Analog input package
        ??? Digital/                   Digital I/O package
        ??? Touch/                     Touch sensor package
│
└── 🌐 Tobot.Web/                      Web control interface
    ├── Program.cs                     ASP.NET Core application
    ├── Hubs/                          SignalR hubs
    │   ├── TobotHub.cs                Main control hub
    │   └── TobotHubEvents.cs          Event constants
    └── Components/                    Blazor UI components
        └── Pages/                     Web pages
            └── Simple.razor           Control interface
```

### Key Design Principles

? **Context-Related Packaging** - Grouped by functionality  
? **Self-Contained Packages** - No cross-package dependencies  
? **Clean APIs** - Intuitive, discoverable interfaces  
? **Comprehensive Docs** - XML documentation everywhere  
? **Resource Safety** - IDisposable pattern throughout  

---

## 🎯 Features & Capabilities

### 🚗 Motor Control
```csharp
hat.Motor.One.Forward(100);      // Full speed ahead
hat.Motor.One.SetSpeed(75);      // 75% forward
hat.Motor.One.SetSpeed(-50);     // 50% backward
hat.Motor.Stop();                // Emergency stop
```

### 💡 LED Control
```csharp
hat.Light.One.On();              // Individual LED
hat.Light.On();                  // All LEDs
hat.Light.Two.Toggle();          // Toggle state
```

### 🔌 Digital I/O
```csharp
// Read input
bool state = hat.Input.One.Read();

// Event-driven
hat.Input.One.Changed += (s, e) => 
    Console.WriteLine($"Changed: {e.ChangeType}");

// Control output
hat.Output.One.On();
hat.Output.Toggle();
```

### 📊 Analog Input (0-5V)
```csharp
double voltage = hat.Analog.One.Read();
Console.WriteLine($"Voltage: {voltage:F2}V");
```

### 👆 Capacitive Touch
```csharp
if (hat.Touch[1].IsTouched())
{
    Console.WriteLine("Touched!");
}

byte allSensors = hat.Touch.ReadAll();
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Tobot/README.md](Tobot/README.md) | Demo application guide |
| [Tobot/QUICKSTART.md](Tobot/QUICKSTART.md) | 5-minute setup |
| [Tobot.Device/ExplorerHat/README.md](Tobot.Device/ExplorerHat/README.md) | API reference |
| [Tobot.Device/ExplorerHat/FILE_STRUCTURE.md](Tobot.Device/ExplorerHat/FILE_STRUCTURE.md) | File organization |
| [Tobot.Device/ExplorerHat/PACKAGE_ORGANIZATION.md](Tobot.Device/ExplorerHat/PACKAGE_ORGANIZATION.md) | Package guide |

---

## 📖 Learning Resources

### Example Projects

1. **Line Following Robot**
   ```csharp
   // Use analog sensors to detect line
   double left = hat.Analog.One.Read();
   double right = hat.Analog.Two.Read();
   
   if (left > 2.5) hat.Motor.One.Forward(50);
   if (right > 2.5) hat.Motor.Two.Forward(50);
   ```

2. **Touch-Controlled Light Show**
   ```csharp
   for (int i = 1; i <= 4; i++)
   {
       if (hat.Touch[i].IsTouched())
           hat.Light[i].Toggle();
   }
   ```

3. **Obstacle Avoiding Robot**
   ```csharp
   double distance = hat.Analog.One.Read();
   if (distance > 3.0)
   {
       hat.Motor.Stop();
       hat.Light.On(); // Warning!
   }
   ```

### Code Examples

All demos in `Tobot/Program.cs` are fully commented and ready to modify. Each example is self-contained and demonstrates best practices.

---

## 🔧 Hardware Specifications

### Explorer HAT Features

| Feature | Quantity | Specifications |
|---------|----------|----------------|
| **Digital Inputs** | 4 | 5V tolerant, buffered |
| **Digital Outputs** | 4 | 500mA sink-to-ground |
| **Analog Inputs** | 4 | 0-5V, 12-bit ADC (ADS1015) |
| **Motor Drivers** | 2 | H-bridge, PWM capable |
| **Onboard LEDs** | 4 | Status indicators |
| **Touch Sensors** | 8 | Capacitive (CAP1208) |

### Pin Mapping

<details>
<summary>🔍 Click to view complete pin mapping</summary>

#### Digital Inputs (BCM GPIO)
- Input 1: GPIO 23
- Input 2: GPIO 22
- Input 3: GPIO 24
- Input 4: GPIO 25

#### Digital Outputs (BCM GPIO)
- Output 1: GPIO 6
- Output 2: GPIO 12
- Output 3: GPIO 13
- Output 4: GPIO 16

#### LEDs (BCM GPIO)
- LED 1: GPIO 4
- LED 2: GPIO 17
- LED 3: GPIO 27
- LED 4: GPIO 5

#### Motors (BCM GPIO)
- Motor 1: Enable 19, Forward 20, Backward 21
- Motor 2: Enable 26, Forward 7, Backward 8

#### I2C Devices
- ADS1015 ADC: Address 0x48
- CAP1208 Touch: Address 0x28

</details>

---

## ⚙️ Advanced Usage

### Custom Robot Control Loop

```csharp
using var hat = new ExplorerHat();

// Setup
hat.Light.Off();
hat.Motor.Stop();

// Main control loop
while (true)
{
    // Read sensors
    bool goButton = hat.Input.One.Read();
    bool stopButton = hat.Input.Two.Read();
    double frontSensor = hat.Analog.One.Read();
    
    // Decision logic
    if (stopButton || frontSensor > 3.0)
    {
        // Emergency stop
        hat.Motor.Stop();
        hat.Light.One.On();
    }
    else if (goButton)
    {
        // Move forward
        hat.Motor.One.Forward(80);
        hat.Motor.Two.Forward(80);
        hat.Light.Two.On();
    }
    else
    {
        // Idle
        hat.Motor.Stop();
        hat.Light.Off();
    }
    
    await Task.Delay(50); // 20Hz update rate
}
```

### Async/Await Support

```csharp
public async Task MonitorSensorsAsync(CancellationToken ct)
{
    using var hat = new ExplorerHat();
    
    while (!ct.IsCancellationRequested)
    {
        var voltage = hat.Analog.One.Read();
        Console.WriteLine($"Sensor: {voltage:F2}V");
        
        await Task.Delay(100, ct);
    }
}
```

---

## 🤝 Contributing

This project welcome contributions! Whether it's:

- 🐛 Bug reports
- 💡 Feature requests  
- 📝 Documentation improvements
- 💻 Code examples
- 🔧 Driver enhancements

Please note, that I am developing this project for my self and there is no intend to make it a "market product" in sense of warranty, liability, etc.

For more, please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 🗺️ Roadmap

### Current Features
- Complete Explorer HAT driver
- Interactive demo application
- Comprehensive documentation
- Package-based architecture

### 🔮 Planned Features 
- [X] PWM motor speed control
- [ ] Advanced pattern library
- [ ] Configuration system
- [ ] Logging framework
- [ ] Unit test coverage
- [ ] CI/CD pipeline

---

## 💻 Why .NET for Robotics?

### Modern Language Features
- **Pattern Matching** - Clean state machine logic
- **Async/Await** - Non-blocking sensor reading
- **LINQ** - Elegant data processing
- **Strong Typing** - Catch errors at compile time

### Excellent Tooling
- **Visual Studio / VS Code** - World-class IDEs
- **IntelliSense** - Discover APIs as you code
- **Debugging** - Full breakpoint support
- **Package Management** - NuGet ecosystem

### Performance
- **Native ARM** - Optimized for Raspberry Pi
- **Efficient Memory** - Garbage collection tuned for IoT
- **Low Latency** - Real-time control capable

---

## 🙏 Acknowledgments

- **Pimoroni** - For creating the amazing Explorer HAT hardware
- **.NET Team** - For bringing .NET to ARM/IoT devices
- **Open Source Community** - For inspiration and support

---

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

## 🔗 Links

- **Hardware**: [Pimoroni Explorer HAT](https://shop.pimoroni.com/products/explorer-hat)
- **Documentation**: [.NET IoT Libraries](https://github.com/dotnet/iot)
- **Community**: [Raspberry Pi Forums](https://forums.raspberrypi.com/)
- **Support**: [Open an Issue](https://github.com/yourusername/tobot/issues)

---

## 🚀 Get Started Now!

```bash
git clone https://github.com/yourusername/tobot.git
cd tobot
dotnet run --project Tobot
```

**Ready to build something amazing?** The future of robotics is .NET! ???

---

<div align="center">

**Made with ❤️ for makers, educators, and robotics enthusiasts**

[⭐ Star this repo](https://github.com/yourusername/tobot) | [📚 Read the docs](Tobot/README.md) | [🚀 Quick start](Tobot/QUICKSTART.md)

</div>
