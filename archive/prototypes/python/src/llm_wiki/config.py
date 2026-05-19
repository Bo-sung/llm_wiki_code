"""Load and expose configuration from environment variables."""

import os
from pathlib import Path
from dotenv import load_dotenv

load_dotenv()

GEMINI_API_KEY: str = os.getenv("GEMINI_API_KEY", "")

_root_env = os.getenv("LLM_WIKI_ROOT", "./data")
WIKI_ROOT: Path = Path(_root_env).resolve()

INBOX_ROOT: Path = WIKI_ROOT / "Inbox"
NOTES_ROOT: Path = WIKI_ROOT / "Notes"
SOURCES_ROOT: Path = WIKI_ROOT / "Sources"
TEMPLATES_ROOT: Path = WIKI_ROOT / "Templates"
SYSTEM_ROOT: Path = WIKI_ROOT / "System"
INDEX_ROOT: Path = WIKI_ROOT / "Index"

PROMPTS_DIR: Path = SYSTEM_ROOT / "prompts"
REFINE_NOTE_PROMPT: Path = PROMPTS_DIR / "refine_note.md"
