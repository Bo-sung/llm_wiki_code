#!/usr/bin/env bash
set -euo pipefail

# ─── Config ─────────────────────────────────────────────────────────────────
QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"
PUBLIC_VAULT_DIR="${PUBLIC_VAULT_DIR:-$HOME/apps/llm-wiki/public-vault}"
QUARTZ_REPO="https://github.com/jackyzha0/quartz.git"
QUARTZ_BRANCH="v4"

echo "=== LLM Wiki: Quartz Experiment Setup ==="
echo "  quartz-site : $QUARTZ_SITE_DIR"
echo "  public-vault: $PUBLIC_VAULT_DIR"
echo ""

# ─── Prerequisite check ─────────────────────────────────────────────────────
if ! command -v node &>/dev/null; then
    echo "[ERROR] Node.js not found."
    echo "  Install via Homebrew: brew install node"
    echo "  Or nvm: curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash && nvm install 20"
    exit 1
fi

NODE_VERSION=$(node --version)
echo "[OK] Node: $NODE_VERSION"

if ! command -v npm &>/dev/null; then
    echo "[ERROR] npm not found."
    exit 1
fi

if [ ! -d "$PUBLIC_VAULT_DIR" ]; then
    echo "[ERROR] public-vault not found: $PUBLIC_VAULT_DIR"
    echo "  Deploy and set up public-vault first."
    exit 1
fi

echo "[OK] public-vault found: $PUBLIC_VAULT_DIR"

# ─── Clone or update quartz-site ────────────────────────────────────────────
if [ -d "$QUARTZ_SITE_DIR/.git" ]; then
    echo "[INFO] quartz-site already exists, skipping clone."
else
    echo "[INFO] Cloning Quartz $QUARTZ_BRANCH into $QUARTZ_SITE_DIR ..."
    git clone --branch "$QUARTZ_BRANCH" --depth 1 "$QUARTZ_REPO" "$QUARTZ_SITE_DIR"
    echo "[OK] Cloned Quartz"
fi

cd "$QUARTZ_SITE_DIR"

# ─── Install dependencies ───────────────────────────────────────────────────
echo "[INFO] Installing npm dependencies..."
npm ci
echo "[OK] Dependencies installed"

# ─── Patch quartz.config.ts to use public-vault as contentDir ───────────────
CONFIG_FILE="$QUARTZ_SITE_DIR/quartz.config.ts"
if grep -q "$PUBLIC_VAULT_DIR" "$CONFIG_FILE" 2>/dev/null; then
    echo "[INFO] quartz.config.ts already points to public-vault, skipping patch."
else
    echo ""
    echo "╔═══════════════════════════════════════════════════════════════╗"
    echo "║  MANUAL STEP REQUIRED                                         ║"
    echo "╟───────────────────────────────────────────────────────────────╢"
    echo "║  Edit: $CONFIG_FILE"
    echo "║  Set:  contentDir to \"$PUBLIC_VAULT_DIR\""
    echo "║"
    echo "║  In configuration section:"
    echo "║    contentDir: \"$PUBLIC_VAULT_DIR\","
    echo "╚═══════════════════════════════════════════════════════════════╝"
    echo ""
fi

echo ""
echo "=== Setup complete ==="
echo "Next steps:"
echo "  1. (If prompted above) Edit quartz.config.ts to set contentDir"
echo "  2. Run: bash scripts/mac/build-quartz-experiment.sh"
echo "  3. Run: bash scripts/mac/serve-quartz-experiment.sh"
