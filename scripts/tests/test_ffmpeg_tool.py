import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import ffmpeg_tool


def _fake_package(root: Path, version: str, rid: str) -> Path:
    binary = root / "monogame.tool.ffmpeg" / version / "binaries" / rid / "ffmpeg"
    binary.parent.mkdir(parents=True)
    binary.write_text("#!/bin/sh\n")
    binary.chmod(0o755)
    return binary


def test_rid_is_one_of_the_packaged_ids():
    assert ffmpeg_tool.rid_for_platform() in {
        "osx",
        "linux-x64",
        "linux-arm64",
        "windows-x64",
        "windows-arm64",
    }


def test_find_returns_binary_for_current_rid(tmp_path):
    expected = _fake_package(tmp_path, "7.0.0.10", ffmpeg_tool.rid_for_platform())
    assert ffmpeg_tool.find_pinned_ffmpeg(tmp_path) == expected


def test_find_prefers_highest_version(tmp_path):
    rid = ffmpeg_tool.rid_for_platform()
    _fake_package(tmp_path, "7.0.0.9", rid)
    newest = _fake_package(tmp_path, "7.0.0.10", rid)
    assert ffmpeg_tool.find_pinned_ffmpeg(tmp_path) == newest


def test_find_raises_when_absent(tmp_path):
    with pytest.raises(ffmpeg_tool.FfmpegNotFoundError) as excinfo:
        ffmpeg_tool.find_pinned_ffmpeg(tmp_path)
    assert "MonoGame.Tool.FFmpeg" in str(excinfo.value)


def test_require_encoders_raises_listing_the_missing_ones(monkeypatch, tmp_path):
    monkeypatch.setattr(ffmpeg_tool, "available_encoders", lambda _: {"aac"})
    with pytest.raises(ffmpeg_tool.MissingEncoderError) as excinfo:
        ffmpeg_tool.require_encoders(tmp_path / "ffmpeg", ["libvorbis"])
    assert "libvorbis" in str(excinfo.value)


def test_require_encoders_passes_when_present(monkeypatch, tmp_path):
    monkeypatch.setattr(ffmpeg_tool, "available_encoders", lambda _: {"libvorbis"})
    ffmpeg_tool.require_encoders(tmp_path / "ffmpeg", ["libvorbis"])


def test_real_pinned_ffmpeg_has_libvorbis():
    """Guards the whole reason this module exists: PATH ffmpeg often lacks libvorbis."""
    try:
        binary = ffmpeg_tool.find_pinned_ffmpeg()
    except ffmpeg_tool.FfmpegNotFoundError:
        pytest.skip("MonoGame.Tool.FFmpeg not restored")
    assert "libvorbis" in ffmpeg_tool.available_encoders(binary)


def test_find_system_ffmpeg_returns_the_binary_on_path(monkeypatch):
    monkeypatch.setattr(ffmpeg_tool.shutil, "which", lambda _: "/usr/bin/ffmpeg")
    assert ffmpeg_tool.find_system_ffmpeg() == Path("/usr/bin/ffmpeg")


def test_find_system_ffmpeg_raises_when_absent(monkeypatch):
    monkeypatch.setattr(ffmpeg_tool.shutil, "which", lambda _: None)
    with pytest.raises(ffmpeg_tool.FfmpegNotFoundError):
        ffmpeg_tool.find_system_ffmpeg()


def test_pinned_ffmpeg_cannot_encode_video():
    """The reason the video step may not use the pinned binary: it is audio-only."""
    try:
        binary = ffmpeg_tool.find_pinned_ffmpeg()
    except ffmpeg_tool.FfmpegNotFoundError:
        pytest.skip("MonoGame.Tool.FFmpeg not restored")
    assert "libvpx-vp9" not in ffmpeg_tool.available_encoders(binary)
