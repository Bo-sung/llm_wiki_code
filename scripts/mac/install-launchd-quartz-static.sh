#!/usr/bin/env bash
set -euo pipefail

PLIST_SRC="$(cd "$(dirname "$0")" && pwd)/com.llmwiki.quartz-static.plist.template"
PLIST_DEST="$HOME/Library/LaunchAgents/com.llmwiki.quartz-static.plist"
LABEL="com.llmwiki.quartz-static"
LOG_DIR="$HOME/apps/llm-wiki/shared/logs"
QUARTZ_PUBLIC="$HOME/apps/llm-wiki/quartz-site/public"

echo "=== LLM Wiki: Install Quartz Static Server ==="

# ─── Prerequisite checks ────────────────────────────────────────────────────
if [ ! -f "$PLIST_SRC" ]; then
    echo "[ERROR] Template not found: $PLIST_SRC" >&2
    exit 1
fi

if [ ! -d "$QUARTZ_PUBLIC" ]; then
    echo "[ERROR] Quartz output not found: $QUARTZ_PUBLIC"
    echo "  Run build-quartz-experiment.sh first to generate static files."
    exit 1
fi
echo "[OK] Quartz output exists: $QUARTZ_PUBLIC"

mkdir -p "$LOG_DIR"
echo "[OK] Log directory: $LOG_DIR"

# ─── Install plist ──────────────────────────────────────────────────────────
mkdir -p "$HOME/Library/LaunchAgents"
cp "$PLIST_SRC" "$PLIST_DEST"
echo "[OK] Installed plist: $PLIST_DEST"

# ─── Load service ───────────────────────────────────────────────────────────
# Unload existing job if running
if launchctl list "$LABEL" &>/dev/null; then
    echo "[INFO] Already loaded — unloading first..."
    launchctl bootout "gui/$(id -u)" "$PLIST_DEST" 2>/dev/null || launchctl unload "$PLIST_DEST" 2>/dev/null || true
fi

launchctl bootstrap "gui/$(id -u)" "$PLIST_DEST"
echo "[OK] Loaded: $LABEL"

echo ""
echo "  URL    : http://127.0.0.1:8081/"
echo "  Logs   : $LOG_DIR/quartz-static.out.log"
echo "           $LOG_DIR/quartz-static.err.log"
echo "  Status : bash scripts/mac/status-launchd-quartz-static.sh"
echo ""
echo "  To rebuild content after new notes:"
echo "    bash scripts/mac/build-quartz-experiment.sh"
echo "    (static server picks up changes on next page load)"
