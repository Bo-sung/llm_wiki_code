"""Gemini API client wrapper.

Keeps API calls isolated so the rest of the pipeline can run even when
the key is missing or the call fails.
"""

from __future__ import annotations

import logging
from pathlib import Path
from typing import Any

from llm_wiki.config import GEMINI_API_KEY, REFINE_NOTE_PROMPT

logger = logging.getLogger(__name__)

_FALLBACK: dict[str, Any] = {
    "title": "",
    "summary": "",
    "key_points": [],
    "project_relevance": "",
    "action_ideas": [],
    "preserved_text": "",
    "related_candidates": [],
    "follow_up_questions": [],
    "tags": [],
    "error": "gemini_unavailable",
}


def _load_prompt() -> str:
    if REFINE_NOTE_PROMPT.exists():
        return REFINE_NOTE_PROMPT.read_text(encoding="utf-8")
    return (
        "Summarize the following content and return a JSON object with keys: "
        "title, summary, key_points, project_relevance, action_ideas, "
        "preserved_text, related_candidates, follow_up_questions, tags."
    )


def summarize_and_structure(raw_text: str, source_meta: dict[str, Any]) -> dict[str, Any]:
    """Call Gemini to summarize and structure raw text.

    Returns a dict with structured note fields.
    On any failure, returns a fallback dict with error key set.
    """
    if not GEMINI_API_KEY:
        logger.warning("GEMINI_API_KEY not set — returning fallback")
        return {**_FALLBACK, "error": "no_api_key"}

    try:
        import google.generativeai as genai  # type: ignore
        import json

        genai.configure(api_key=GEMINI_API_KEY)

        # Model name is intentionally not hardcoded to a specific version.
        # DECISION NEEDED: which Gemini model to use (see reports/setup_report.md)
        model = genai.GenerativeModel("gemini-1.5-flash")

        prompt = _load_prompt()
        full_prompt = (
            f"{prompt}\n\n"
            f"Source metadata: {source_meta}\n\n"
            f"Content:\n{raw_text}"
        )

        response = model.generate_content(full_prompt)
        text = response.text.strip()

        # Strip markdown code fences if present
        if text.startswith("```"):
            lines = text.splitlines()
            text = "\n".join(lines[1:-1] if lines[-1] == "```" else lines[1:])

        result: dict[str, Any] = json.loads(text)
        return result

    except ImportError:
        logger.error("google-generativeai not installed")
        return {**_FALLBACK, "error": "library_not_installed"}
    except Exception as exc:  # noqa: BLE001
        logger.error("Gemini call failed: %s", exc)
        return {**_FALLBACK, "error": str(exc)}
