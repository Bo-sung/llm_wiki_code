#!/usr/bin/env bash
set -euo pipefail

LABEL="com.llmwiki.quartz-static"
LOG_DIR="$HOME/apps/llm-wiki/shared/logs"

echo "=== launchd status: $LABEL ==="
launchctl list "$LABEL" 2>/dev/null || echo "(not loaded)"

echo ""
echo "  Internal URL: http://127.0.0.1:8080/"
echo "  External URL: http://8eh1ndy0u.iptime.org:8081/"
echo "  Port forward: external 8081 -> internal 8080"

echo ""
echo "=== HTTP checks ==="
echo "--- GET / ---"
curl -sI http://127.0.0.1:8080/ 2>/dev/null | head -1 || echo "(no response)"

echo "--- GET /Notes/ ---"
curl -sI http://127.0.0.1:8080/Notes/ 2>/dev/null | head -1 || echo "(no response)"

# Check first note found in public/Notes/References/
NOTE_PATH=$(find "$HOME/apps/llm-wiki/quartz-site/public/Notes/References" \
    -maxdepth 1 -name "*.html" 2>/dev/null | head -1 | \
    sed "s|$HOME/apps/llm-wiki/quartz-site/public||" | sed 's|\.html$||')

if [ -n "$NOTE_PATH" ]; then
    echo "--- GET $NOTE_PATH (extensionless) ---"
    curl -sI "http://127.0.0.1:8080${NOTE_PATH}" 2>/dev/null | head -1 || echo "(no response)"
else
    echo "--- GET extensionless note: no notes found in public/Notes/References/ ---"
fi

echo ""
echo "=== last 20 lines: quartz-static.out.log ==="
tail -20 "$LOG_DIR/quartz-static.out.log" 2>/dev/null || echo "(no log)"

echo ""
echo "=== last 20 lines: quartz-static.err.log ==="
tail -20 "$LOG_DIR/quartz-static.err.log" 2>/dev/null || echo "(no log)"
