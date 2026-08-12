import subprocess
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import manifest, video


def _arg_after(command: list[str], flag: str) -> str:
    return command[command.index(flag) + 1]


def test_command_encodes_vp9_and_opus():
    command = video.webm_command(Path("/ff"), Path("in.mp4"), Path("out.webm"))
    assert command[0] == "/ff"
    assert _arg_after(command, "-c:v") == "libvpx-vp9"
    assert _arg_after(command, "-c:a") == "libopus"
    assert _arg_after(command, "-b:a") == "96k"
    assert command[-1] == "out.webm"


def test_command_pairs_crf_with_zero_video_bitrate():
    """Without -b:v 0, libvpx-vp9 reads -crf as a ceiling, not as constant quality."""
    command = video.webm_command(Path("/ff"), Path("in.mp4"), Path("out.webm"))
    assert _arg_after(command, "-crf") == "32"
    assert _arg_after(command, "-b:v") == "0"


def test_settings_key_names_every_quality_parameter():
    assert video.SETTINGS == "webm:vp9:crf32:opus:96k"


def test_required_encoders_are_the_ones_the_command_uses():
    assert set(video.REQUIRED_ENCODERS) == {"libvpx-vp9", "libopus"}


def _seed_video(content: Path, name: str = "ctr_intro.mp4") -> Path:
    source = content / "video_hd" / name
    source.parent.mkdir(parents=True, exist_ok=True)
    source.write_bytes(b"not really an mp4")
    return source


def test_convert_writes_output_and_records_a_stamp(tmp_path, monkeypatch):
    content = tmp_path / "content"
    out = tmp_path / "out"
    source = _seed_video(content)
    entries: dict[str, str] = {}

    def fake_run(command, check):
        Path(command[-1]).write_bytes(b"webm")

    monkeypatch.setattr(subprocess, "run", fake_run)
    converted, skipped = video.convert_videos(content, out, entries, Path("/ff"))

    assert (converted, skipped) == (1, 0)
    assert (out / "video_hd" / "ctr_intro.webm").exists()
    assert entries["video_hd/ctr_intro.webm"] == manifest.stamp_for(
        source, video.SETTINGS
    )


def test_convert_skips_an_unchanged_source(tmp_path, monkeypatch):
    content = tmp_path / "content"
    out = tmp_path / "out"
    _seed_video(content)
    entries: dict[str, str] = {}

    def fake_run(command, check):
        Path(command[-1]).write_bytes(b"webm")

    monkeypatch.setattr(subprocess, "run", fake_run)
    video.convert_videos(content, out, entries, Path("/ff"))

    def explode(command, check):
        raise AssertionError("an unchanged source must not be reconverted")

    monkeypatch.setattr(subprocess, "run", explode)
    assert video.convert_videos(content, out, entries, Path("/ff")) == (0, 1)


def test_convert_handles_a_content_tree_with_no_videos(tmp_path):
    assert video.convert_videos(
        tmp_path / "content", tmp_path / "out", {}, Path("/ff")
    ) == (
        0,
        0,
    )
