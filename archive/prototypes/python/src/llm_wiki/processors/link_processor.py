"""Process link files dropped into data/Inbox/links/."""

from __future__ import annotations

import logging
from pathlib import Path
from typing import Any

from llm_wiki.gemini_client import summarize_and_structure
from llm_wiki.markdown_writer import write_note
from llm_wiki.paths import notes_dir

logger = logging.getLogger(__name__)


def _read_link_file(path: Path) -> tuple[str, dict[str, Any]]:
    """Read a .txt or .url file; return (raw_text, source_meta)."""
    text = path.read_text(encoding="utf-8").strip()
    # Assume first non-empty line is the URL, rest is optional context
    lines = [l for l in text.splitlines() if l.strip()]
    url = lines[0] if lines else ""
    extra = "\n".join(lines[1:]) if len(lines) > 1 else ""
    meta: dict[str, Any] = {
        "url": url,
        "source_type": "webpage",
        "title": path.stem,
        "extra_context": extra,
    }
    return text, meta


def process_link_file(path: Path, category: str = "references") -> Path | None:
    """Process a single link file and write a note. Returns output path or None on error."""
    try:
        raw_text, source_meta = _read_link_file(path)
        structured = summarize_and_structure(raw_text, source_meta)
        output_path = write_note(structured, source_meta, notes_dir(category))
        logger.info("Wrote note: %s", output_path)
        return output_path
    except Exception as exc:  # noqa: BLE001
        logger.error("Failed to process %s: %s", path, exc)
        return None
