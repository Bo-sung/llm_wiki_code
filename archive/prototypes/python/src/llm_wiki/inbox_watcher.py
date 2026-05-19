"""Watchdog-based watcher for the Inbox directory."""

from __future__ import annotations

import logging
import time
from pathlib import Path

from llm_wiki.config import INBOX_ROOT

logger = logging.getLogger(__name__)

WATCHED_EXTENSIONS = {".txt", ".md", ".url"}


def _dispatch(path: Path) -> None:
    from llm_wiki.processors.link_processor import process_link_file
    from llm_wiki.processors.text_processor import process_text_file

    parent_name = path.parent.name
    if parent_name == "links" or path.suffix == ".url":
        process_link_file(path)
    elif parent_name in ("raw_clips", "mobile"):
        process_text_file(path)
    else:
        logger.info("No processor for %s — skipping", path)


def _make_handler():
    from watchdog.events import FileSystemEventHandler  # type: ignore

    class _Handler(FileSystemEventHandler):
        def on_created(self, event):
            if event.is_directory:
                return
            p = Path(event.src_path)
            if p.suffix.lower() in WATCHED_EXTENSIONS:
                logger.info("Detected new file: %s", p)
                _dispatch(p)

    return _Handler()


def run_watcher(inbox: Path = INBOX_ROOT) -> None:
    """Block and watch inbox for new files. Ctrl-C to stop."""
    from watchdog.observers import Observer  # type: ignore

    handler = _make_handler()
    observer = Observer()
    observer.schedule(handler, str(inbox), recursive=True)
    observer.start()
    logger.info("Watching %s ...", inbox)
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()
