using System;
using System.Threading;
using Tobot.Device.ExplorerHat;
using Tobot.Device.HcSr04;

namespace Tobot;

/// <summary>
/// Main program demonstrating the Explorer HAT functionality for Raspberry Pi.
/// This example showcases all major features including LEDs, inputs, outputs, motors, analog inputs, and touch sensors.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Pimoroni Explorer HAT - Interactive Demo             ║");
        Console.WriteLine("║     Tobot Robotics Platform - .NET 9                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        if (args.Length == 0)
        {
            ShowMenu();
        }
        else
        {
            RunExample(args[0]);
        }
    }

    /// <summary>
    /// Displays the interactive menu for selecting demo examples.
    /// </summary>
    static void ShowMenu()
    {
        while (true)
        {
            Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  Select an Explorer HAT Demo:                          │");
            Console.WriteLine("├─────────────────────────────────────────────────────────┤");
            Console.WriteLine("│  1. LED Light Show          - Onboard LED patterns     │");
            Console.WriteLine("│  2. Digital Input Monitor   - Read input states        │");
            Console.WriteLine("│  3. Digital Output Control  - Control outputs          │");
            Console.WriteLine("│  4. Analog Sensor Reader    - Read analog voltages     │");
            Console.WriteLine("│  5. Motor Control Demo      - Drive motors             │");
            Console.WriteLine("│  6. Touch Sensor Demo       - Capacitive touch         │");
            Console.WriteLine("│  7. Robot Control System    - Complete robot control   │");
            Console.WriteLine("│  8. System Status Check     - Test all components      │");
            Console.WriteLine("│  9. Pan-Tilt HAT Demo       - Move pan & tilt servos   │");
            Console.WriteLine("│ 10. HC-SR04 Distance Demo   - Ultrasonic range test    │");
            Console.WriteLine("│  0. Exit                                                │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");
            Console.Write("\nEnter your choice (0-10): ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RunLedDemo();
                    break;
                case "2":
                    RunInputMonitor();
                    break;
                case "3":
                    RunOutputControl();
                    break;
                case "4":
                    RunAnalogReader();
                    break;
                case "5":
                    RunMotorDemo();
                    break;
                case "6":
                    RunTouchDemo();
                    break;
                case "7":
                    RunRobotControl();
                    break;
                case "8":
                    RunSystemCheck();
                    break;
                case "9":
                    RunPanTiltDemo();
                    break;
                case "10":
                    RunHcSr04Demo();
                    break;
                case "0":
                    Console.WriteLine("Exiting Explorer HAT Demo. Goodbye!");
                    return;
                default:
                    Console.WriteLine("⚠️  Invalid choice. Please enter 0-10.");
                    break;
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    /// <summary>
    /// Runs a specific example by name from command line arguments.
    /// </summary>
    static void RunExample(string exampleName)
    {
        switch (exampleName.ToLower())
        {
            case "led":
                RunLedDemo();
                break;
            case "input":
                RunInputMonitor();
                break;
            case "output":
                RunOutputControl();
                break;
            case "analog":
                RunAnalogReader();
                break;
            case "motor":
                RunMotorDemo();
                break;
            case "touch":
                RunTouchDemo();
                break;
            case "robot":
                RunRobotControl();
                break;
            case "check":
                RunSystemCheck();
                break;
            case "pantilt":
                RunPanTiltDemo();
                break;
            case "hcsr04":
            case "ultrasonic":
                RunHcSr04Demo();
                break;
            default:
                Console.WriteLine($"Unknown example: {exampleName}");
                Console.WriteLine("Available: led, input, output, analog, motor, touch, robot, check, pantilt, hcsr04");
                break;
        }
    }

    /// <summary>
    /// Demonstrates LED control with various light patterns and effects.
    /// Shows sequential lighting, simultaneous control, and chase patterns.
    /// </summary>
    static void RunLedDemo()
    {
        Console.WriteLine("🔆 LED Light Show Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Demonstrating onboard LED control...\n");

        try
        {
            using var hat = new ExplorerHat();

            // Sequential LED activation
            Console.WriteLine("▶ Sequential lighting...");
            hat.Light.One.On();
            Console.WriteLine("  LED 1: ON");
            Thread.Sleep(300);
            
            hat.Light.Two.On();
            Console.WriteLine("  LED 2: ON");
            Thread.Sleep(300);
            
            hat.Light.Three.On();
            Console.WriteLine("  LED 3: ON");
            Thread.Sleep(300);
            
            hat.Light.Four.On();
            Console.WriteLine("  LED 4: ON");
            Thread.Sleep(1000);
            
            // All LEDs on
            Console.WriteLine("\n▶ All LEDs on...");
            hat.Light.On();
            Thread.Sleep(1000);
            
            // All LEDs off
            Console.WriteLine("▶ All LEDs off...");
            hat.Light.Off();
            Thread.Sleep(500);
            
            // Knight Rider style chase
            Console.WriteLine("\n▶ Knight Rider chase pattern...");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                for (int i = 1; i <= 4; i++)
                {
                    hat.Light[i].On();
                    Thread.Sleep(100);
                    hat.Light[i].Off();
                }
                for (int i = 3; i >= 2; i--)
                {
                    hat.Light[i].On();
                    Thread.Sleep(100);
                    hat.Light[i].Off();
                }
            }
            
            // Blink all
            Console.WriteLine("\n▶ Synchronized blinking...");
            for (int i = 0; i < 5; i++)
            {
                hat.Light.On();
                Thread.Sleep(200);
                hat.Light.Off();
                Thread.Sleep(200);
            }
            
            Console.WriteLine("\n✅ LED demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Monitors digital inputs and displays their state in real-time.
    /// Demonstrates event-driven input handling and state polling.
    /// </summary>
    static void RunInputMonitor()
    {
        Console.WriteLine("🔌 Digital Input Monitor");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Monitoring digital inputs (Press Ctrl+C to stop)...\n");

        try
        {
            using var hat = new ExplorerHat();

            // Setup event handlers
            hat.Input.One.Changed += (s, e) => 
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 1: {e.ChangeType}");
            hat.Input.Two.Changed += (s, e) => 
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 2: {e.ChangeType}");
            hat.Input.Three.Changed += (s, e) => 
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 3: {e.ChangeType}");
            hat.Input.Four.Changed += (s, e) => 
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 4: {e.ChangeType}");

            Console.WriteLine("📊 Initial States:");
            Console.WriteLine($"  Input 1: {(hat.Input.One.Read() ? "HIGH" : "LOW")}");
            Console.WriteLine($"  Input 2: {(hat.Input.Two.Read() ? "HIGH" : "LOW")}");
            Console.WriteLine($"  Input 3: {(hat.Input.Three.Read() ? "HIGH" : "LOW")}");
            Console.WriteLine($"  Input 4: {(hat.Input.Four.Read() ? "HIGH" : "LOW")}");
            Console.WriteLine("\n👂 Listening for changes...\n");

            // Keep monitoring (would normally use cancellation token)
            Thread.Sleep(30000); // Monitor for 30 seconds
            
            Console.WriteLine("\n✅ Input monitoring stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates digital output control with various switching patterns.
    /// Shows individual control, batch operations, and toggle functionality.
    /// </summary>
    static void RunOutputControl()
    {
        Console.WriteLine("⚡ Digital Output Control Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Controlling digital outputs...\n");

        try
        {
            using var hat = new ExplorerHat();

            // Individual output control
            Console.WriteLine("▶ Testing individual outputs...");
            for (int i = 1; i <= 4; i++)
            {
                Console.WriteLine($"  Output {i}: ON");
                hat.Output[i].On();
                Thread.Sleep(500);
                
                Console.WriteLine($"  Output {i}: OFF");
                hat.Output[i].Off();
                Thread.Sleep(300);
            }
            
            // All outputs on
            Console.WriteLine("\n▶ All outputs ON...");
            hat.Output.On();
            Thread.Sleep(1000);
            
            // All outputs off
            Console.WriteLine("▶ All outputs OFF...");
            hat.Output.Off();
            Thread.Sleep(500);
            
            // Toggle demonstration
            Console.WriteLine("\n▶ Toggle demonstration...");
            for (int i = 0; i < 6; i++)
            {
                hat.Output.One.Toggle();
                hat.Output.Three.Toggle();
                Thread.Sleep(250);
                
                hat.Output.Two.Toggle();
                hat.Output.Four.Toggle();
                Thread.Sleep(250);
            }
            
            // Cleanup
            hat.Output.Off();
            
            Console.WriteLine("\n✅ Output control demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and displays analog input voltages from the ADS1015 ADC.
    /// Demonstrates continuous sampling and voltage display.
    /// </summary>
    static void RunAnalogReader()
    {
        Console.WriteLine("📊 Analog Sensor Reader");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Reading analog inputs (0-5V range)...\n");

        try
        {
            using var hat = new ExplorerHat();

            Console.WriteLine("Sampling 20 readings from all channels:\n");
            
            for (int i = 0; i < 20; i++)
            {
                double v1 = hat.Analog.One.Read();
                double v2 = hat.Analog.Two.Read();
                double v3 = hat.Analog.Three.Read();
                double v4 = hat.Analog.Four.Read();
                
                Console.Write($"\r[{i + 1:00}/20] ");
                Console.Write($"A1: {v1:F3}V │ ");
                Console.Write($"A2: {v2:F3}V │ ");
                Console.Write($"A3: {v3:F3}V │ ");
                Console.Write($"A4: {v4:F3}V");
                
                Thread.Sleep(200);
            }
            
            Console.WriteLine("\n\n✅ Analog reading complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates motor control with various movement patterns.
    /// Shows forward, backward, speed control, and synchronized movement.
    /// </summary>
    static void RunMotorDemo()
    {
        Console.WriteLine("🤖 Motor Control Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Testing motor drivers...\n");

        try
        {
            using var hat = new ExplorerHat();

            // Motor 1 forward
            Console.WriteLine("▶ Motor 1: Forward (100%)");
            hat.Motor.One.Forward(100);
            Thread.Sleep(2000);
            
            Console.WriteLine("▶ Motor 1: Stop");
            hat.Motor.One.Stop();
            Thread.Sleep(500);
            
            // Motor 1 backward
            Console.WriteLine("▶ Motor 1: Backward (50%)");
            hat.Motor.One.Backward(50);
            Thread.Sleep(2000);
            
            Console.WriteLine("▶ Motor 1: Stop");
            hat.Motor.One.Stop();
            Thread.Sleep(500);
            
            // Motor 2 speed control
            Console.WriteLine("\n▶ Motor 2: Variable speed");
            for (int speed = 0; speed <= 100; speed += 25)
            {
                Console.WriteLine($"  Speed: {speed}%");
                hat.Motor.Two.SetSpeed(speed);
                Thread.Sleep(1000);
            }
            
            // Both motors synchronized
            Console.WriteLine("\n▶ Both motors: Forward (75%)");
            hat.Motor.One.Forward(75);
            hat.Motor.Two.Forward(75);
            Thread.Sleep(2000);
            
            Console.WriteLine("▶ Both motors: Stop");
            hat.Motor.Stop();
            
            Console.WriteLine("\n✅ Motor demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates capacitive touch sensor reading and response.
    /// Shows individual sensor detection and LED feedback.
    /// </summary>
    static void RunTouchDemo()
    {
        Console.WriteLine("👆 Touch Sensor Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Testing capacitive touch sensors...\n");
        Console.WriteLine("Touch any sensor (monitoring for 15 seconds)...\n");

        try
        {
            using var hat = new ExplorerHat();

            DateTime startTime = DateTime.Now;
            
            while ((DateTime.Now - startTime).TotalSeconds < 15)
            {
                // Read all touch sensors
                byte touchState = hat.Touch.ReadAll();
                
                // Check individual sensors and light corresponding LEDs
                for (int i = 1; i <= 4; i++)
                {
                    if (hat.Touch[i].IsTouched())
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Touch sensor {i} detected!");
                        hat.Light[i].On();
                    }
                    else
                    {
                        hat.Light[i].Off();
                    }
                }
                
                Thread.Sleep(50);
            }
            
            // Cleanup
            hat.Light.Off();
            
            Console.WriteLine("\n✅ Touch sensor demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete robot control system demonstration.
    /// Integrates inputs, motors, LEDs, and analog sensors for autonomous control.
    /// </summary>
    static void RunRobotControl()
    {
        Console.WriteLine("🤖 Robot Control System");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Complete robot control demonstration\n");
        Console.WriteLine("Controls:");
        Console.WriteLine("  Input 1: Move Forward");
        Console.WriteLine("  Input 2: Move Backward");
        Console.WriteLine("  Input 3: Turn Left");
        Console.WriteLine("  Input 4: Turn Right");
        Console.WriteLine("  Analog 1: Obstacle detection (>3V = obstacle)");
        Console.WriteLine("\nPress Ctrl+C to exit\n");

        try
        {
            using var hat = new ExplorerHat();

            while (true)
            {
                // Read inputs
                bool forward = hat.Input.One.Read();
                bool backward = hat.Input.Two.Read();
                bool left = hat.Input.Three.Read();
                bool right = hat.Input.Four.Read();
                
                // Check for obstacles
                double obstacleDistance = hat.Analog.One.Read();
                bool obstacleDetected = obstacleDistance > 3.0;
                
                if (obstacleDetected)
                {
                    // Obstacle detected - stop and alert
                    hat.Motor.Stop();
                    hat.Light.One.Toggle();
                    hat.Output.One.On();
                    Console.WriteLine("⚠️  OBSTACLE DETECTED! Stopping...");
                }
                else if (forward)
                {
                    // Move forward
                    hat.Motor.One.Forward(80);
                    hat.Motor.Two.Forward(80);
                    hat.Light.One.On();
                    hat.Light.Two.On();
                    hat.Light.Three.Off();
                    hat.Light.Four.Off();
                    hat.Output.One.Off();
                    Console.Write("\r▶ Moving FORWARD    ");
                }
                else if (backward)
                {
                    // Move backward
                    hat.Motor.One.Backward(80);
                    hat.Motor.Two.Backward(80);
                    hat.Light.One.Off();
                    hat.Light.Two.Off();
                    hat.Light.Three.On();
                    hat.Light.Four.On();
                    hat.Output.One.Off();
                    Console.Write("\r◀ Moving BACKWARD   ");
                }
                else if (left)
                {
                    // Turn left
                    hat.Motor.One.Backward(60);
                    hat.Motor.Two.Forward(60);
                    hat.Light.One.On();
                    hat.Light.Four.Off();
                    hat.Output.One.Off();
                    Console.Write("\r↺ Turning LEFT      ");
                }
                else if (right)
                {
                    // Turn right
                    hat.Motor.One.Forward(60);
                    hat.Motor.Two.Backward(60);
                    hat.Light.Two.On();
                    hat.Light.Three.Off();
                    hat.Output.One.Off();
                    Console.Write("\r↻ Turning RIGHT     ");
                }
                else
                {
                    // Stop
                    hat.Motor.Stop();
                    hat.Light.Off();
                    hat.Output.Off();
                    Console.Write("\r■ STOPPED           ");
                }
                
                Thread.Sleep(50);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs a comprehensive system check of all Explorer HAT components.
    /// Tests connectivity and basic functionality of all subsystems.
    /// </summary>
    static void RunSystemCheck()
    {
        Console.WriteLine("🔧 Explorer HAT System Check");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Testing all components...\n");

        try
        {
            using var hat = new ExplorerHat();

            // Test LEDs
            Console.Write("Testing LEDs...          ");
            hat.Light.On();
            Thread.Sleep(500);
            hat.Light.Off();
            Console.WriteLine("✅ OK");

            // Test Outputs
            Console.Write("Testing Outputs...       ");
            hat.Output.On();
            Thread.Sleep(500);
            hat.Output.Off();
            Console.WriteLine("✅ OK");

            // Test Inputs
            Console.Write("Testing Inputs...        ");
            bool i1 = hat.Input.One.Read();
            bool i2 = hat.Input.Two.Read();
            Console.WriteLine("✅ OK");

            // Test Analog
            Console.Write("Testing Analog ADC...    ");
            double v1 = hat.Analog.One.Read();
            Console.WriteLine("✅ OK");

            // Test Touch
            Console.Write("Testing Touch Sensors... ");
            byte touch = hat.Touch.ReadAll();
            Console.WriteLine("✅ OK");

            // Test Motors
            Console.Write("Testing Motors...        ");
            hat.Motor.One.Forward(50);
            Thread.Sleep(500);
            hat.Motor.Stop();
            Console.WriteLine("✅ OK");

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║  System Check Complete - All OK! ✅  ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ System Check Failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Streams HC-SR04 distance measurements to the console.
    /// </summary>
    static void RunHcSr04Demo()
    {
        Console.WriteLine("📐 HC-SR04 Distance Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Polling the ultrasonic range finder...\n");

        const int delayMs = 500;
        const int samplesPerReading = 5;

        Console.WriteLine("Update the pin numbers above to match your wiring.\n");

        try
        {
            using var sensor = new HcSr04Sensor();

            for (int i = 0; i < iterations; i++)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                if (sensor.TryReadDistance(out double distanceCm, samplesPerReading))
                {
                    Console.WriteLine($"[{timestamp}] Distance: {distanceCm:F1} cm");
                }
                else
                {
                    Console.WriteLine($"[{timestamp}] ⚠️  Measurement failed. Ensure the target is within range.");
                }

                Thread.Sleep(delayMs);
            }

            Console.WriteLine("\n✅ HC-SR04 demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing HC-SR04 sensor: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the Pan-Tilt HAT by sweeping pan and tilt angles.
    /// Requires a Pimoroni Pan-Tilt HAT connected via I2C (default address 0x15).
    /// </summary>
    static void RunPanTiltDemo()
    {
        Console.WriteLine("🎯 Pan-Tilt HAT Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Demonstrating pan and tilt servo control...\n");

        try
        {
            // Create PanTiltHat with default settings (2 second idle timeout)
            using var panTilt = new Tobot.Device.PanTiltHat.PanTiltHat();

            // Center position (0°, 0°)
            Console.WriteLine("▶ Centering servos...");
            panTilt.Pan(0);
            panTilt.Tilt(0);
            Thread.Sleep(1000);

            // Sweep pan: -60° -> +60°
            Console.WriteLine("\n▶ Pan sweep: -60° to +60°");
            for (int a = -60; a <= 60; a += 15)
            {
                panTilt.Pan(a);
                Console.WriteLine($"  Pan: {a}° (current: {panTilt.CurrentPanAngle}°)");
                Thread.Sleep(250);
            }
            
            // Return to center
            Console.WriteLine("\n▶ Returning to center...");
            for (int a = 60; a >= 0; a -= 15)
            {
                panTilt.Pan(a);
                Thread.Sleep(200);
            }

            // Sweep tilt: -30° -> +30°
            Console.WriteLine("\n▶ Tilt sweep: -30° to +30°");
            for (int a = -30; a <= 30; a += 10)
            {
                panTilt.Tilt(a);
                Console.WriteLine($"  Tilt: {a}° (current: {panTilt.CurrentTiltAngle}°)");
                Thread.Sleep(300);
            }
            
            // Return to center
            Console.WriteLine("\n▶ Returning to center...");
            for (int a = 30; a >= 0; a -= 10)
            {
                panTilt.Tilt(a);
                Thread.Sleep(200);
            }

            // Combined movement demonstration
            Console.WriteLine("\n▶ Combined movement pattern");
            panTilt.Pan(-45);
            panTilt.Tilt(20);
            Console.WriteLine("  Position: (-45°, 20°)");
            Thread.Sleep(600);
            
            panTilt.Pan(45);
            panTilt.Tilt(-20);
            Console.WriteLine("  Position: (45°, -20°)");
            Thread.Sleep(600);
            
            panTilt.Pan(0);
            panTilt.Tilt(0);
            Console.WriteLine("  Position: (0°, 0°) - Centered");
            Thread.Sleep(500);

            // Read back current positions
            Console.WriteLine("\n▶ Reading positions from device...");
            var currentPan = panTilt.GetPan();
            var currentTilt = panTilt.GetTilt();
            Console.WriteLine($"  Reported Pan: {currentPan}°");
            Console.WriteLine($"  Reported Tilt: {currentTilt}°");

            Console.WriteLine($"\n▶ Idle timeout: {panTilt.IdleTimeout} seconds");
            Console.WriteLine("  (Servos will auto-disable after inactivity)\n");

            Console.WriteLine("✅ Pan-Tilt demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
