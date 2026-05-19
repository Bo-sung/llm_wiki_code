#!/usr/bin/env python
"""Start the Inbox watcher (blocking)."""

import logging
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")

from llm_wiki.inbox_watcher import run_watcher  # noqa: E402

if __name__ == "__main__":
    run_watcher()
