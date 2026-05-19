#!/usr/bin/env bash
# Export a single note from operating vault to public vault.
# Does NOT git add, commit, or push.
#
# Usage:
#   bash export-public-note.sh /full/path/to/note.md
#   bash export-public-note.sh Notes/References/2026-05-19-example.md

set -euo pipefail

# ─── Defaults ────────────────────────────────────────────────────────────────

OPERATING_VAULT="${OPERATING_VAULT:-/Users/boseong/apps/llm-wiki/shared/data}"
PUBLIC_VAULT="/Users/boseong/apps/llm-wiki/public-vault"
CONFIG_FILE="${CONFIG_FILE:-/Users/boseong/apps/llm-wiki/shared/config/public-vault.env}"

# Read PUBLIC_VAULT_PATH from config file selectively (never source the whole file)
if [ -f "$CONFIG_FILE" ]; then
    _vault_from_config=$(grep -E '^PUBLIC_VAULT_PATH=' "$CONFIG_FILE" 2>/dev/null | head -1 | cut -d= -f2-)
    if [ -n "$_vault_from_config" ]; then
        PUBLIC_VAULT="$_vault_from_config"
    fi
fi

# ─── Helpers ─────────────────────────────────────────────────────────────────

err()  { echo "[ERROR] $*" >&2; exit 1; }
warn() { echo "[WARN]  $*"; }
ok()   { echo "[OK]    $*"; }
step() { echo ""; echo "==> $*"; }

# ─── Input parsing ────────────────────────────────────────────────────────────

if [ $# -eq 0 ]; then
    echo "Usage: bash $0 <note-path>"
    echo "  Absolute: ${OPERATING_VAULT}/Notes/References/a.md"
    echo "  Relative: Notes/References/a.md"
    exit 1
fi

INPUT="$1"

# Convert relative to absolute
if [ "${INPUT#/}" = "$INPUT" ]; then
    INPUT="${OPERATING_VAULT}/${INPUT}"
fi

# Normalise path (resolve symlinks / .. without requiring realpath)
if [ -e "$INPUT" ]; then
    SOURCE="$(cd "$(dirname "$INPUT")" && pwd)/$(basename "$INPUT")"
else
    SOURCE="$INPUT"
fi

# ─── Validation ──────────────────────────────────────────────────────────────

step "Validating source"

[ -e "$SOURCE" ] || err "Source file not found: $SOURCE"
[ -f "$SOURCE" ] || err "Source must be a file, not a directory: $SOURCE"

# Must be inside operating vault
case "$SOURCE" in
    "${OPERATING_VAULT}/"*) ;;
    *) err "Source file must be inside operating vault: ${OPERATING_VAULT}" ;;
esac

# Compute relative path
RELATIVE="${SOURCE#${OPERATING_VAULT}/}"
BASENAME="$(basename "$SOURCE")"
NAMENOEXT="${BASENAME%.*}"

ok "Source:   $SOURCE"
ok "Relative: $RELATIVE"

# ─── Forbidden check ─────────────────────────────────────────────────────────

step "Checking export permissions"

# Forbidden file names and extensions
case "$BASENAME" in
    .env|.DS_Store)
        err "Refusing to export private or unsafe file: $BASENAME" ;;
    *.log|*.tmp)
        err "Refusing to export private or unsafe file: $BASENAME" ;;
esac

# Forbidden path prefixes
case "$RELATIVE" in
    Inbox/*|Inbox)
        err "Refusing to export private or unsafe path: $RELATIVE" ;;
    Sources/*|Sources)
        err "Refusing to export private or unsafe path: $RELATIVE" ;;
    System/private/*)
        err "Refusing to export private or unsafe path: $RELATIVE" ;;
    logs/*)
        err "Refusing to export private or unsafe path: $RELATIVE" ;;
esac

# Allowed path check
case "$RELATIVE" in
    Notes/*|Index/*|Templates/*|System/public/*)
        ok "Path permitted: $RELATIVE" ;;
    *)
        err "Path not in allowed export list. Allowed: Notes/, Index/, Templates/, System/public/" ;;
esac

# ─── Destination ─────────────────────────────────────────────────────────────

step "Preparing destination"

DEST="${PUBLIC_VAULT}/${RELATIVE}"
DEST_DIR="$(dirname "$DEST")"

if [ -f "$DEST" ]; then
    warn "Overwriting existing public note: $RELATIVE"
fi

mkdir -p "$DEST_DIR"
ok "Destination: $DEST"

# ─── Copy ────────────────────────────────────────────────────────────────────

step "Copying"
cp "$SOURCE" "$DEST"
ok "Copied: $RELATIVE"

# ─── Git status / diff ───────────────────────────────────────────────────────

step "Git status in public vault"

if [ -d "${PUBLIC_VAULT}/.git" ]; then
    git -C "$PUBLIC_VAULT" status --short

    echo ""
    echo "==> Git diff for this file"
    if git -C "$PUBLIC_VAULT" status --short -- "$RELATIVE" 2>/dev/null | grep -q '^??'; then
        echo "    (New untracked file — no diff to show)"
    else
        git -C "$PUBLIC_VAULT" diff -- "$RELATIVE" 2>/dev/null || true
    fi
else
    warn "public-vault is not a git repo. Run: git -C '$PUBLIC_VAULT' init"
fi

# ─── Manual commit instructions ──────────────────────────────────────────────

echo ""
echo "==> Next: review, then commit and push manually"
echo ""
echo "    cd $PUBLIC_VAULT"
echo "    git add $RELATIVE"
echo "    git commit -m \"Publish note: $NAMENOEXT\""
echo "    git push"

# ─── Review reminder ─────────────────────────────────────────────────────────

echo ""
echo "==> Before committing, review:"
echo "    - no API keys, tokens, or passwords"
echo "    - no personal email, phone, or address"
echo "    - no private project names or internal information"
echo "    - no copyrighted full text"
echo "    - no raw Inbox content"
echo "    - not an accidental test note"
echo "    - source URL included if required"
echo ""
echo "    Checklist: docs/PUBLIC_NOTE_REVIEW_CHECKLIST.md"
