#!/usr/bin/env bash
# Uninstall llm-wiki launchd watch agent.
# Stops the job, removes the plist.
# Does NOT delete: shared/data, shared/.env, shared/logs.

set -euo pipefail

LABEL="com.llmwiki.watch"
PLIST_PATH="${HOME}/Library/LaunchAgents/${LABEL}.plist"

ok()   { echo "[OK]  $*"; }
warn() { echo "[WARN] $*"; }
step() { echo ""; echo "==> $*"; }

step "Uninstalling ${LABEL}"

UID_VAL="$(id -u)"

if launchctl print "gui/${UID_VAL}/${LABEL}" > /dev/null 2>&1; then
    launchctl bootout "gui/${UID_VAL}" "$PLIST_PATH" 2>/dev/null || \
        launchctl bootout "gui/${UID_VAL}/${LABEL}" 2>/dev/null || true
    ok "launchd job unloaded"
else
    warn "Job not currently loaded — nothing to unload"
fi

if [ -f "$PLIST_PATH" ]; then
    rm -f "$PLIST_PATH"
    ok "plist removed: $PLIST_PATH"
else
    warn "plist not found: $PLIST_PATH"
fi

echo ""
echo "==> Uninstall complete"
echo ""
echo "The following were NOT removed:"
echo "    ${HOME}/apps/llm-wiki/shared/data"
echo "    ${HOME}/apps/llm-wiki/shared/.env"
echo "    ${HOME}/apps/llm-wiki/shared/logs"
