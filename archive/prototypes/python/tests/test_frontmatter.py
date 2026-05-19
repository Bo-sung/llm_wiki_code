import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

from datetime import date
from llm_wiki.utils.frontmatter import build_frontmatter, frontmatter_to_str, render_frontmatter_simple


def test_build_frontmatter_defaults():
    fm = build_frontmatter(title="Test")
    assert fm["title"] == "Test"
    assert fm["status"] == "processed"
    assert fm["tags"] == []
    assert fm["related"] == []


def test_build_frontmatter_date_obj():
    fm = build_frontmatter(title="T", created=date(2026, 1, 15))
    assert fm["created"] == "2026-01-15"


def test_build_frontmatter_date_str():
    fm = build_frontmatter(title="T", created="2026-03-01")
    assert fm["created"] == "2026-03-01"


def test_render_frontmatter_simple_contains_delimiters():
    fm = build_frontmatter(title="A Note", tags=["ai", "llm"])
    rendered = render_frontmatter_simple(fm)
    assert rendered.startswith("---")
    assert "---" in rendered[3:]


def test_render_frontmatter_simple_list():
    fm = {"tags": ["a", "b"], "title": "X", "created": "2026-05-19",
          "source_url": "", "source_type": "", "status": "processed", "related": []}
    rendered = render_frontmatter_simple(fm)
    assert "- a" in rendered
    assert "- b" in rendered


def test_frontmatter_to_str_returns_string():
    fm = build_frontmatter(title="Hello")
    result = frontmatter_to_str(fm)
    assert isinstance(result, str)
    assert "---" in result
