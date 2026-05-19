#!/usr/bin/env bash
set -euo pipefail

QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"
PORT="${QUARTZ_PORT:-8080}"

echo "=== LLM Wiki: Quartz Experiment Serve ==="
echo "  quartz-site: $QUARTZ_SITE_DIR"
echo "  port       : $PORT"
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

cd "$QUARTZ_SITE_DIR"

LAN_IP=$(ipconfig getifaddr en0 2>/dev/null || echo "unknown")

echo "Starting local preview server..."
echo ""
echo "  Local  : http://localhost:$PORT"
if [ "$LAN_IP" != "unknown" ]; then
    echo "  Network: http://$LAN_IP:$PORT"
fi
echo ""
echo "Press Ctrl+C to stop."
echo ""

npx quartz build --serve --port "$PORT"
