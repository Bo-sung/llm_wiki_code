#!/usr/bin/env bash
set -euo pipefail

QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"

echo "=== LLM Wiki: Quartz Experiment Build ==="
echo "  quartz-site: $QUARTZ_SITE_DIR"
echo ""

if [ ! -d "$QUARTZ_SITE_DIR" ]; then
    echo "[ERROR] quartz-site not found: $QUARTZ_SITE_DIR"
    echo "  Run setup-quartz-experiment.sh first."
    exit 1
fi

if ! command -v node &>/dev/null; then
    echo "[ERROR] Node.js not found. Run setup-quartz-experiment.sh first."
    exit 1
fi

cd "$QUARTZ_SITE_DIR"

echo "[INFO] Building Quartz..."
npx quartz build

OUTPUT_DIR="$QUARTZ_SITE_DIR/public"
if [ -d "$OUTPUT_DIR" ]; then
    FILE_COUNT=$(find "$OUTPUT_DIR" -type f | wc -l | tr -d ' ')
    echo "[OK] Build complete: $FILE_COUNT files in $OUTPUT_DIR"
else
    echo "[WARN] Output directory not found at $OUTPUT_DIR — check quartz build output."
fi
