#!/usr/bin/env bash
set -euo pipefail

# Add Tobot.Web kiosk to autostart using a user systemd service.
# This creates and enables ~/.config/systemd/user/tobot-web-kiosk.service
# which runs scripts/run-tobot-web-kiosk.sh after the graphical session starts.

SERVICE_NAME="tobot-web-kiosk.service"
UNIT_DIR="${XDG_CONFIG_HOME:-$HOME/.config}/systemd/user"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
RUN_SCRIPT="${REPO_ROOT}/scripts/run-tobot-web-kiosk.sh"

if [ ! -f "${RUN_SCRIPT}" ]; then
  echo "Error: kiosk run script not found at ${RUN_SCRIPT}" >&2
  exit 1
fi

mkdir -p "${UNIT_DIR}"
UNIT_FILE="${UNIT_DIR}/${SERVICE_NAME}"

cat >"${UNIT_FILE}" <<EOF
[Unit]
Description=Tobot Web Kiosk (user session)
After=graphical-session.target network-online.target
Wants=graphical-session.target network-online.target

[Service]
Type=simple
Environment=DISPLAY=:0
WorkingDirectory=${REPO_ROOT}
ExecStart=${RUN_SCRIPT}
Restart=on-failure
RestartSec=5

[Install]
WantedBy=default.target
EOF

# Reload user systemd, enable and start the service
systemctl --user daemon-reload
systemctl --user enable --now "${SERVICE_NAME}"

echo "Autostart enabled: ${SERVICE_NAME}"
echo "Unit file: ${UNIT_FILE}"

# Tip for boot without interactive login
if loginctl show-user "$USER" 2>/dev/null | grep -q "Linger=no"; then
  echo "Tip: to keep the user service running after boot without login, run:"
  echo "  sudo loginctl enable-linger $USER"
fi

# Show status summary (optional)
if systemctl --user is-active --quiet "${SERVICE_NAME}"; then
  echo "Service is active. Check status with: systemctl --user status ${SERVICE_NAME} --no-pager"
else
  echo "Service is not active yet. Check logs in the repo: scripts/tobot-web.log (after first run)."
fi
