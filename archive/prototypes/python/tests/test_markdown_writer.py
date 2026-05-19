import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

import tempfile
from datetime import date
from llm_wiki.markdown_writer import write_note


SAMPLE_STRUCTURED = {
    "title": "Test Note",
    "summary": "This is a summary.",
    "key_points": ["Point A", "Point B"],
    "project_relevance": "Relevant to LLM wiki.",
    "action_ideas": ["Do X", "Try Y"],
    "preserved_text": "Original snippet.",
    "related_candidates": ["Note A"],
    "follow_up_questions": ["Why?"],
    "tags": ["test", "llm"],
}

SAMPLE_META = {
    "url": "https://example.com",
    "source_type": "webpage",
    "title": "Test Note",
}


def test_write_note_creates_file():
    with tempfile.TemporaryDirectory() as tmpdir:
        out_dir = Path(tmpdir)
        path = write_note(SAMPLE_STRUCTURED, SAMPLE_META, out_dir)
        assert path.exists()
        assert path.suffix == ".md"


def test_write_note_contains_frontmatter():
    with tempfile.TemporaryDirectory() as tmpdir:
        path = write_note(SAMPLE_STRUCTURED, SAMPLE_META, Path(tmpdir))
        content = path.read_text(encoding="utf-8")
        assert "---" in content
        assert "title" in content


def test_write_note_contains_sections():
    with tempfile.TemporaryDirectory() as tmpdir:
        path = write_note(SAMPLE_STRUCTURED, SAMPLE_META, Path(tmpdir))
        content = path.read_text(encoding="utf-8")
        assert "# 요약" in content
        assert "## 핵심 내용" in content
        assert "## 후속 질문" in content


def test_write_note_filename_has_date():
    with tempfile.TemporaryDirectory() as tmpdir:
        path = write_note(SAMPLE_STRUCTURED, SAMPLE_META, Path(tmpdir))
        today = date.today().isoformat()
        assert path.name.startswith(today)


def test_write_note_fallback_title():
    structured = {**SAMPLE_STRUCTURED, "title": ""}
    meta = {**SAMPLE_META, "title": "fallback-title"}
    with tempfile.TemporaryDirectory() as tmpdir:
        path = write_note(structured, meta, Path(tmpdir))
        assert "fallback" in path.name
