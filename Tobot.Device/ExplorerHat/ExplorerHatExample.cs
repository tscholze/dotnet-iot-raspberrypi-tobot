namespace Tobot.Device.ExplorerHat.Examples
{
    /// <summary>
    /// Provides usage examples for the Explorer HAT.
    /// </summary>
    public static class ExplorerHatExample
    {
        /// <summary>
        /// Demonstrates basic LED control.
        /// </summary>
        public static void LedExample()
        {
            using var hat = new ExplorerHat();
            
            // Turn on individual LEDs
            hat.Light.One.On();
            Thread.Sleep(500);
            
            hat.Light.Two.On();
            Thread.Sleep(500);
            
            // Turn on all LEDs
            hat.Light.On();
            Thread.Sleep(1000);
            
            // Turn off all LEDs
            hat.Light.Off();
        }

        /// <summary>
        /// Demonstrates digital input reading.
        /// </summary>
        public static void InputExample()
        {
            using var hat = new ExplorerHat();
            
            // Read a single input
            bool input1State = hat.Input.One.Read();
            Console.WriteLine($"Input 1 is: {(input1State ? "HIGH" : "LOW")}");
            
            // Register event handler for input changes
            hat.Input.One.Changed += (sender, args) =>
            {
                Console.WriteLine($"Input 1 changed to: {args.ChangeType}");
            };
        }

        /// <summary>
        /// Demonstrates digital output control.
        /// </summary>
        public static void OutputExample()
        {
            using var hat = new ExplorerHat();
            
            // Control individual outputs
            hat.Output.One.On();
            Thread.Sleep(500);
            hat.Output.One.Off();
            
            // Toggle output
            hat.Output.Two.Toggle();
            
            // Control all outputs
            hat.Output.On();
            Thread.Sleep(1000);
            hat.Output.Off();
        }

        /// <summary>
        /// Demonstrates analog input reading.
        /// </summary>
        public static void AnalogExample()
        {
            using var hat = new ExplorerHat();
            
            // Read analog input (returns voltage 0-5V)
            double voltage = hat.Analog.One.Read();
            Console.WriteLine($"Analog 1 voltage: {voltage:F2}V");
            
            // Continuous reading
            for (int i = 0; i < 10; i++)
            {
                double v1 = hat.Analog.One.Read();
                double v2 = hat.Analog.Two.Read();
                Console.WriteLine($"A1: {v1:F2}V, A2: {v2:F2}V");
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Demonstrates motor control.
        /// </summary>
        public static void MotorExample()
        {
            using var hat = new ExplorerHat();
            
            // Move motor 1 forward at full speed
            hat.Motor.One.Forward(100);
            Thread.Sleep(2000);
            
            // Move motor 1 backward at half speed
            hat.Motor.One.Backward(50);
            Thread.Sleep(2000);
            
            // Stop motor
            hat.Motor.One.Stop();
            
            // Use SetSpeed for precise control
            hat.Motor.Two.SetSpeed(75);  // Forward at 75%
            Thread.Sleep(2000);
            hat.Motor.Two.SetSpeed(-50); // Backward at 50%
            Thread.Sleep(2000);
            
            // Stop all motors
            hat.Motor.Stop();
        }

        /// <summary>
        /// Demonstrates capacitive touch sensor reading.
        /// </summary>
        public static void TouchExample()
        {
            using var hat = new ExplorerHat();
            
            // Read all touch sensors at once
            byte touchState = hat.Touch.ReadAll();
            Console.WriteLine($"Touch state: {touchState:X2}");
            
            // Read individual touch sensor
            while (true)
            {
                if (hat.Touch[1].IsTouched())
                {
                    Console.WriteLine("Touch sensor 1 is touched!");
                    hat.Light.One.On();
                }
                else
                {
                    hat.Light.One.Off();
                }
                
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Demonstrates a complete robot control example.
        /// </summary>
        public static void RobotExample()
        {
            using var hat = new ExplorerHat();
            
            Console.WriteLine("Robot Control Example");
            Console.WriteLine("Use Input 1 to move forward");
            Console.WriteLine("Use Input 2 to move backward");
            Console.WriteLine("Press Ctrl+C to exit");
            
            while (true)
            {
                // Read inputs
                bool forward = hat.Input.One.Read();
                bool backward = hat.Input.Two.Read();
                
                if (forward)
                {
                    // Move forward
                    hat.Motor.One.Forward(80);
                    hat.Motor.Two.Forward(80);
                    hat.Light.One.On();
                    hat.Light.Two.On();
                }
                else if (backward)
                {
                    // Move backward
                    hat.Motor.One.Backward(80);
                    hat.Motor.Two.Backward(80);
                    hat.Light.Three.On();
                    hat.Light.Four.On();
                }
                else
                {
                    // Stop
                    hat.Motor.Stop();
                    hat.Light.Off();
                }
                
                // Read analog sensor for obstacle detection
                double distance = hat.Analog.One.Read();
                if (distance > 3.0)
                {
                    // Obstacle detected, flash LED
                    hat.Output.One.Toggle();
                }
                
                Thread.Sleep(50);
            }
        }
    }
}
