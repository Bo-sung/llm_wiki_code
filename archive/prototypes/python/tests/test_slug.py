import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

from datetime import date
from llm_wiki.utils.slug import make_slug, dated_slug


def test_make_slug_basic():
    assert make_slug("Hello World") == "hello-world"


def test_make_slug_special_chars():
    result = make_slug("C++: Best Practices & Tips!")
    assert " " not in result
    assert result != ""


def test_make_slug_empty():
    assert make_slug("") == "untitled"


def test_make_slug_max_len():
    long_title = "a" * 100
    result = make_slug(long_title, max_len=20)
    assert len(result) <= 20


def test_dated_slug_format():
    d = date(2026, 5, 19)
    result = dated_slug("My Note", created=d)
    assert result.startswith("2026-05-19-")


def test_dated_slug_default_date():
    result = dated_slug("Test Note")
    today = date.today().isoformat()
    assert result.startswith(today)
