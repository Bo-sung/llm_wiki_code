#!/usr/bin/env bash
set -euo pipefail

LABEL="com.llmwiki.captureapi"
LOG_DIR="$HOME/apps/llm-wiki/shared/logs"

echo "=== launchd status: $LABEL ==="
launchctl list "$LABEL" || echo "(not loaded)"

echo ""
echo "=== health check ==="
curl -s http://127.0.0.1:5055/health || echo "(no response)"

echo ""
echo "=== last 20 lines: captureapi.out.log ==="
tail -20 "$LOG_DIR/captureapi.out.log" 2>/dev/null || echo "(no log)"

echo ""
echo "=== last 20 lines: captureapi.err.log ==="
tail -20 "$LOG_DIR/captureapi.err.log" 2>/dev/null || echo "(no log)"
