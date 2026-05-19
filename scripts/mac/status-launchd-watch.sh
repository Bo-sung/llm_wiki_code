#!/usr/bin/env bash
# Show status of the llm-wiki launchd watch agent.

set -uo pipefail

LABEL="com.llmwiki.watch"
PLIST_PATH="${HOME}/Library/LaunchAgents/${LABEL}.plist"
LOG_DIR="${HOME}/apps/llm-wiki/shared/logs"
LOG_OUT="${LOG_DIR}/watch.out.log"
LOG_ERR="${LOG_DIR}/watch.err.log"
TAIL_LINES=50

section() { echo ""; echo "──── $* ────"; }

section "plist"
if [ -f "$PLIST_PATH" ]; then
    echo "[OK]  $PLIST_PATH"
else
    echo "[MISSING] $PLIST_PATH"
fi

section "launchctl status"
UID_VAL="$(id -u)"
launchctl print "gui/${UID_VAL}/${LABEL}" 2>&1 || echo "(not loaded)"

section "stdout log (last ${TAIL_LINES} lines): ${LOG_OUT}"
if [ -f "$LOG_OUT" ]; then
    tail -n "$TAIL_LINES" "$LOG_OUT"
else
    echo "(log not found)"
fi

section "stderr log (last ${TAIL_LINES} lines): ${LOG_ERR}"
if [ -f "$LOG_ERR" ]; then
    tail -n "$TAIL_LINES" "$LOG_ERR"
else
    echo "(log not found)"
fi
