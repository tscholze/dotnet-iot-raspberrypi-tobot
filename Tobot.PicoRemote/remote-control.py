"""
Tobot Remote Control - Raspberry Pi Pico W Keypad Controller

Description:
    A firmware application for the Raspberry Pi Pico W with Pimoroni PicoKeypad that acts as
    a wireless remote control for the Tobot robot. Uses WiFi to send HTTP GET requests to the
    Tobot.Web application's /remote endpoint with action query parameters.

Functionality:
    - 16-key keypad control with visual LED feedback
    - WiFi connectivity status indicators (boot, WiFi, remote endpoint)
    - Controller layout: directional arrows (up/down/left/right), center stop button
    - Special function keys: Light On, Light Off, and additional controls
    - Real-time HTTP communication with Tobot.Web remote control page
    - Status LEDs show connection state and system health

Requirements:
    - Raspberry Pi Pico W (WiFi capable)
    - Pimoroni PicoKeypad (16 RGB backlit keys)
    - MicroPython with picokeypad library
    - WiFi credentials in secret.py (SSID and PASSWORD variables)
    - Network access to Tobot.Web application at configured host/port
    - Tobot.Web running with /remote page accessible (default: pi41:5247/remote)

Configuration:
    - WiFi credentials: Create secret.py with SSID and PASSWORD
    - Remote host: Modify REMOTE_HOST ("pi41") and REMOTE_PORT (5247) as needed
    - Action mappings: Adjust CONTROLLER_ACTIONS dict for custom key bindings

Status Indicators (Top Row):
    - Pad 0 (Boot): Green = booted, Red = not ready
    - Pad 1 (WiFi): Green = connected, Red = disconnected
    - Pad 2 (Remote): Green = endpoint reachable, Red = unreachable
    - Pad 3 (Companion): Orange (reserved for future use)
"""

import time
import picokeypad
import network
import socket
import json


# Import WiFi credentials
try:
    from secret import SSID, PASSWORD
    CREDENTIALS_OK = True
except ImportError:
    print("Error: secret.py not found. Please create secret.py with SSID and PASSWORD variables.")
    SSID = None
    PASSWORD = None
    CREDENTIALS_OK = False


# Color constants
OFF = (0, 0, 0)              # LED off
GREEN = (0, 255, 0)          # Full-bright green
GREEN_DIM = (0, 127, 0)      # Half-bright green for status indicators
RED = (255, 0, 0)            # Full-bright red
BLUE = (0, 0, 255)           # Full-bright blue
BLUE_DIM = (0, 0, 127)       # Half-bright blue for controller arrows
PURPLE = (128, 0, 255)       # Purple for all special function keys
ORANGE = (255, 128, 0)       # Orange (unused)
WHITE_DIM = (96, 96, 96)     # Dim white baseline for non-status keys

# Status pad indices
STATUS_PAD_ROW = 0           # Top status row (keys 0-3)
STATUS_BOOT_PAD = 0          # Boot status indicator pad
STATUS_WIFI_PAD = 1          # WiFi connection indicator pad
STATUS_REMOTE_PAD = 2        # Remote endpoint indicator pad
STATUS_COMPANION_PAD = 3     # Companion device indicator pad (unused)

#controller pad indices
CONTROLLER_UP = 6            # Forward
CONTROLLER_LEFT = 9          # Left
CONTROLLER_STOP = 10         # Stop
CONTROLLER_RIGHT = 11        # Right 
CONTROLLER_DOWN = 14         # Backwards

# Special function pads
FUNCTION_KEY_1 = 4           # Special function key 1
FUNCTION_KEY_2 = 8           # Special function key 2
FUNCTION_KEY_3 = 12          # Special function key 3 (orange)

# Status tracking
boot_status = False         # True when booted
wifi_status = False         # True when WiFi connected
remote_status = False       # True when remote endpoint reachable

# Remote endpoint parsing
REMOTE_HOST = "pi41"        # Base URL
REMOTE_PORT = 5247          # Port number
REMOTE_PATH = "/remote"     # Path

# Initialize keypad
keypad = picokeypad.PicoKeypad()
NUM_PADS = keypad.get_num_pads()

