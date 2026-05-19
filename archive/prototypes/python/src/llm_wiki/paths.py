"""Path resolution helpers for the wiki directory structure."""

from pathlib import Path
from llm_wiki.config import NOTES_ROOT, SOURCES_ROOT


CATEGORY_MAP: dict[str, Path] = {
    "projects": NOTES_ROOT / "Projects",
    "tech": NOTES_ROOT / "Tech",
    "ai": NOTES_ROOT / "AI",
    "gamedev": NOTES_ROOT / "GameDev",
    "references": NOTES_ROOT / "References",
}


def notes_dir(category: str = "references") -> Path:
    return CATEGORY_MAP.get(category.lower(), NOTES_ROOT / "References")


def sources_dir(source_type: str = "webpages") -> Path:
    allowed = {"webpages", "youtube", "pdfs"}
    key = source_type.lower() if source_type.lower() in allowed else "webpages"
    return SOURCES_ROOT / key
