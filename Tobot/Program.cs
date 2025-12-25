using System.Device.Gpio;
using Tobot.Device;

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
        Console.WriteLine("║     Pimoroni Explorer HAT - Interactive Demo              ║");
        Console.WriteLine("║     Tobot Robotics Platform - .NET 10                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        using var controller = new TobotController();

        if (args.Length == 0)
        {
            ShowMenu(controller);
        }
        else
        {
            RunExample(controller, args[0]);
        }
    }

    /// <summary>
    /// Displays the interactive menu for selecting demo examples.
    /// </summary>
    static void ShowMenu(TobotController controller)
    {
        while (true)
        {
            Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  Select an Explorer HAT Demo:                            │");
            Console.WriteLine("├─────────────────────────────────────────────────────────-┤");
            Console.WriteLine("│  01. LED Light Show          - Onboard LED patterns      │");
            Console.WriteLine("│  02. Digital Input Monitor   - Read input states         │");
            Console.WriteLine("│  03. Digital Output Control  - Control outputs           │");
            Console.WriteLine("│  04. Analog Sensor Reader    - Read analog voltages      │");
            Console.WriteLine("│  05. Motor Control Demo      - Drive motors              │");
            Console.WriteLine("│  06. Touch Sensor Demo       - Capacitive touch          │");
            Console.WriteLine("│  07. Robot Control System    - Complete robot control    │");
            Console.WriteLine("│  08. System Status Check     - Test all components       │");
            Console.WriteLine("│  09. Pan-Tilt HAT Demo       - Move pan & tilt servos    │");
            Console.WriteLine("│  10. HC-SR04 Distance Demo   - Ultrasonic range test     │");
            Console.WriteLine("│  11. Observable Distance     - Reactive distance monitor │");
            Console.WriteLine("│  12. Random Drive Demo       - Autonomous obstacle avoid │");
            Console.WriteLine("│  13. Directed Detection      - Find object direction     │");
            Console.WriteLine("│  14. Direction Classifier    - Simple direction labeling │");
            Console.WriteLine("│  0. Exit                                                 │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘");
            Console.Write("\nEnter your choice (0-14): ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RunLedDemo(controller);
                    break;
                case "2":
                    RunInputMonitor(controller);
                    break;
                case "3":
                    RunOutputControl(controller);
                    break;
                case "4":
                    RunAnalogReader(controller);
                    break;
                case "5":
                    RunMotorDemo(controller);
                    break;
                case "6":
                    RunTouchDemo(controller);
                    break;
                case "7":
                    RunRobotControl(controller);
                    break;
                case "8":
                    RunSystemCheck(controller);
                    break;
                case "9":
                    RunPanTiltDemo(controller);
                    break;
                case "10":
                    RunHcSr04Demo(controller);
                    break;
                case "11":
                    RunObservableDistanceDemo(controller);
                    break;
                case "12":
                    RunRandomDriveDemo(controller);
                    break;
                case "13":
                    RunDirectedObjectDetectionDemo(controller);
                    break;
                case "14":
                    RunDirectionClassifierDemo(controller);
                    break;
                case "0":
                    Console.WriteLine("Exiting Explorer HAT Demo. Goodbye!");
                    return;
                default:
                    Console.WriteLine("⚠️  Invalid choice. Please enter 0-14.");
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
    static void RunExample(TobotController controller, string exampleName)
    {
        switch (exampleName.ToLower())
        {
            case "led":
                RunLedDemo(controller);
                break;
            case "input":
                RunInputMonitor(controller);
                break;
            case "output":
                RunOutputControl(controller);
                break;
            case "analog":
                RunAnalogReader(controller);
                break;
            case "motor":
                RunMotorDemo(controller);
                break;
            case "touch":
                RunTouchDemo(controller);
                break;
            case "robot":
                RunRobotControl(controller);
                break;
            case "check":
                RunSystemCheck(controller);
                break;
            case "pantilt":
                RunPanTiltDemo(controller);
                break;
            case "hcsr04":
            case "ultrasonic":
                RunHcSr04Demo(controller);
                break;
            case "observable":
            case "rxdistance":
                RunObservableDistanceDemo(controller);
                break;
            case "randomdrive":
            case "autonomous":
                RunRandomDriveDemo(controller);
                break;
            case "detection":
            case "classifier":
            case "direction":
                RunDirectionClassifierDemo(controller);
                break;
            default:
                Console.WriteLine($"Unknown example: {exampleName}");
                Console.WriteLine("Available: led, input, output, analog, motor, touch, robot, check, pantilt, hcsr04, observable, randomdrive, detection, classifier");
                break;
        }
    }

    /// <summary>
    /// Demonstrates LED control with various light patterns and effects.
    /// Shows sequential lighting, simultaneous control, and chase patterns.
    /// </summary>
    static void RunLedDemo(TobotController controller)
    {
        Console.WriteLine("🔆 LED Light Show Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Demonstrating onboard LED control...\n");

        try
        {
            // Sequential LED activation
            Console.WriteLine("▶ Sequential lighting...");
            for (int i = 1; i <= 4; i++)
            {
                controller.SetLedState(i, true);
                Console.WriteLine($"  LED {i}: ON");
                Thread.Sleep(300);
            }
            Thread.Sleep(700);
			
            // All LEDs on
            Console.WriteLine("\n▶ All LEDs on...");
            controller.SetAllLeds(true);
            Thread.Sleep(1000);
			
            // All LEDs off
            Console.WriteLine("▶ All LEDs off...");
            controller.SetAllLeds(false);
            Thread.Sleep(500);
			
            // Knight Rider style chase
            Console.WriteLine("\n▶ Knight Rider chase pattern...");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                for (int i = 1; i <= 4; i++)
                {
                    controller.SetLedState(i, true);
                    Thread.Sleep(100);
                    controller.SetLedState(i, false);
                }
                for (int i = 3; i >= 2; i--)
                {
                    controller.SetLedState(i, true);
                    Thread.Sleep(100);
                    controller.SetLedState(i, false);
                }
            }
			
            // Blink all
            Console.WriteLine("\n▶ Synchronized blinking...");
            for (int i = 0; i < 5; i++)
            {
                controller.SetAllLeds(true);
                Thread.Sleep(200);
                controller.SetAllLeds(false);
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
    static void RunInputMonitor(TobotController controller)
    {
        Console.WriteLine("🔌 Digital Input Monitor");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Monitoring digital inputs (Press Ctrl+C to stop)...\n");

        try
        {
            void Input1Changed(object? _, PinValueChangedEventArgs e) =>
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 1: {e.ChangeType}");
            void Input2Changed(object? _, PinValueChangedEventArgs e) =>
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 2: {e.ChangeType}");
            void Input3Changed(object? _, PinValueChangedEventArgs e) =>
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 3: {e.ChangeType}");
            void Input4Changed(object? _, PinValueChangedEventArgs e) =>
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Input 4: {e.ChangeType}");

            controller.RegisterInputChangedHandler(1, Input1Changed);
            controller.RegisterInputChangedHandler(2, Input2Changed);
            controller.RegisterInputChangedHandler(3, Input3Changed);
            controller.RegisterInputChangedHandler(4, Input4Changed);

            Console.WriteLine("📊 Initial States:");
            Console.WriteLine($"  Input 1: {(controller.ReadDigitalInput(1) ? "HIGH" : "LOW")}");
            Console.WriteLine($"  Input 2: {(controller.ReadDigitalInput(2) ? "HIGH" : "LOW")}");
            Console.WriteLine($"  Input 3: {(controller.ReadDigitalInput(3) ? "HIGH" : "LOW")}");
            Console.WriteLine($"  Input 4: {(controller.ReadDigitalInput(4) ? "HIGH" : "LOW")}");
            Console.WriteLine("\n👂 Listening for changes...\n");

            // Keep monitoring (would normally use cancellation token)
            try
            {
                Thread.Sleep(30000); // Monitor for 30 seconds
            }
            finally
            {
                controller.UnregisterInputChangedHandler(1, Input1Changed);
                controller.UnregisterInputChangedHandler(2, Input2Changed);
                controller.UnregisterInputChangedHandler(3, Input3Changed);
                controller.UnregisterInputChangedHandler(4, Input4Changed);
            }
			
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
    static void RunOutputControl(TobotController controller)
    {
        Console.WriteLine("⚡ Digital Output Control Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Controlling digital outputs...\n");

        try
        {
            // Individual output control
            Console.WriteLine("▶ Testing individual outputs...");
            for (int i = 1; i <= 4; i++)
            {
                Console.WriteLine($"  Output {i}: ON");
                controller.SetDigitalOutput(i, true);
                Thread.Sleep(500);
				
                Console.WriteLine($"  Output {i}: OFF");
                controller.SetDigitalOutput(i, false);
                Thread.Sleep(300);
            }
			
            // All outputs on
            Console.WriteLine("\n▶ All outputs ON...");
            controller.SetAllDigitalOutputs(true);
            Thread.Sleep(1000);
			
            // All outputs off
            Console.WriteLine("▶ All outputs OFF...");
            controller.SetAllDigitalOutputs(false);
            Thread.Sleep(500);
			
            // Toggle demonstration
            Console.WriteLine("\n▶ Toggle demonstration...");
            for (int i = 0; i < 6; i++)
            {
                controller.ToggleDigitalOutput(1);
                controller.ToggleDigitalOutput(3);
                Thread.Sleep(250);
				
                controller.ToggleDigitalOutput(2);
                controller.ToggleDigitalOutput(4);
                Thread.Sleep(250);
            }
			
            // Cleanup
            controller.SetAllDigitalOutputs(false);
			
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
    static void RunAnalogReader(TobotController controller)
    {
        Console.WriteLine("📊 Analog Sensor Reader");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Reading analog inputs (0-5V range)...\n");

        try
        {
            Console.WriteLine("Sampling 20 readings from all channels:\n");
			
            for (int i = 0; i < 20; i++)
            {
                double v1 = controller.ReadAnalogValue(1);
                double v2 = controller.ReadAnalogValue(2);
                double v3 = controller.ReadAnalogValue(3);
                double v4 = controller.ReadAnalogValue(4);
				
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
    static void RunMotorDemo(TobotController controller)
    {
        Console.WriteLine("🤖 Motor Control Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Testing motor drivers...\n");

        try
        {
            // Motor 1 forward
            Console.WriteLine("▶ Motor 1: Forward (100%)");
            controller.DriveMotor(1, 100);
            Thread.Sleep(2000);
			
            Console.WriteLine("▶ Motor 1: Stop");
            controller.StopMotor(1);
            Thread.Sleep(500);
			
            // Motor 1 backward
            Console.WriteLine("▶ Motor 1: Backward (50%)");
            controller.DriveMotor(1, -50);
            Thread.Sleep(2000);
			
            Console.WriteLine("▶ Motor 1: Stop");
            controller.StopMotor(1);
            Thread.Sleep(500);
			
            // Motor 2 speed control
            Console.WriteLine("\n▶ Motor 2: Variable speed");
            for (int speed = 0; speed <= 100; speed += 25)
            {
                Console.WriteLine($"  Speed: {speed}%");
                controller.DriveMotor(2, speed);
                Thread.Sleep(1000);
            }
            controller.StopMotor(2);
			
            // Both motors synchronized
            Console.WriteLine("\n▶ Both motors: Forward (75%)");
            controller.DriveMotors(75, 75);
            Thread.Sleep(2000);
			
            Console.WriteLine("▶ Both motors: Stop");
            controller.StopMotors();
			
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
    static void RunTouchDemo(TobotController controller)
    {
        Console.WriteLine("👆 Touch Sensor Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Testing capacitive touch sensors...\n");
        Console.WriteLine("Touch any sensor (monitoring for 15 seconds)...\n");

        try
        {
            DateTime startTime = DateTime.Now;
			
            while ((DateTime.Now - startTime).TotalSeconds < 15)
            {
                // Check individual sensors and light corresponding LEDs
                for (int i = 1; i <= 4; i++)
                {
                    if (controller.ReadTouchSensor(i))
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Touch sensor {i} detected!");
                        controller.SetLedState(i, true);
                    }
                    else
                    {
                        controller.SetLedState(i, false);
                    }
                }
				
                Thread.Sleep(50);
            }
			
            // Cleanup
            controller.SetAllLeds(false);
			
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
    static void RunRobotControl(TobotController controller)
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
            while (true)
            {
                // Read inputs
                bool forward = controller.ReadDigitalInput(1);
                bool backward = controller.ReadDigitalInput(2);
                bool left = controller.ReadDigitalInput(3);
                bool right = controller.ReadDigitalInput(4);
			
                // Check for obstacles
                double obstacleDistance = controller.ReadAnalogValue(1);
                bool obstacleDetected = obstacleDistance > 3.0;
			
                if (obstacleDetected)
                {
                    // Obstacle detected - stop and alert
                    controller.StopMotors();
                    controller.ToggleLed(1);
                    controller.SetDigitalOutput(1, true);
                    Console.WriteLine("⚠️  OBSTACLE DETECTED! Stopping...");
                }
                else if (forward)
                {
                    // Move forward
                    controller.DriveMotors(80, 80);
                    controller.SetLedState(1, true);
                    controller.SetLedState(2, true);
                    controller.SetLedState(3, false);
                    controller.SetLedState(4, false);
                    controller.SetDigitalOutput(1, false);
                    Console.Write("\r▶ Moving FORWARD    ");
                }
                else if (backward)
                {
                    // Move backward
                    controller.DriveMotors(-80, -80);
                    controller.SetLedState(1, false);
                    controller.SetLedState(2, false);
                    controller.SetLedState(3, true);
                    controller.SetLedState(4, true);
                    controller.SetDigitalOutput(1, false);
                    Console.Write("\r◀ Moving BACKWARD   ");
                }
                else if (left)
                {
                    // Turn left
                    controller.DriveMotor(1, -60);
                    controller.DriveMotor(2, 60);
                    controller.SetLedState(1, true);
                    controller.SetLedState(4, false);
                    controller.SetDigitalOutput(1, false);
                    Console.Write("\r↺ Turning LEFT      ");
                }
                else if (right)
                {
                    // Turn right
                    controller.DriveMotor(1, 60);
                    controller.DriveMotor(2, -60);
                    controller.SetLedState(2, true);
                    controller.SetLedState(3, false);
                    controller.SetDigitalOutput(1, false);
                    Console.Write("\r↻ Turning RIGHT     ");
                }
                else
                {
                    // Stop
                    controller.StopMotors();
                    controller.SetAllLeds(false);
                    controller.SetAllDigitalOutputs(false);
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
    static void RunSystemCheck(TobotController controller)
    {
        Console.WriteLine("🔧 Explorer HAT System Check");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Testing all components...\n");

        try
        {
            // Test LEDs
            Console.Write("Testing LEDs...          ");
            controller.SetAllLeds(true);
            Thread.Sleep(500);
            controller.SetAllLeds(false);
            Console.WriteLine("✅ OK");

            // Test Outputs
            Console.Write("Testing Outputs...       ");
            controller.SetAllDigitalOutputs(true);
            Thread.Sleep(500);
            controller.SetAllDigitalOutputs(false);
            Console.WriteLine("✅ OK");

            // Test Inputs
            Console.Write("Testing Inputs...        ");
            bool i1 = controller.ReadDigitalInput(1);
            bool i2 = controller.ReadDigitalInput(2);
            _ = (i1, i2);
            Console.WriteLine("✅ OK");

            // Test Analog
            Console.Write("Testing Analog ADC...    ");
            double v1 = controller.ReadAnalogValue(1);
            _ = v1;
            Console.WriteLine("✅ OK");

            // Test Touch
            Console.Write("Testing Touch Sensors... ");
            byte touch = controller.ReadTouchState();
            _ = touch;
            Console.WriteLine("✅ OK");

            // Test Motors
            Console.Write("Testing Motors...        ");
            controller.DriveMotor(1, 50);
            Thread.Sleep(500);
            controller.StopMotors();
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
    /// Demonstrates reactive distance monitoring using System.Reactive.
    /// Shows how to observe distance changes with automatic 500ms polling and 1cm threshold.
    /// </summary>
    static void RunObservableDistanceDemo(TobotController controller)
    {
        Console.WriteLine("🔭 Observable Distance Monitor (Reactive)");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Monitoring distance changes reactively...");
        Console.WriteLine("Press any key to stop monitoring.\n");

        const double thresholdCm = 1.0;
        const int samplesPerReading = 5;

        try
        {
            // Subscribe to distance changes
            var subscription = controller.ObserveDistance(thresholdCm, samplesPerReading)
                .Subscribe(
                    distance =>
                    {
                        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                        Console.WriteLine($"[{timestamp}] ⚡ Distance changed: {distance:F1} cm");
                    },
                    error =>
                    {
                        Console.WriteLine($"❌ Error: {error.Message}");
                    },
                    () =>
                    {
                        Console.WriteLine("✅ Monitoring completed.");
                    }
                );

            Console.WriteLine($"📊 Monitoring with {thresholdCm:F1}cm threshold and {samplesPerReading} samples per reading.");
            Console.WriteLine("   Changes are detected automatically every 500ms.\n");

            // Wait for user to press key
            Console.ReadKey(intercept: true);

            // Clean up subscription
            subscription.Dispose();

            Console.WriteLine("\n✅ Observable distance demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Streams HC-SR04 distance measurements to the console.
    /// </summary>
    static void RunHcSr04Demo(TobotController controller)
    {
        Console.WriteLine("📐 HC-SR04 Distance Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Polling the ultrasonic range finder...\n");

        const int iterations = 20;
        const int delayMs = 500;
        const int samplesPerReading = 5;

        try
        {
            for (int i = 0; i < iterations; i++)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                if (controller.TryReadDistance(out double distanceCm, samplesPerReading))
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
    /// Demonstrates autonomous random drive with obstacle avoidance.
    /// Robot drives forward until detecting an obstacle, then randomly turns left or right until clear.
    /// </summary>
    static void RunRandomDriveDemo(TobotController controller)
    {
        Console.WriteLine("🤖 Random Drive Demo (Autonomous Obstacle Avoidance)");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Starting autonomous navigation...");
        Console.WriteLine("Press any key to stop.\n");

        try
        {
            Console.WriteLine("▶ Configuration:");
            Console.WriteLine("  Forward Speed: 50%");
            Console.WriteLine("  Turn Speed: 40%");
            Console.WriteLine("  Obstacle Distance: < 20cm");
            Console.WriteLine("  Clear Distance: > 30cm\n");

            // Start autonomous driving in background
            var driveTask = controller.StartRandomDrive(
                forwardSpeed: 50,
                turnSpeed: 40,
                obstacleDistanceCm: 20.0,
                clearDistanceCm: 30.0);

            Console.WriteLine("🚀 Autonomous mode active!\n");

            // Wait for key press
            Console.ReadKey(intercept: true);

            // Stop autonomous driving
            Console.WriteLine("\n▶ Stopping autonomous mode...");
            controller.StopRandomDrive();

            // Wait for task to complete
            try
            {
                driveTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
                // Expected when task is cancelled
            }

            Console.WriteLine("\n✅ Random drive demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            controller.StopRandomDrive();
        }
    }

    /// <summary>
    /// Demonstrates the Pan-Tilt HAT by sweeping pan and tilt angles.
    /// Requires a Pimoroni Pan-Tilt HAT connected via I2C (default address 0x15).
    /// </summary>
    static void RunPanTiltDemo(TobotController controller)
    {
        Console.WriteLine("🎯 Pan-Tilt HAT Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Demonstrating pan and tilt servo control...\n");

        try
        {
            // Center position (0°, 0°)
            Console.WriteLine("▶ Centering servos...");
            controller.PanTilt(0, 0);
            Thread.Sleep(1000);

            // Sweep pan: -60° -> +60°
            Console.WriteLine("\n▶ Pan sweep: -60° to +60°");
            for (int a = -60; a <= 60; a += 15)
            {
                controller.SetPanAngle(a);
                var current = controller.GetPanTiltAngles().Pan;
                Console.WriteLine($"  Pan: {a}° (current: {current}°)");
                Thread.Sleep(250);
            }
		
            // Return to center
            Console.WriteLine("\n▶ Returning to center...");
            for (int a = 60; a >= 0; a -= 15)
            {
                controller.SetPanAngle(a);
                Thread.Sleep(200);
            }

            // Sweep tilt: -30° -> +30°
            Console.WriteLine("\n▶ Tilt sweep: -30° to +30°");
            for (int a = -30; a <= 30; a += 10)
            {
                controller.SetTiltAngle(a);
                var current = controller.GetPanTiltAngles().Tilt;
                Console.WriteLine($"  Tilt: {a}° (current: {current}°)");
                Thread.Sleep(300);
            }
		
            // Return to center
            Console.WriteLine("\n▶ Returning to center...");
            for (int a = 30; a >= 0; a -= 10)
            {
                controller.SetTiltAngle(a);
                Thread.Sleep(200);
            }

            // Combined movement demonstration
            Console.WriteLine("\n▶ Combined movement pattern");
            controller.SetPanAngle(-45);
            controller.SetTiltAngle(20);
            Console.WriteLine("  Position: (-45°, 20°)");
            Thread.Sleep(600);
		
            controller.SetPanAngle(45);
            controller.SetTiltAngle(-20);
            Console.WriteLine("  Position: (45°, -20°)");
            Thread.Sleep(600);
		
            controller.PanTilt(0, 0);
            Console.WriteLine("  Position: (0°, 0°) - Centered");
            Thread.Sleep(500);

            // Read back current positions
            Console.WriteLine("\n▶ Reading positions from device...");
            var (currentPan, currentTilt) = controller.GetPanTiltAngles();
            Console.WriteLine($"  Reported Pan: {currentPan}°");
            Console.WriteLine($"  Reported Tilt: {currentTilt}°");

            Console.WriteLine($"\n▶ Idle timeout: {controller.GetPanTiltIdleTimeout()} seconds");
            Console.WriteLine("  (Servos will auto-disable after inactivity)\n");

            Console.WriteLine("✅ Pan-Tilt demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates directed object detection using pan-sweep with the HC-SR04 sensor.
    /// Sweeps the sensor left to right to locate the closest object and report its direction.
    /// </summary>
    static void RunDirectedObjectDetectionDemo(TobotController controller)
    {
        Console.WriteLine("🎯 Directed Object Detection Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Scanning for objects with directional info...\n");
        Console.WriteLine("Configuration:");
        Console.WriteLine("  Pan Range: -45° to +45°");
        Console.WriteLine("  Sweep Increment: 5°");
        Console.WriteLine("  Samples per Angle: 5");
        Console.WriteLine("  Object Classification: Left (<-5°), Center (±5°), Right (>5°)");
        Console.WriteLine();

        try
        {
            // Perform multiple detection sweeps
            for (int sweep = 1; sweep <= 3; sweep++)
            {
                Console.WriteLine($"\n▶ Scan {sweep}/3: Sweeping for closest object...");
                var detected = controller.FindClosestObject();

                if (detected != null)
                {
                    Console.WriteLine($"  ✓ Object Detected!");
                    Console.WriteLine($"    Distance: {detected.Distance:F1} cm");
                    Console.WriteLine($"    Pan Angle: {detected.PanAngle}°");
                    Console.WriteLine($"    Direction: {detected.Direction}");
                    
                    // Provide directional feedback
                    string feedback = detected.Direction switch
                    {
                        Tobot.Device.HcSr04.ObjectDirection.Left => "◄ Object is to the LEFT",
                        Tobot.Device.HcSr04.ObjectDirection.Right => "► Object is to the RIGHT",
                        _ => "● Object is CENTERED"
                    };
                    Console.WriteLine($"    {feedback}");
                }
                else
                {
                    Console.WriteLine("  ✗ No object detected in range.");
                }

                // Center servos between scans
                if (sweep < 3)
                {
                    Console.WriteLine("  Centering servos...");
                    controller.PanTilt(0, 0);
                    Thread.Sleep(800);
                }
            }

            // Return to center position
            Console.WriteLine("\n▶ Returning servos to center...");
            controller.PanTilt(0, 0);
            Thread.Sleep(500);

            Console.WriteLine("\n✅ Directed object detection demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates direction classification by manually positioning the pan servo and reading the direction.
    /// This is a lightweight alternative to autonomous sweeping - useful when pan control is handled externally.
    /// </summary>
    static void RunDirectionClassifierDemo(TobotController controller)
    {
        Console.WriteLine("🏷️  Direction Classifier Demo");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("Manual pan positioning with direction classification...\n");
        Console.WriteLine("Configuration:");
        Console.WriteLine("  Direction Zones: Left (<-5°), Center (±5°), Right (>5°)");
        Console.WriteLine("  Method: External pan control + lightweight direction classification");
        Console.WriteLine();

        try
        {
            // Define test positions
            var testPositions = new[]
            {
                (-45, "Far Left"),
                (-30, "Left"),
                (-10, "Slightly Left"),
                (0, "Center"),
                (10, "Slightly Right"),
                (30, "Right"),
                (45, "Far Right")
            };

            Console.WriteLine("▶ Testing direction classification at various pan angles:\n");

            foreach (var (angle, description) in testPositions)
            {
                // Move to position
                controller.SetPanAngle(angle);
                Thread.Sleep(400); // Wait for servo to settle

                // Read distance and get direction
                if (controller.TryReadDistanceWithDirection(angle, out double distanceCm, out var direction))
                {
                    // Get visual indicator
                    string indicator = direction switch
                    {
                        Tobot.Device.HcSr04.ObjectDirection.Left => "◄",
                        Tobot.Device.HcSr04.ObjectDirection.Right => "►",
                        _ => "●"
                    };

                    Console.WriteLine($"  [{indicator}] {angle:+00;-00}° | {description,-20} | {distanceCm:F1}cm | Dir: {direction}");
                }
                else
                {
                    Console.WriteLine($"  [?] {angle:+00;-00}° | {description,-20} | No object detected");
                }
            }

            // Return to center
            Console.WriteLine("\n▶ Returning to center...");
            controller.PanTilt(0, 0);
            Thread.Sleep(500);

            // Demonstrate the lighter-weight GetObjectDirection method
            Console.WriteLine("\n▶ Direction classification examples (without distance reading):");
            int[] angleTests = { -45, -10, 0, 10, 45 };
            foreach (int testAngle in angleTests)
            {
                var direction = controller.GetObjectDirection(testAngle);
                string description = direction switch
                {
                    Tobot.Device.HcSr04.ObjectDirection.Left => "→ LEFT",
                    Tobot.Device.HcSr04.ObjectDirection.Right => "← RIGHT",
                    _ => "→ CENTER ←"
                };
                Console.WriteLine($"  Pan {testAngle:+00;-00}°: {description}");
            }

            Console.WriteLine("\n✅ Direction classifier demo complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
