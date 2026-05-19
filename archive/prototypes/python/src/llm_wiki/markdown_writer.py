"""Write structured note dicts to Markdown files."""

from __future__ import annotations

from datetime import date
from pathlib import Path
from typing import Any

from llm_wiki.utils.frontmatter import build_frontmatter, frontmatter_to_str
from llm_wiki.utils.slug import dated_slug


NOTE_BODY_TEMPLATE = """\
# 요약
{summary}

## 핵심 내용
{key_points}

## 내 프로젝트와의 관련성
{project_relevance}

## 실행 아이디어
{action_ideas}

## 보존할 원문 정보
{preserved_text}

## 연결 후보
{related_candidates}

## 후속 질문
{follow_up_questions}
"""


def _list_to_md(items: list[str]) -> str:
    if not items:
        return ""
    return "\n".join(f"- {i}" for i in items)


def write_note(
    structured: dict[str, Any],
    source_meta: dict[str, Any],
    output_dir: Path,
) -> Path:
    """Write a structured note dict to a .md file in output_dir.

    Returns the path of the written file.
    """
    title = structured.get("title") or source_meta.get("title", "untitled")
    created = date.today()

    fm = build_frontmatter(
        title=title,
        created=created,
        source_url=source_meta.get("url", ""),
        source_type=source_meta.get("source_type", ""),
        status="processed",
        tags=structured.get("tags", []),
        related=structured.get("related_candidates", []),
    )

    body = NOTE_BODY_TEMPLATE.format(
        summary=structured.get("summary", ""),
        key_points=_list_to_md(structured.get("key_points", [])),
        project_relevance=structured.get("project_relevance", ""),
        action_ideas=_list_to_md(structured.get("action_ideas", [])),
        preserved_text=structured.get("preserved_text", ""),
        related_candidates=_list_to_md(structured.get("related_candidates", [])),
        follow_up_questions=_list_to_md(structured.get("follow_up_questions", [])),
    )

    content = frontmatter_to_str(fm) + "\n" + body

    output_dir.mkdir(parents=True, exist_ok=True)
    filename = dated_slug(title, created) + ".md"
    output_path = output_dir / filename
    output_path.write_text(content, encoding="utf-8")
    return output_path
