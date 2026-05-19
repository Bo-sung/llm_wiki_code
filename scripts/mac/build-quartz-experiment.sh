#!/usr/bin/env bash
set -euo pipefail

QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"
PUBLIC_VAULT_DIR="${PUBLIC_VAULT_DIR:-$HOME/apps/llm-wiki/public-vault}"

echo "=== LLM Wiki: Quartz Experiment Build ==="
echo "  quartz-site: $QUARTZ_SITE_DIR"
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
echo "[OK] content symlink: $(readlink "$CONTENT_LINK")"

if [ ! -f "$CONTENT_LINK/index.md" ]; then
    echo "[ERROR] content/index.md not found."
    echo "  Run setup-quartz-experiment.sh to create index.md."
    exit 1
fi
echo "[OK] content/index.md exists"

if [ ! -d "$CONTENT_LINK/Notes" ]; then
    echo "[WARN] content/Notes/ not found — vault may be empty or Notes not yet generated."
fi

# ─── Build ──────────────────────────────────────────────────────────────────
cd "$QUARTZ_SITE_DIR"

echo ""
echo "[INFO] Building Quartz..."
npx quartz build

OUTPUT_DIR="$QUARTZ_SITE_DIR/public"
if [ -d "$OUTPUT_DIR" ]; then
    FILE_COUNT=$(find "$OUTPUT_DIR" -type f | wc -l | tr -d ' ')
    echo "[OK] Build complete: $FILE_COUNT files in $OUTPUT_DIR"
else
    echo "[WARN] Output directory not found at $OUTPUT_DIR — check quartz build output."
fi
