"""YAML frontmatter generation for Obsidian-compatible Markdown notes."""

from __future__ import annotations

from datetime import date
from typing import Any


def build_frontmatter(
    title: str = "",
    created: date | str | None = None,
    source_url: str = "",
    source_type: str = "",
    status: str = "processed",
    tags: list[str] | None = None,
    related: list[str] | None = None,
) -> dict[str, Any]:
    """Return a frontmatter dict ready for serialization."""
    created_str = (
        created.isoformat() if isinstance(created, date) else (created or date.today().isoformat())
    )
    return {
        "title": title,
        "created": created_str,
        "source_url": source_url,
        "source_type": source_type,
        "status": status,
        "tags": tags or [],
        "related": related or [],
    }


def render_frontmatter(fm: dict[str, Any]) -> str:
    """Serialize frontmatter dict to YAML block string (including --- delimiters)."""
    import yaml  # type: ignore

    return f"---\n{yaml.dump(fm, allow_unicode=True, default_flow_style=False)}---\n"


def _yaml_available() -> bool:
    try:
        import yaml  # noqa: F401
        return True
    except ImportError:
        return False


def render_frontmatter_simple(fm: dict[str, Any]) -> str:
    """Fallback serializer that doesn't require PyYAML."""
    lines = ["---"]
    for k, v in fm.items():
        if isinstance(v, list):
            if v:
                lines.append(f"{k}:")
                for item in v:
                    lines.append(f"  - {item}")
            else:
                lines.append(f"{k}: []")
        else:
            lines.append(f'{k}: "{v}"')
    lines.append("---")
    return "\n".join(lines) + "\n"


def frontmatter_to_str(fm: dict[str, Any]) -> str:
    if _yaml_available():
        return render_frontmatter(fm)
    return render_frontmatter_simple(fm)
