#!/usr/bin/env bash
set -euo pipefail

# Run Tobot.Web and open it in Firefox kiosk mode on Raspberry Pi
# - Binds ASP.NET Core to 0.0.0.0:5247
# - Waits until the site is reachable
# - Launches Firefox in fullscreen kiosk mode to /bot

# Config
PORT="5247"
RELATIVE_PAGE="/bot"
ASPNETCORE_URLS_DEFAULT="http://0.0.0.0:${PORT}"
URL_PATH="http://127.0.0.1:${PORT}${RELATIVE_PAGE}"

# Allow override via environment
ASPNETCORE_URLS="${ASPNETCORE_URLS:-$ASPNETCORE_URLS_DEFAULT}"
export ASPNETCORE_URLS

# Move to repo root (this script lives in ./scripts)
cd "$(dirname "$0")/.."

# Ensure dotnet is available
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is not installed or not in PATH" >&2
  exit 1
fi

# Choose Firefox binary
FIREFOX_BIN=""
if command -v firefox >/dev/null 2>&1; then
  FIREFOX_BIN="firefox"
elif command -v firefox-esr >/dev/null 2>&1; then
  FIREFOX_BIN="firefox-esr"
else
  echo "Firefox is not installed (firefox/firefox-esr not found)." >&2
  echo "Install Firefox or adjust this script to use Chromium (chromium-browser --kiosk)." >&2
  exit 1
fi

# Start the web app
echo "Starting Tobot.Web on ${ASPNETCORE_URLS} ..."
DOTNET_LOG="./tobot-web.log"
# Start in background; capture PID
( set -x; dotnet run --project Tobot.Web ) >"${DOTNET_LOG}" 2>&1 &
DOTNET_PID=$!

# Clean up on exit
cleanup() {
  echo "Stopping Tobot.Web (PID ${DOTNET_PID}) ..."
  kill ${DOTNET_PID} >/dev/null 2>&1 || true
  # Try to close Firefox gracefully
  pkill -f "${FIREFOX_BIN} .*${RELATIVE_PAGE}" >/dev/null 2>&1 || true
}
trap cleanup EXIT INT TERM

# Wait for server to be ready
echo -n "Waiting for web app to become reachable"
ATTEMPTS=0
until curl -fsS "http://127.0.0.1:${PORT}/" >/dev/null 2>&1; do
  ATTEMPTS=$((ATTEMPTS+1))
  if [ ${ATTEMPTS} -gt 120 ]; then
    echo
    echo "Timeout waiting for Tobot.Web to start. See ${DOTNET_LOG}." >&2
    exit 1
  fi
  echo -n "."
  sleep 0.5
done
echo " done."

# Ensure we have a GUI display available (X11)
if [ -z "${DISPLAY:-}" ]; then
  if [ -S /tmp/.X11-unix/X0 ]; then
    export DISPLAY=:0
  fi
fi

# If still no DISPLAY, provide guidance and exit gracefully
if [ -z "${DISPLAY:-}" ]; then
  echo "Error: no DISPLAY environment available for GUI browser." >&2
  echo "Hints:" >&2
  echo "- Run this script from the Raspberry Pi desktop session (not over SSH), or" >&2
  echo "- Export DISPLAY=:0 in the same user session that owns the desktop, or" >&2
  echo "- Wrap launch with: DISPLAY=:0 ${FIREFOX_BIN} --kiosk ${URL_PATH}, or" >&2
  echo "- Create a user systemd service that sets DISPLAY=:0 and runs after graphical-session.target" >&2
  echo "The web app is running; open ${URL_PATH} from any device on the network." >&2
  # Keep following logs so the app keeps running
  echo "Logs: tail -f ${DOTNET_LOG} (Ctrl+C to stop)"
  tail -f "${DOTNET_LOG}"
  exit 0
fi

# Open Firefox in kiosk mode to the Simple page
echo "Launching ${FIREFOX_BIN} in kiosk mode at ${URL_PATH} on DISPLAY=${DISPLAY} ..."
( set -x; "${FIREFOX_BIN}" --kiosk "${URL_PATH}" ) &

# Follow the dotnet output so the script stays attached
echo "Logs: tail -f ${DOTNET_LOG} (Ctrl+C to stop)"
tail -f "${DOTNET_LOG}"