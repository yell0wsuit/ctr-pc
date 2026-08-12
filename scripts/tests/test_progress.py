import io
import os
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import progress


class _Clock:
    """A monotonic clock the tests advance by hand."""

    def __init__(self) -> None:
        self.now = 100.0

    def __call__(self) -> float:
        return self.now


def _reporter(tty: bool, monkeypatch, interval: float = 2.0):
    clock = _Clock()
    monkeypatch.setattr(progress.time, "monotonic", clock)
    stream = io.StringIO()
    return progress.LiveReporter(stream, interval=interval, tty=tty), stream, clock


def test_silent_reporter_writes_nothing(capsys):
    progress.SILENT.start("images", 3)
    progress.SILENT.advance("images/a.webp")
    progress.SILENT.finish()

    captured = capsys.readouterr()
    assert captured.out == ""
    assert captured.err == ""


def test_log_mode_throttles_to_one_line_per_interval(monkeypatch):
    report, stream, clock = _reporter(tty=False, monkeypatch=monkeypatch)

    report.start("images", 4)
    report.advance("images/a.webp")
    clock.now += 0.5
    report.advance("images/b.webp")
    clock.now += 5.0
    report.advance("images/c.webp")
    report.finish()

    lines = stream.getvalue().splitlines()
    assert len(lines) == 2
    assert lines[0] == "images 1/4  25% 0:00 images/a.webp"
    assert lines[1] == "images 3/4  75% 0:05 images/c.webp"


def test_tty_mode_rewrites_one_line_and_clears_it(monkeypatch):
    report, stream, _clock = _reporter(tty=True, monkeypatch=monkeypatch)

    report.start("audio", 2)
    report.advance("sounds/a.ogg")
    report.advance("sounds/b.ogg")
    report.finish()

    written = stream.getvalue()
    last = "audio 2/2 100% 0:00 sounds/b.ogg"
    assert written.count("\n") == 0
    assert "\raudio 1/2  50% 0:00 sounds/a.ogg" in written
    assert "\r" + last in written
    # The stage ends with the line blanked, so the summary is not printed over it.
    assert written.endswith("\r" + " " * len(last) + "\r")


def test_tty_mode_truncates_to_the_terminal_width(monkeypatch):
    report, stream, _clock = _reporter(tty=True, monkeypatch=monkeypatch)
    monkeypatch.setattr(
        progress.shutil,
        "get_terminal_size",
        lambda _fallback: os.terminal_size((40, 24)),
    )

    report.start("images", 1)
    report.advance("images/a/very/deeply/nested/asset/name.webp")

    line = stream.getvalue().lstrip("\r")
    assert len(line) == 39
    assert line.startswith("...")
    assert line.endswith("name.webp")


def test_empty_stage_reports_nothing(monkeypatch):
    report, stream, _clock = _reporter(tty=False, monkeypatch=monkeypatch)

    report.start("video", 0)
    report.finish()

    assert stream.getvalue() == ""
