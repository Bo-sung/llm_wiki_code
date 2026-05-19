#!/usr/bin/env bash
set -euo pipefail

PLIST_DEST="$HOME/Library/LaunchAgents/com.llmwiki.captureapi.plist"
LABEL="com.llmwiki.captureapi"

if launchctl list "$LABEL" &>/dev/null; then
    launchctl unload "$PLIST_DEST"
    echo "[OK] Unloaded: $LABEL"
else
    echo "[INFO] Not loaded: $LABEL"
fi

if [ -f "$PLIST_DEST" ]; then
    rm "$PLIST_DEST"
    echo "[OK] Removed: $PLIST_DEST"
fi
