"""Filename slug generation."""

import re
from datetime import date


def make_slug(title: str, max_len: int = 60) -> str:
    """Convert title to a filesystem-safe ASCII slug."""
    try:
        from slugify import slugify  # type: ignore
        slug = slugify(title, max_length=max_len, separator="-")
    except ImportError:
        slug = re.sub(r"[^\w\s-]", "", title.lower())
        slug = re.sub(r"[\s_]+", "-", slug).strip("-")
        slug = slug[:max_len]
    return slug or "untitled"


def dated_slug(title: str, created: date | None = None) -> str:
    """Return YYYY-MM-DD-slug format filename (without extension)."""
    d = created or date.today()
    return f"{d.isoformat()}-{make_slug(title)}"