# WiFi and network globals
wlan = None

# Controller action mappings
CONTROLLER_ACTIONS = {
    CONTROLLER_UP: "forward",
    CONTROLLER_LEFT: "left",
    CONTROLLER_STOP: "stop",
    CONTROLLER_RIGHT: "right",
    CONTROLLER_DOWN: "backwards",
    FUNCTION_KEY_1: "light-on",
    FUNCTION_KEY_2: "light-off",
}

def update_status_leds():
    """Set status row LEDs from boot/WiFi/SignalR flags and flush to the keypad."""
    # Boot status (first key) - green when booted
    if boot_status:
        keypad.illuminate(STATUS_BOOT_PAD, GREEN_DIM[0], GREEN_DIM[1], GREEN_DIM[2])
    else:
        keypad.illuminate(STATUS_BOOT_PAD, RED[0], RED[1], RED[2])
    
    # WiFi status (second key) - green when connected
    if wifi_status:
        keypad.illuminate(STATUS_WIFI_PAD, GREEN_DIM[0], GREEN_DIM[1], GREEN_DIM[2])
    else:
        keypad.illuminate(STATUS_WIFI_PAD, RED[0], RED[1], RED[2])
    
    # Remote status (third key) - green when remote reachable
    if remote_status:
        keypad.illuminate(STATUS_REMOTE_PAD, GREEN_DIM[0], GREEN_DIM[1], GREEN_DIM[2])
    else:
        keypad.illuminate(STATUS_REMOTE_PAD, RED[0], RED[1], RED[2])

    # Companion (second Pi) status
    keypad.illuminate(STATUS_COMPANION_PAD, ORANGE[0], ORANGE[1], ORANGE[2])
    
    # Update the display
    keypad.update()


def show_controller_pattern():
    """Draw the controller arrows and special keys without clearing the status row."""
    # Set all keys to off first
    for i in range(NUM_PADS):
        if i in (STATUS_BOOT_PAD, STATUS_WIFI_PAD, STATUS_REMOTE_PAD):
            continue  # Preserve status indicators
        keypad.illuminate(i, WHITE_DIM[0], WHITE_DIM[1], WHITE_DIM[2])
    
    # Display controller pattern with blue dim LEDs
    keypad.illuminate(CONTROLLER_UP, BLUE_DIM[0], BLUE_DIM[1], BLUE_DIM[2])
    keypad.illuminate(CONTROLLER_LEFT, BLUE_DIM[0], BLUE_DIM[1], BLUE_DIM[2])
    keypad.illuminate(CONTROLLER_STOP, BLUE_DIM[0], BLUE_DIM[1], BLUE_DIM[2])
    keypad.illuminate(CONTROLLER_RIGHT, BLUE_DIM[0], BLUE_DIM[1], BLUE_DIM[2])
    keypad.illuminate(CONTROLLER_DOWN, BLUE_DIM[0], BLUE_DIM[1], BLUE_DIM[2])
    
    # Display special function keys
    keypad.illuminate(FUNCTION_KEY_1, PURPLE[0], PURPLE[1], PURPLE[2])
    keypad.illuminate(FUNCTION_KEY_2, PURPLE[0], PURPLE[1], PURPLE[2])
    keypad.illuminate(FUNCTION_KEY_3, PURPLE[0], PURPLE[1], PURPLE[2])
    
    # Refresh status LEDs so they stay visible
    update_status_leds()


def http_get(query):
    """Perform a simple HTTP GET to http://pi41:5247/remote with provided query string. Returns (status_code, body)."""
    addr = socket.getaddrinfo(REMOTE_HOST, REMOTE_PORT)[0][-1]
    s = socket.socket()
    s.settimeout(10)
    s.connect(addr)
    full_path = '{}?{}'.format(REMOTE_PATH, query) if query else REMOTE_PATH
    req = (
        "GET {} HTTP/1.1\r\n"
        "Host: {}:{}\r\n"
        "Accept: */*\r\n"
        "Connection: close\r\n\r\n"
    ).format(full_path, REMOTE_HOST, REMOTE_PORT)
    s.send(req)
    data = b""
    while True:
        chunk = s.recv(1024)
        if not chunk:
            break
        data += chunk
    s.close()
    parts = data.split(b"\r\n\r\n", 1)
    header = parts[0]
    body = parts[1] if len(parts) > 1 else b""
    first_line = header.split(b"\r\n", 1)[0]
    try:
        status_code = int(first_line.split()[1])
    except Exception:
        status_code = 0
    return status_code, body


