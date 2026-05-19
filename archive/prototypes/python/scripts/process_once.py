#!/usr/bin/env python
"""One-shot: process all unprocessed files currently in Inbox."""

import logging
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")

from llm_wiki.config import INBOX_ROOT  # noqa: E402
from llm_wiki.processors.link_processor import process_link_file  # noqa: E402
from llm_wiki.processors.text_processor import process_text_file  # noqa: E402

LINK_EXTS = {".url", ".txt"}
TEXT_EXTS = {".txt", ".md"}


def main() -> None:
    processed = 0
    failed = 0

    links_dir = INBOX_ROOT / "links"
    for p in links_dir.glob("*"):
        if p.suffix.lower() in LINK_EXTS:
            result = process_link_file(p)
            if result:
                processed += 1
            else:
                failed += 1

    for subdir in ("raw_clips", "mobile"):
        clip_dir = INBOX_ROOT / subdir
        for p in clip_dir.glob("*"):
            if p.suffix.lower() in TEXT_EXTS:
                result = process_text_file(p)
                if result:
                    processed += 1
                else:
                    failed += 1

    print(f"Done. processed={processed} failed={failed}")


if __name__ == "__main__":
    main()
