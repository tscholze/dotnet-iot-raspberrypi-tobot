# Quick Start Guide - Tobot Explorer HAT Demo

Get up and running with the Explorer HAT demo in 5 minutes!

## Prerequisites Checklist

- [ ] Raspberry Pi (any model with 40-pin GPIO)
- [ ] Pimoroni Explorer HAT attached
- [ ] .NET 9 SDK installed
- [ ] I2C enabled
- [ ] Optional: Motors connected to motor ports

## 5-Minute Setup

### Step 1: Install .NET 9 on Raspberry Pi

```bash
# Download and install .NET 9
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0

# Add to PATH (add to ~/.bashrc for permanent)
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
```

### Step 2: Enable I2C

```bash
sudo raspi-config
```

Navigate: **Interface Options** ? **I2C** ? **Yes** ? **OK** ? **Finish**

Reboot:
```bash
sudo reboot
```

### Step 3: Verify I2C Devices

```bash
# Install i2c-tools if needed
sudo apt-get install -y i2c-tools

# Check for Explorer HAT devices
i2cdetect -y 1
```

Expected output:
```
     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f
00:          -- -- -- -- -- -- -- -- -- -- -- -- -- 
10:          -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
20:          -- -- -- -- -- -- -- -- 28 -- -- -- -- -- -- -- 
30:          -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
40:          -- -- -- -- -- -- -- -- 48 -- -- -- -- -- -- -- 
```
- `0x28` = CAP1208 Touch Controller ?
- `0x48` = ADS1015 ADC ?

### Step 4: Clone and Build

```bash
# Clone repository (or copy your files)
cd ~/
git clone <your-repo-url> Tobot
cd Tobot

# Build the solution
dotnet build

# Should see: Build succeeded. 0 Warning(s)
```

### Step 5: Run the Demo

```bash
dotnet run --project Tobot
```

## First Demo - LED Test

1. Select option **1** from the menu
2. Watch the LEDs light up in sequence
3. See the Knight Rider chase pattern
4. Confirm all 4 LEDs work

## Quick Demo Commands

```bash
# LED light show
dotnet run --project Tobot led

# System check (verify everything)
dotnet run --project Tobot check

# Motor test (ensure motors connected!)
dotnet run --project Tobot motor

# Interactive menu
dotnet run --project Tobot
```

## Troubleshooting Quick Fixes

### "Permission denied" errors

```bash
sudo usermod -a -G gpio $USER
sudo usermod -a -G i2c $USER
sudo reboot
```

### I2C devices not found

```bash
# Ensure I2C is enabled
sudo raspi-config

# Check kernel modules
lsmod | grep i2c
# Should see: i2c_bcm2835

# Manual enable
sudo modprobe i2c-dev
sudo modprobe i2c-bcm2835
```

### "Could not find GPIO controller"

```bash
# Install required libraries
sudo apt-get update
sudo apt-get install -y libgpiod-dev

# Check GPIO device
ls -l /dev/gpiochip*
# Should exist
```

### Build errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## Interactive Menu Guide

```
???????????????????????????????????????
?  1. LED Light Show          ? Start here
?  2. Digital Input Monitor   ? Test inputs
?  3. Digital Output Control  ? Test outputs
?  4. Analog Sensor Reader    ? Test ADC
?  5. Motor Control Demo      ? Test motors
?  6. Touch Sensor Demo       ? Test touch
?  7. Robot Control System    ? Full demo
?  8. System Status Check     ? Verify all
?  0. Exit
???????????????????????????????????????
```

## Safety First! ??

Before running motor demos:

1. ? Check motor connections
2. ? Ensure proper power supply (motors need external power!)
3. ? Keep clear of moving parts
4. ? Have emergency stop ready (Ctrl+C)

## Command Line Cheat Sheet

```bash
# Quick system check
dotnet run --project Tobot check

# LED patterns
dotnet run --project Tobot led

# Monitor inputs (30 seconds)
dotnet run --project Tobot input

# Test analog readings
dotnet run --project Tobot analog

# Full robot control
dotnet run --project Tobot robot
```

## Next Steps

Once basic demos work:

1. **Customize** - Edit `Tobot/Program.cs` to add your own patterns
2. **Explore** - Review `Tobot.Device/ExplorerHat/` for driver details
3. **Build** - Create your own robot control logic
4. **Share** - Contribute your examples!

## Getting Help

### Check System Status
```bash
dotnet run --project Tobot check
```

### View Logs
The demos output status messages showing what's happening.

### Hardware Check
- LEDs should be visible on the Explorer HAT
- Green power LED should be lit
- Check all connections are secure

## Success Indicators ?

You're ready to go when:

- [x] `i2cdetect` shows devices at 0x28 and 0x48
- [x] `dotnet build` completes successfully
- [x] LED demo runs and all 4 LEDs light up
- [x] System check shows all "? OK"

## Common Use Cases

### Test Everything Quickly
```bash
dotnet run --project Tobot check
```

### Create a Light Show
```bash
dotnet run --project Tobot led
```

### Build a Robot
```bash
# Interactive control with all features
dotnet run --project Tobot robot
```

### Monitor Sensors
```bash
# Watch analog sensors in real-time
dotnet run --project Tobot analog
```

## What's Next?

- ?? Read the full [README.md](README.md)
- ?? Explore [Tobot.Device documentation](../Tobot.Device/ExplorerHat/README.md)
- ?? Check out example code in [ExplorerHatExample.cs](../Tobot.Device/ExplorerHat/ExplorerHatExample.cs)
- ?? Build your own robot control logic!

---

**Happy Making!** ??

If you got this far and everything works, you're ready to build something awesome!