def check_remote_endpoint():
    """Perform a simple GET to verify remote endpoint availability."""
    global remote_status
    if not wifi_status:
        remote_status = False
        return False
    try:
        status_code, _ = http_get("action=ping")
        remote_status = (status_code == 200)
        print("Remote endpoint status:", status_code)
        return remote_status
    except Exception as e:
        print("Remote check failed:", e)
        remote_status = False
        return False


def connect_wifi():
    """Connect to WiFi using secret.py credentials; updates wifi_status and returns success."""
    global wlan, wifi_status
    
    if not SSID or not PASSWORD:
        print("Missing WiFi credentials (secret.py). Skipping WiFi connection.")
        wifi_status = False
        return False

    print("Connecting to WiFi...")
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    
    # Connect to network
    wlan.connect(SSID, PASSWORD)
    
    # Wait for connection with timeout
    timeout = 10
    start_time = time.time()
    while not wlan.isconnected() and (time.time() - start_time) < timeout:
        print("Waiting for WiFi connection...")
        time.sleep(1)
    
    if wlan.isconnected():
        print(f"WiFi connected! IP: {wlan.ifconfig()[0]}")
        wifi_status = True
        return True
    else:
        print("WiFi connection failed!")
        wifi_status = False
        return False



def initialize():
    """Boot sequence: check creds, set boot flag, connect WiFi, check remote, render LEDs."""
    global boot_status
    
    print("Booting remote control...")
    
    if not CREDENTIALS_OK:
        print("Cannot start: secret.py with SSID and PASSWORD is missing.")
        return
    
    # Mark as booted
    boot_status = True
    update_status_leds()
    
    # Connect to WiFi
    connect_wifi()
    update_status_leds()

    # Check remote endpoint availability
    check_remote_endpoint()
    update_status_leds()

    # Show controller pattern
    show_controller_pattern()
    
    print("Initialization complete!")


def send_action(action):
    """Send a simple GET request to remote endpoint with action parameter."""
    if not wifi_status:
        print("WiFi not connected")
        return False
    try:
        status, body = http_get('action={}'.format(action))
        print('Action', action, 'status:', status)
        ok = (status == 200)
        if not ok:
            # Mark remote as failed and refresh entire display
            global remote_status
            remote_status = False
            show_controller_pattern()
        return ok
    except Exception as e:
        print('Action failed:', e)
        # Mark remote as failed and refresh entire display
        global remote_status
        remote_status = False
        show_controller_pattern()
        return False


def cleanup():
    """Disconnect WiFi safely during shutdown."""
    global wlan
    if wlan:
        try:
            if wlan.isconnected():
                wlan.disconnect()
        except Exception:
            pass
    print("Cleanup complete")


def handle_keypad_events():
    """Main loop to handle keypad button presses and send actions."""
    print("Starting keypad event loop...")
    last_button_states = 0
    last_refresh = 0
    refresh_interval = 1  # Refresh display every 1 second
    
    while True:
        # Read current button states
        button_states = keypad.get_button_states()
        
        # Check each controller pad
        for pad_index, action in CONTROLLER_ACTIONS.items():
            # Check if this button is newly pressed (bit set in current, not in last)
            button_bit = 1 << pad_index
            if (button_states & button_bit) and not (last_button_states & button_bit):
                print(f"Pad {pad_index} pressed, sending action: {action}")
                send_action(action)
        
        # Update last state
        last_button_states = button_states
        
        # Periodic display refresh to keep LEDs active
        current_time = time.time()
        if current_time - last_refresh > refresh_interval:
            show_controller_pattern()
            last_refresh = current_time
        
        # Small delay to avoid CPU spinning
        time.sleep(0.05)


# Main execution
if __name__ == "__main__":
    initialize()
    handle_keypad_events()