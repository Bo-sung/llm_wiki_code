#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PLIST_SRC="$SCRIPT_DIR/com.llmwiki.quartz-static.plist.template"
CADDYFILE_SRC="$SCRIPT_DIR/Caddyfile.quartz-static"
PLIST_DEST="$HOME/Library/LaunchAgents/com.llmwiki.quartz-static.plist"
CADDYFILE_DEST="$HOME/apps/llm-wiki/quartz-site/Caddyfile"
LABEL="com.llmwiki.quartz-static"
LOG_DIR="$HOME/apps/llm-wiki/shared/logs"
QUARTZ_PUBLIC="$HOME/apps/llm-wiki/quartz-site/public"

echo "=== LLM Wiki: Install Quartz Static Server (Caddy) ==="

# ─── Caddy check ─────────────────────────────────────────────────────────────
CADDY_BIN=$(command -v caddy 2>/dev/null || echo "")
if [ -z "$CADDY_BIN" ]; then
    echo "[ERROR] caddy not found."
    echo "  Install with: brew install caddy"
    echo "  Or: https://caddyserver.com/docs/install"
    exit 1
fi
echo "[OK] caddy: $CADDY_BIN"

# ─── Prerequisite checks ─────────────────────────────────────────────────────
if [ ! -f "$PLIST_SRC" ]; then
    echo "[ERROR] Template not found: $PLIST_SRC" >&2
    exit 1
fi

if [ ! -f "$CADDYFILE_SRC" ]; then
    echo "[ERROR] Caddyfile template not found: $CADDYFILE_SRC" >&2
    exit 1
fi

if [ ! -d "$QUARTZ_PUBLIC" ]; then
    echo "[ERROR] Quartz output not found: $QUARTZ_PUBLIC"
    echo "  Run build-quartz-experiment.sh first."
    exit 1
fi
echo "[OK] Quartz output exists: $QUARTZ_PUBLIC"

# ─── Install Caddyfile ───────────────────────────────────────────────────────
cp "$CADDYFILE_SRC" "$CADDYFILE_DEST"
echo "[OK] Caddyfile: $CADDYFILE_DEST"

mkdir -p "$LOG_DIR"
echo "[OK] Log directory: $LOG_DIR"

# ─── Install plist (substitute caddy path) ───────────────────────────────────
mkdir -p "$HOME/Library/LaunchAgents"
sed "s|__CADDY_PATH__|$CADDY_BIN|g" "$PLIST_SRC" > "$PLIST_DEST"
echo "[OK] Installed plist: $PLIST_DEST"
echo "  caddy path: $CADDY_BIN"

# ─── Load service ────────────────────────────────────────────────────────────
if launchctl list "$LABEL" &>/dev/null; then
    echo "[INFO] Already loaded — unloading first..."
    launchctl bootout "gui/$(id -u)" "$PLIST_DEST" 2>/dev/null || launchctl unload "$PLIST_DEST" 2>/dev/null || true
fi

launchctl bootstrap "gui/$(id -u)" "$PLIST_DEST"
echo "[OK] Loaded: $LABEL"

echo ""
echo "  Internal : http://127.0.0.1:8080/"
echo "  External : http://8eh1ndy0u.iptime.org:8081/ (port-forward: ext 8081 -> int 8080)"
echo "  Logs     : $LOG_DIR/quartz-static.out.log"
echo "             $LOG_DIR/quartz-static.err.log"
echo "  Status   : bash scripts/mac/status-launchd-quartz-static.sh"
echo ""
echo "  To rebuild after new notes:"
echo "    bash scripts/mac/build-quartz-experiment.sh"
echo "    (Caddy picks up changes immediately — no restart needed)"
