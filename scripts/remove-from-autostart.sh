#!/usr/bin/env bash
set -euo pipefail

# Remove Tobot.Web kiosk autostart by disabling and deleting the user systemd unit.

SERVICE_NAME="tobot-web-kiosk.service"
UNIT_DIR="${XDG_CONFIG_HOME:-$HOME/.config}/systemd/user"
UNIT_FILE="${UNIT_DIR}/${SERVICE_NAME}"

# Try to stop/disable the service (ignore errors if not present)
systemctl --user disable --now "${SERVICE_NAME}" >/dev/null 2>&1 || true

# Remove unit file if it exists
if [ -f "${UNIT_FILE}" ]; then
  rm -f "${UNIT_FILE}"
fi

# Reload user systemd
systemctl --user daemon-reload

echo "Autostart removed: ${SERVICE_NAME}"
echo "If linger was enabled, you can disable it with: sudo loginctl disable-linger $USER"
