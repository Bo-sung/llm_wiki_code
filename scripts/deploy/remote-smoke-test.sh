#!/usr/bin/env bash
# Remote smoke test — runs on the Mac mini directly.
# Usage: bash remote-smoke-test.sh [remote_base] [self_contained]
#   remote_base   : default ~/apps/llm-wiki
#   self_contained: "true" or "false" (default false)

set -euo pipefail

REMOTE_BASE="${1:-$HOME/apps/llm-wiki}"
SELF_CONTAINED="${2:-false}"
SHARED="$REMOTE_BASE/shared"
CURRENT="$REMOTE_BASE/current"

pass() { echo "[OK]   $1"; }
warn() { echo "[WARN] $1"; }
fail() { echo "[FAIL] $1"; exit 1; }

echo "=== llm-wiki smoke test ==="
echo "    remote_base   : $REMOTE_BASE"
echo "    self_contained: $SELF_CONTAINED"
echo ""

# 1. current symlink
[ -L "$CURRENT" ] && pass "current symlink exists" || fail "current symlink missing: $CURRENT"

# 2. shared/data
[ -d "$SHARED/data" ] && pass "shared/data exists" || fail "shared/data missing: $SHARED/data"

# 3. shared/.env (warn only)
if [ -f "$SHARED/.env" ]; then
    pass "shared/.env exists"
else
    warn "shared/.env not found — app will run in fallback mode"
fi

# 4. app binary
if [ "$SELF_CONTAINED" = "true" ]; then
    BINARY="$CURRENT/LlmWiki.Cli"
    [ -f "$BINARY" ] && pass "binary exists: $BINARY" || fail "binary missing: $BINARY"
    chmod +x "$BINARY" 2>/dev/null || true
    RUN_CMD="$BINARY"
else
    DLL="$CURRENT/LlmWiki.Cli.dll"
    [ -f "$DLL" ] && pass "dll exists: $DLL" || fail "dll missing: $DLL"
    RUN_CMD="dotnet $DLL"
fi

# 5. process-once --dry-run
echo ""
echo "--- process-once --dry-run ---"
cd "$CURRENT"
LLM_WIKI_ROOT="$SHARED/data" $RUN_CMD process-once --dry-run
STATUS=$?
if [ $STATUS -eq 0 ]; then
    pass "process-once --dry-run exited 0"
else
    fail "process-once --dry-run exited $STATUS"
fi

echo ""
echo "=== Smoke test passed ==="
