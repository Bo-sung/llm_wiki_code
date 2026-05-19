#!/usr/bin/env bash
set -euo pipefail

PLIST_DEST="$HOME/Library/LaunchAgents/com.llmwiki.quartz-static.plist"
LABEL="com.llmwiki.quartz-static"

echo "=== LLM Wiki: Uninstall Quartz Static Server ==="

if launchctl list "$LABEL" &>/dev/null; then
    launchctl bootout "gui/$(id -u)" "$PLIST_DEST" 2>/dev/null || launchctl unload "$PLIST_DEST" 2>/dev/null || true
    echo "[OK] Unloaded: $LABEL"
else
    echo "[INFO] Not loaded: $LABEL"
fi

if [ -f "$PLIST_DEST" ]; then
    rm "$PLIST_DEST"
    echo "[OK] Removed: $PLIST_DEST"
fi

echo "[INFO] Logs and quartz-site/public are preserved."
