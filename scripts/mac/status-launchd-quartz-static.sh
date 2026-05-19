#!/usr/bin/env bash
set -euo pipefail

LABEL="com.llmwiki.quartz-static"
LOG_DIR="$HOME/apps/llm-wiki/shared/logs"

echo "=== launchd status: $LABEL ==="
launchctl list "$LABEL" 2>/dev/null || echo "(not loaded)"

echo ""
echo "=== HTTP check: http://127.0.0.1:8081/ ==="
curl -sI http://127.0.0.1:8081/ 2>/dev/null | head -3 || echo "(no response)"

echo ""
echo "=== last 20 lines: quartz-static.out.log ==="
tail -20 "$LOG_DIR/quartz-static.out.log" 2>/dev/null || echo "(no log)"

echo ""
echo "=== last 20 lines: quartz-static.err.log ==="
tail -20 "$LOG_DIR/quartz-static.err.log" 2>/dev/null || echo "(no log)"
