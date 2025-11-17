using System;
using System.Threading;
using Tobot.Device.ExplorerHat;

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
            Console.WriteLine("│  0. Exit                                                │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");
            Console.Write("\nEnter your choice (0-8): ");

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
                case "0":
                    Console.WriteLine("Exiting Explorer HAT Demo. Goodbye!");
                    return;
                default:
                    Console.WriteLine("⚠️  Invalid choice. Please enter 0-8.");
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
            default:
                Console.WriteLine($"Unknown example: {exampleName}");
                Console.WriteLine("Available: led, input, output, analog, motor, touch, robot, check");
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
}
