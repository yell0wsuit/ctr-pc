"""Live per-file progress for the converters.

A cold run converts 202 images, 187 sounds and two VP9 cutscenes, and until now said
nothing at all until each stage had finished. Every converter now announces the file it
is about to handle.

Two rendering modes, because a line rewritten with a carriage return is unreadable
anywhere without a cursor: a terminal gets one line updated in place, while a log
(GitHub Actions, a pipe, a redirect) gets append-only lines throttled to roughly one
every few seconds, so a long build stays traceable without burying the summary.
"""

from __future__ import annotations

import shutil
import sys
import time
from typing import IO

#: Minimum gap between two lines when the output is a log rather than a terminal.
LOG_INTERVAL_SECONDS = 2.0

#: Fallback width when the terminal size cannot be determined.
FALLBACK_COLUMNS = 80


def _format_elapsed(seconds: float) -> str:
    """Formats an elapsed duration as M:SS."""
    total = int(seconds)
    return f"{total // 60}:{total % 60:02d}"


class Reporter:
    """Accepts every call and renders nothing.

    The default for library callers and tests, and the null object `LiveReporter`
    replaces when the pipeline is asked for progress.
    """

    def start(self, label: str, total: int) -> None:
        """Begins a stage covering `total` files."""

    def advance(self, name: str) -> None:
        """Announces the file about to be converted or skipped."""

    def finish(self) -> None:
        """Ends the stage."""


SILENT = Reporter()


class LiveReporter(Reporter):
    """Renders progress for one stage at a time."""

    def __init__(
        self,
        stream: IO[str] | None = None,
        interval: float = LOG_INTERVAL_SECONDS,
        tty: bool | None = None,
    ) -> None:
        self._stream = sys.stderr if stream is None else stream
        self._interval = interval
        self._tty = self._stream.isatty() if tty is None else tty
        self._label = ""
        self._total = 0
        self._index = 0
        self._started = 0.0
        self._last_write = 0.0
        self._painted = 0

    def start(self, label: str, total: int) -> None:
        self._label = label
        self._total = total
        self._index = 0
        self._started = time.monotonic()
        self._last_write = 0.0
        self._painted = 0

    def advance(self, name: str) -> None:
        self._index += 1
        if self._total <= 0:
            return
        now = time.monotonic()
        # A terminal repaints the same line, so it can afford every file. A log cannot:
        # the first file of a stage goes out immediately, the rest are throttled.
        if (
            not self._tty
            and self._index > 1
            and now - self._last_write < self._interval
        ):
            return
        self._last_write = now
        self._write(self._line(name))

    def finish(self) -> None:
        """Clears the in-place line a terminal is left holding."""
        if self._tty and self._painted:
            self._stream.write("\r" + " " * self._painted + "\r")
            self._stream.flush()
        self._painted = 0

    def _line(self, name: str) -> str:
        percent = self._index * 100 // self._total
        elapsed = _format_elapsed(time.monotonic() - self._started)
        return (
            f"{self._label} {self._index}/{self._total} "
            f"{percent:3d}% {elapsed} {name}"
        )

    def _write(self, line: str) -> None:
        if not self._tty:
            self._stream.write(line + "\n")
            self._stream.flush()
            return
        # One column is left free: writing the last cell wraps on some terminals.
        width = max(shutil.get_terminal_size((FALLBACK_COLUMNS, 24)).columns - 1, 20)
        if len(line) > width:
            line = "..." + line[len(line) - width + 3 :]
        self._stream.write("\r" + line.ljust(self._painted))
        self._stream.flush()
        self._painted = len(line)
