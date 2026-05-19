#!/usr/bin/env bash
set -euo pipefail

# ─── Config ─────────────────────────────────────────────────────────────────
QUARTZ_SITE_DIR="${QUARTZ_SITE_DIR:-$HOME/apps/llm-wiki/quartz-site}"
PRIVATE_CONTENT_DIR="${PRIVATE_CONTENT_DIR:-$HOME/apps/llm-wiki/quartz-private-content}"
SHARED_DATA_DIR="${SHARED_DATA_DIR:-$HOME/apps/llm-wiki/shared/data}"
QUARTZ_REPO="https://github.com/jackyzha0/quartz.git"
QUARTZ_BRANCH="v4"

echo "=== LLM Wiki: Quartz Private Viewer Setup ==="
echo "  quartz-site         : $QUARTZ_SITE_DIR"
echo "  private-content     : $PRIVATE_CONTENT_DIR"
echo "  shared/data (source): $SHARED_DATA_DIR"
echo ""

# ─── Prerequisite: Node ─────────────────────────────────────────────────────
if ! command -v node &>/dev/null; then
    echo "[ERROR] Node.js not found."
    echo "  Install: brew install node"
    exit 1
fi
echo "[OK] Node: $(node --version)"

if ! command -v npm &>/dev/null; then
    echo "[ERROR] npm not found."
    exit 1
fi

# ─── Prerequisite: shared/data/Notes ────────────────────────────────────────
NOTES_SRC="$SHARED_DATA_DIR/Notes"
if [ ! -d "$NOTES_SRC" ]; then
    echo "[ERROR] shared/data/Notes not found: $NOTES_SRC"
    echo "  Ensure the watcher has run and generated at least one note."
    exit 1
fi
echo "[OK] shared/data/Notes: $NOTES_SRC"

# ─── Clone quartz-site ──────────────────────────────────────────────────────
if [ -d "$QUARTZ_SITE_DIR/.git" ]; then
    echo "[INFO] quartz-site already exists, skipping clone."
else
    echo "[INFO] Cloning Quartz $QUARTZ_BRANCH into $QUARTZ_SITE_DIR ..."
    git clone --branch "$QUARTZ_BRANCH" --depth 1 "$QUARTZ_REPO" "$QUARTZ_SITE_DIR"
    echo "[OK] Cloned Quartz"
fi

cd "$QUARTZ_SITE_DIR"
echo "[INFO] Installing npm dependencies..."
npm ci
echo "[OK] Dependencies installed"

# ─── quartz-private-content directory ───────────────────────────────────────
mkdir -p "$PRIVATE_CONTENT_DIR"
echo "[OK] private-content dir: $PRIVATE_CONTENT_DIR"

# Notes symlink
_setup_symlink() {
    local link="$1"
    local target="$2"
    local label="$3"

    if [ -L "$link" ]; then
        local cur
        cur=$(readlink "$link")
        if [ "$cur" = "$target" ]; then
            echo "[OK] $label -> $target (already correct)"
        else
            echo "[WARN] $label points to wrong target ($cur), relinking..."
            rm "$link"
            ln -s "$target" "$link"
            echo "[OK] $label -> $target"
        fi
    elif [ -d "$link" ] && [ -z "$(ls -A "$link" 2>/dev/null)" ]; then
        rm -rf "$link"
        ln -s "$target" "$link"
        echo "[OK] $label -> $target"
    elif [ -e "$link" ]; then
        echo "[ERROR] $link exists and is not a symlink."
        echo "  Remove it manually and rerun."
        exit 1
    else
        ln -s "$target" "$link"
        echo "[OK] $label -> $target"
    fi
}

_setup_symlink "$PRIVATE_CONTENT_DIR/Notes" "$NOTES_SRC" "Notes"

INDEX_SRC="$SHARED_DATA_DIR/Index"
if [ -d "$INDEX_SRC" ]; then
    _setup_symlink "$PRIVATE_CONTENT_DIR/Index" "$INDEX_SRC" "Index"
else
    echo "[WARN] shared/data/Index not found ($INDEX_SRC) — skipping Index symlink."
    echo "       Create the directory and rerun setup if Index notes are needed."
fi

# index.md
INDEX_FILE="$PRIVATE_CONTENT_DIR/index.md"
if [ -f "$INDEX_FILE" ]; then
    echo "[OK] index.md exists, skip"
else
    cat > "$INDEX_FILE" <<'INDEXEOF'
---
title: LLM Wiki Private
---

# LLM Wiki Private

## Notes

- [[Notes]]
INDEXEOF
    echo "[OK] index.md created: $INDEX_FILE"
fi

# ─── quartz-site/content symlink -> private-content ─────────────────────────
_setup_symlink "$QUARTZ_SITE_DIR/content" "$PRIVATE_CONTENT_DIR" "content"

echo ""
echo "=== Setup complete ==="
echo "  Content source : $PRIVATE_CONTENT_DIR"
echo "  Notes source   : $NOTES_SRC"
echo ""
echo "Next steps:"
echo "  Build : bash scripts/mac/build-quartz-experiment.sh"
echo "  Serve : bash scripts/mac/serve-quartz-experiment.sh static"
