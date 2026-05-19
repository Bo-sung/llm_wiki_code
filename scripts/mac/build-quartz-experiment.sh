#!/usr/bin/env bash
set -euo pipefail

QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"
PRIVATE_CONTENT_DIR="${PRIVATE_CONTENT_DIR:-$HOME/apps/llm-wiki/quartz-private-content}"

echo "=== LLM Wiki: Quartz Build ==="
echo "  quartz-site    : $QUARTZ_SITE_DIR"
echo "  private-content: $PRIVATE_CONTENT_DIR"
echo ""

# ─── Prerequisites ───────────────────────────────────────────────────────────
if [ ! -d "$QUARTZ_SITE_DIR" ]; then
    echo "[ERROR] quartz-site not found: $QUARTZ_SITE_DIR"
    echo "  Run setup-quartz-experiment.sh first."
    exit 1
fi

if ! command -v node &>/dev/null; then
    echo "[ERROR] Node.js not found. Run setup-quartz-experiment.sh first."
    exit 1
fi

# ─── Sanity checks ──────────────────────────────────────────────────────────
CONTENT_LINK="$QUARTZ_SITE_DIR/content"

if [ ! -L "$CONTENT_LINK" ]; then
    echo "[ERROR] $CONTENT_LINK is not a symlink."
    echo "  Run setup-quartz-experiment.sh to create the content symlink."
    exit 1
fi

CONTENT_TARGET=$(readlink "$CONTENT_LINK")
echo "[OK] content -> $CONTENT_TARGET"

if [ ! -f "$CONTENT_LINK/index.md" ]; then
    echo "[ERROR] content/index.md not found."
    echo "  Run setup-quartz-experiment.sh."
    exit 1
fi
echo "[OK] content/index.md exists"

if [ ! -L "$PRIVATE_CONTENT_DIR/Notes" ]; then
    echo "[WARN] $PRIVATE_CONTENT_DIR/Notes is not a symlink — Notes may not appear."
fi

if [ ! -d "$CONTENT_LINK/Notes" ]; then
    echo "[WARN] content/Notes not found — no notes will be rendered."
fi

# ─── Build ──────────────────────────────────────────────────────────────────
cd "$QUARTZ_SITE_DIR"

echo ""
echo "[INFO] Building Quartz (content: $CONTENT_TARGET)..."
npx quartz build

OUTPUT_DIR="$QUARTZ_SITE_DIR/public"
if [ -d "$OUTPUT_DIR" ]; then
    FILE_COUNT=$(find "$OUTPUT_DIR" -type f | wc -l | tr -d ' ')
    echo "[OK] Quartz build complete"
    echo "[OK] Output: $OUTPUT_DIR ($FILE_COUNT files)"
    echo ""
    echo "  Install static server: bash scripts/mac/install-launchd-quartz-static.sh"
    echo "  Quick serve locally:   bash scripts/mac/serve-quartz-experiment.sh static"
else
    echo "[ERROR] Output directory not found at $OUTPUT_DIR"
    exit 1
fi
