"""Process raw text clips dropped into data/Inbox/raw_clips/."""

from __future__ import annotations

import logging
from pathlib import Path
from typing import Any

from llm_wiki.gemini_client import summarize_and_structure
from llm_wiki.markdown_writer import write_note
from llm_wiki.paths import notes_dir

logger = logging.getLogger(__name__)


def process_text_file(path: Path, category: str = "references") -> Path | None:
    """Process a raw text clip and write a note. Returns output path or None on error."""
    try:
        raw_text = path.read_text(encoding="utf-8")
        source_meta: dict[str, Any] = {
            "url": "",
            "source_type": "raw_clip",
            "title": path.stem,
        }
        structured = summarize_and_structure(raw_text, source_meta)
        output_path = write_note(structured, source_meta, notes_dir(category))
        logger.info("Wrote note: %s", output_path)
        return output_path
    except Exception as exc:  # noqa: BLE001
        logger.error("Failed to process %s: %s", path, exc)
        return None
