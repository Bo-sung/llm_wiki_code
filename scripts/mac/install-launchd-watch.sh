#!/usr/bin/env bash
# Install llm-wiki watch as a launchd user agent on Mac mini.
# Runs as the current user. Does NOT require sudo.
# Does NOT modify shared/data or shared/.env.

set -euo pipefail

LABEL="com.llmwiki.watch"
APP_BASE="${HOME}/apps/llm-wiki"
PLIST_DEST="${HOME}/Library/LaunchAgents/${LABEL}.plist"
LOG_DIR="${APP_BASE}/shared/logs"
DOTNET_CANDIDATES=(
    "${HOME}/.dotnet/dotnet"
    "/usr/local/share/dotnet/dotnet"
    "/opt/homebrew/bin/dotnet"
)

# ── helpers ──────────────────────────────────────────────────────────────────

ok()   { echo "[OK]  $*"; }
warn() { echo "[WARN] $*"; }
fail() { echo "[FAIL] $*"; exit 1; }
step() { echo ""; echo "==> $*"; }

# ── 1. detect dotnet ─────────────────────────────────────────────────────────

step "Detecting dotnet"
DOTNET_EXE=""
for candidate in "${DOTNET_CANDIDATES[@]}"; do
    if [ -x "$candidate" ]; then
        DOTNET_EXE="$candidate"
        ok "dotnet: $DOTNET_EXE"
        break
    fi
done

if [ -z "$DOTNET_EXE" ]; then
    fail "dotnet not found. Run deploy-to-mac.ps1 first (Step 4 installs the runtime)."
fi

DOTNET_ROOT_DIR="$(dirname "$DOTNET_EXE")"

# ── 2. check required paths ───────────────────────────────────────────────────

step "Checking required paths"

if [ ! -e "${APP_BASE}/current" ]; then
    fail "current symlink/directory not found: ${APP_BASE}/current — run deploy-to-mac.ps1 first."
fi

if [ ! -f "${APP_BASE}/current/LlmWiki.Cli.dll" ]; then
    fail "LlmWiki.Cli.dll not found in ${APP_BASE}/current — run deploy-to-mac.ps1 first."
fi

if [ ! -f "${APP_BASE}/shared/.env" ]; then
    warn "shared/.env not found: ${APP_BASE}/shared/.env"
    warn "App will run in fallback mode (Gemini disabled)."
else
    ok "shared/.env exists"
fi

ok "current: ${APP_BASE}/current"
ok "LlmWiki.Cli.dll found"

# ── 3. create log directory ───────────────────────────────────────────────────

step "Creating log directory"
mkdir -p "$LOG_DIR"
ok "$LOG_DIR"

# ── 4. generate plist ─────────────────────────────────────────────────────────

step "Generating plist: $PLIST_DEST"
mkdir -p "${HOME}/Library/LaunchAgents"

cat > "$PLIST_DEST" << PLIST_EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>

    <key>Label</key>
    <string>${LABEL}</string>

    <key>ProgramArguments</key>
    <array>
        <string>${DOTNET_EXE}</string>
        <string>LlmWiki.Cli.dll</string>
        <string>watch</string>
    </array>

    <key>WorkingDirectory</key>
    <string>${APP_BASE}/current</string>

    <key>EnvironmentVariables</key>
    <dict>
        <key>LLM_WIKI_ENV_FILE</key>
        <string>${APP_BASE}/shared/.env</string>
        <key>LLM_WIKI_ROOT</key>
        <string>${APP_BASE}/shared/data</string>
        <key>DOTNET_ROOT</key>
        <string>${DOTNET_ROOT_DIR}</string>
        <key>PATH</key>
        <string>${DOTNET_ROOT_DIR}:/usr/local/bin:/opt/homebrew/bin:/usr/bin:/bin:/usr/sbin:/sbin</string>
    </dict>

    <key>StandardOutPath</key>
    <string>${LOG_DIR}/watch.out.log</string>

    <key>StandardErrorPath</key>
    <string>${LOG_DIR}/watch.err.log</string>

    <key>KeepAlive</key>
    <true/>

    <key>RunAtLoad</key>
    <true/>

</dict>
</plist>
PLIST_EOF

ok "plist written"

# ── 5. load launchd agent ─────────────────────────────────────────────────────

step "Loading launchd agent"
UID_VAL="$(id -u)"

# Unload existing job if present (ignore errors)
if launchctl print "gui/${UID_VAL}/${LABEL}" > /dev/null 2>&1; then
    echo "     Unloading existing job..."
    launchctl bootout "gui/${UID_VAL}" "$PLIST_DEST" 2>/dev/null || true
    sleep 1
fi

launchctl bootstrap "gui/${UID_VAL}" "$PLIST_DEST"
ok "bootstrapped: ${LABEL}"

# ── done ──────────────────────────────────────────────────────────────────────

echo ""
echo "==> Install complete"
echo ""
echo "    Label:   ${LABEL}"
echo "    plist:   ${PLIST_DEST}"
echo "    stdout:  ${LOG_DIR}/watch.out.log"
echo "    stderr:  ${LOG_DIR}/watch.err.log"
echo ""
echo "Check status:"
echo "    bash $(dirname "$0")/status-launchd-watch.sh"
echo ""
echo "Test (add a file to Inbox, then check Notes/References):"
echo "    echo 'test' > ${APP_BASE}/shared/data/Inbox/raw_clips/launchd-test.txt"
