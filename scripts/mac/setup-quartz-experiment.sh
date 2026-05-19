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

# ─── Prerequisite: Node ─────────────────────────────────────────────────────
if ! command -v node &>/dev/null; then
    echo "[ERROR] Node.js not found."
    echo "  Install via Homebrew: brew install node"
    echo "  Or nvm: curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash && nvm install 20"
    exit 1
fi
echo "[OK] Node: $(node --version)"

if ! command -v npm &>/dev/null; then
    echo "[ERROR] npm not found."
    exit 1
fi

# ─── Prerequisite: public-vault ─────────────────────────────────────────────
if [ ! -d "$PUBLIC_VAULT_DIR" ]; then
    echo "[ERROR] public-vault not found: $PUBLIC_VAULT_DIR"
    echo "  Deploy and set up public-vault first."
    exit 1
fi
echo "[OK] public-vault found: $PUBLIC_VAULT_DIR"

# ─── Clone quartz-site ──────────────────────────────────────────────────────
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

# ─── Content symlink ────────────────────────────────────────────────────────
# Quartz reads from quartz-site/content/. We link it to public-vault so
# Quartz config files stay isolated in quartz-site and vault stays clean.
CONTENT_LINK="$QUARTZ_SITE_DIR/content"

if [ -L "$CONTENT_LINK" ]; then
    CURRENT_TARGET=$(readlink "$CONTENT_LINK")
    if [ "$CURRENT_TARGET" = "$PUBLIC_VAULT_DIR" ]; then
        echo "[OK] content -> $PUBLIC_VAULT_DIR (symlink already correct)"
    else
        echo "[WARN] content symlink points to wrong target: $CURRENT_TARGET"
        echo "  Relinking to $PUBLIC_VAULT_DIR ..."
        rm "$CONTENT_LINK"
        ln -s "$PUBLIC_VAULT_DIR" "$CONTENT_LINK"
        echo "[OK] content -> $PUBLIC_VAULT_DIR"
    fi
elif [ -d "$CONTENT_LINK" ]; then
    if [ -z "$(ls -A "$CONTENT_LINK" 2>/dev/null)" ]; then
        echo "[INFO] content is an empty directory, replacing with symlink..."
        rm -rf "$CONTENT_LINK"
        ln -s "$PUBLIC_VAULT_DIR" "$CONTENT_LINK"
        echo "[OK] content -> $PUBLIC_VAULT_DIR"
    else
        echo "[ERROR] $CONTENT_LINK is a non-empty directory."
        echo "  Remove or back it up manually, then rerun this script."
        exit 1
    fi
elif [ -e "$CONTENT_LINK" ]; then
    echo "[ERROR] $CONTENT_LINK exists but is not a directory or symlink."
    exit 1
else
    ln -s "$PUBLIC_VAULT_DIR" "$CONTENT_LINK"
    echo "[OK] content -> $PUBLIC_VAULT_DIR"
fi

# ─── index.md ───────────────────────────────────────────────────────────────
# Quartz requires content/index.md as the home page. Create a minimal one
# if it doesn't exist. Never overwrites an existing file.
INDEX_FILE="$PUBLIC_VAULT_DIR/index.md"
if [ -f "$INDEX_FILE" ]; then
    echo "[OK] index.md exists, skip"
else
    cat > "$INDEX_FILE" <<'INDEXEOF'
---
title: LLM Wiki
---

# LLM Wiki

## Notes

- [[Notes]]
INDEXEOF
    echo "[OK] index.md created: $INDEX_FILE"
fi

echo ""
echo "=== Setup complete ==="
echo "Next steps:"
echo "  Build : bash scripts/mac/build-quartz-experiment.sh"
echo "  Serve : bash scripts/mac/serve-quartz-experiment.sh"
