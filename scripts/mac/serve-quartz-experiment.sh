#!/usr/bin/env bash
set -euo pipefail

# Usage: serve-quartz-experiment.sh [port] [host]
#   port  default: 8080
#   host  default: localhost  (use 0.0.0.0 for LAN/Tailscale access)
#
# Examples:
#   bash serve-quartz-experiment.sh
#   bash serve-quartz-experiment.sh 8081
#   bash serve-quartz-experiment.sh 8080 0.0.0.0

QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"
PORT="${1:-${QUARTZ_PORT:-8080}}"
HOST="${2:-localhost}"

echo "=== LLM Wiki: Quartz Experiment Serve ==="
echo "  quartz-site: $QUARTZ_SITE_DIR"
echo "  port       : $PORT"
echo "  host       : $HOST"
echo ""

if [ ! -d "$QUARTZ_SITE_DIR" ]; then
    echo "[ERROR] quartz-site not found: $QUARTZ_SITE_DIR"
    echo "  Run setup-quartz-experiment.sh and build-quartz-experiment.sh first."
    exit 1
fi

if ! command -v node &>/dev/null; then
    echo "[ERROR] Node.js not found."
    exit 1
fi

CONTENT_LINK="$QUARTZ_SITE_DIR/content"
if [ ! -L "$CONTENT_LINK" ]; then
    echo "[ERROR] content symlink not found. Run setup-quartz-experiment.sh first."
    exit 1
fi

cd "$QUARTZ_SITE_DIR"

LAN_IP=$(ipconfig getifaddr en0 2>/dev/null || echo "")

echo "Starting Quartz preview server..."
echo ""
if [ "$HOST" = "0.0.0.0" ]; then
    echo "  Local  : http://localhost:$PORT"
    [ -n "$LAN_IP" ] && echo "  Network: http://$LAN_IP:$PORT"
    echo ""
    echo "  [NOTE] Bound to 0.0.0.0 — accessible from LAN/Tailscale."
    echo "         Ensure firewall allows port $PORT if needed."
else
    echo "  Local  : http://localhost:$PORT"
    echo ""
    echo "  [NOTE] Bound to localhost only."
    echo "         For LAN access, use SSH tunnel:"
    echo "           ssh -L $PORT:localhost:$PORT <mac-user>@<mac-ip>"
fi
echo ""
echo "Press Ctrl+C to stop."
echo ""

if [ "$HOST" = "0.0.0.0" ]; then
    npx quartz build --serve --port "$PORT" --host "$HOST"
else
    npx quartz build --serve --port "$PORT"
fi
