#!/usr/bin/env bash
set -euo pipefail

PLIST_SRC="$(cd "$(dirname "$0")" && pwd)/com.llmwiki.captureapi.plist.template"
PLIST_DEST="$HOME/Library/LaunchAgents/com.llmwiki.captureapi.plist"
LABEL="com.llmwiki.captureapi"
LOG_DIR="$HOME/apps/llm-wiki/shared/logs"

mkdir -p "$LOG_DIR"

if [ ! -f "$PLIST_SRC" ]; then
    echo "[ERROR] Template not found: $PLIST_SRC" >&2
    exit 1
fi

cp "$PLIST_SRC" "$PLIST_DEST"
echo "[OK] Installed plist: $PLIST_DEST"

if launchctl list "$LABEL" &>/dev/null; then
    echo "[INFO] Already loaded — reloading..."
    launchctl unload "$PLIST_DEST"
fi

launchctl load "$PLIST_DEST"
echo "[OK] Loaded: $LABEL"
echo "[INFO] Logs: $LOG_DIR/captureapi.out.log / captureapi.err.log"
echo "[INFO] Check status: launchctl list $LABEL"
