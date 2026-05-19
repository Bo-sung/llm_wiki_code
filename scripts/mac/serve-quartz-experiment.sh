#!/usr/bin/env bash
set -euo pipefail

# Usage: serve-quartz-experiment.sh [mode] [port] [host]
#   mode  static (default) | dev
#   port  default: 8080
#   host  default: localhost  (use 0.0.0.0 for LAN/Tailscale access)
#
# Examples:
#   bash serve-quartz-experiment.sh                        # static, caddy, localhost:8080
#   bash serve-quartz-experiment.sh static 8080 0.0.0.0   # static, caddy, LAN-accessible
#   bash serve-quartz-experiment.sh dev                    # dev server (npx quartz), localhost:8080
#
# Mode differences:
#   static — caddy with try_files (pretty URL support) — recommended for ops
#   dev    — npx quartz build --serve (live-reload, dev only, no caddy needed)

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
MODE="${1:-static}"
QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"

case "$MODE" in
  static) PORT="${2:-8080}"; HOST="${3:-localhost}" ;;
  dev)    PORT="${2:-8080}"; HOST="${3:-localhost}" ;;
  *)
    echo "[ERROR] Unknown mode: $MODE"
    echo "  Usage: $0 [static|dev] [port] [host]"
    exit 1
    ;;
esac

echo "=== LLM Wiki: Quartz Serve ($MODE) ==="
echo "  quartz-site: $QUARTZ_SITE_DIR"
echo "  port       : $PORT"
echo "  host       : $HOST"
echo ""

if [ ! -d "$QUARTZ_SITE_DIR" ]; then
    echo "[ERROR] quartz-site not found: $QUARTZ_SITE_DIR"
    echo "  Run setup-quartz-experiment.sh first."
    exit 1
fi

CONTENT_LINK="$QUARTZ_SITE_DIR/content"
if [ ! -L "$CONTENT_LINK" ]; then
    echo "[ERROR] content symlink not found. Run setup-quartz-experiment.sh first."
    exit 1
fi

LAN_IP=$(ipconfig getifaddr en0 2>/dev/null || echo "")

_print_urls() {
    if [ "$HOST" = "0.0.0.0" ]; then
        echo "  Local  : http://localhost:$PORT"
        [ -n "$LAN_IP" ] && echo "  Network: http://$LAN_IP:$PORT"
        echo "  [NOTE] Bound to 0.0.0.0 — accessible from LAN/Tailscale."
    else
        echo "  Local  : http://localhost:$PORT"
        echo "  [NOTE] Bound to localhost only."
        echo "         For LAN access: ssh -L $PORT:localhost:$PORT <user>@<mac-ip>"
    fi
}

cd "$QUARTZ_SITE_DIR"
echo "Press Ctrl+C to stop."
echo ""

if [ "$MODE" = "static" ]; then
    OUTPUT_DIR="$QUARTZ_SITE_DIR/public"
    if [ ! -d "$OUTPUT_DIR" ]; then
        echo "[ERROR] $OUTPUT_DIR not found. Run build-quartz-experiment.sh first."
        exit 1
    fi

    CADDY_BIN=$(command -v caddy 2>/dev/null || echo "")
    if [ -z "$CADDY_BIN" ]; then
        echo "[ERROR] caddy not found. Install with: brew install caddy"
        echo "  python3 http.server does NOT support Quartz extensionless URLs."
        exit 1
    fi

    # Generate a temporary Caddyfile for this session
    TEMP_CADDYFILE=$(mktemp /tmp/Caddyfile.XXXXXX)
    trap 'rm -f "$TEMP_CADDYFILE"' EXIT
    cat > "$TEMP_CADDYFILE" <<CADDY
:${PORT} {
    bind ${HOST}
    root * ${OUTPUT_DIR}
    file_server
    try_files {path} {path}.html {path}/index.html
}
CADDY

    _print_urls
    echo ""
    exec "$CADDY_BIN" run --config "$TEMP_CADDYFILE" --adapter caddyfile
else
    # dev mode — npx quartz build --serve (caddy not needed)
    if ! command -v node &>/dev/null; then
        echo "[ERROR] Node.js not found."
        exit 1
    fi
    _print_urls
    echo ""
    if [ "$HOST" = "0.0.0.0" ]; then
        npx quartz build --serve --port "$PORT" --host "$HOST"
    else
        npx quartz build --serve --port "$PORT"
    fi
fi
