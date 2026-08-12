"""Locates ffmpeg binaries and verifies they can encode what we need.

Audio uses the NuGet-pinned ffmpeg because builds in the wild routinely lack libvorbis.
Video uses the system ffmpeg only after its required encoders have been verified because
the pinned build is audio-only.
"""

from __future__ import annotations

import platform
import re
import shutil
import subprocess
from collections.abc import Iterable
from pathlib import Path

PACKAGE_ID = "monogame.tool.ffmpeg"


class FfmpegNotFoundError(Exception):
    """The pinned ffmpeg package is not present in the NuGet cache."""


class MissingEncoderError(Exception):
    """The pinned ffmpeg lacks an encoder this pipeline requires."""


def rid_for_platform() -> str:
    """Returns the runtime identifier naming this platform's binaries directory."""
    system = platform.system()
    if system == "Darwin":
        return "osx"
    arch = "arm64" if platform.machine().lower() in {"arm64", "aarch64"} else "x64"
    if system == "Windows":
        return f"windows-{arch}"
    return f"linux-{arch}"


def _version_key(path: Path) -> tuple[int, ...]:
    return tuple(int(p) for p in re.findall(r"\d+", path.name))


def find_pinned_ffmpeg(nuget_root: Path | None = None) -> Path:
    """Returns the highest-versioned pinned ffmpeg binary for this platform."""
    root = nuget_root or (Path.home() / ".nuget" / "packages")
    package = root / PACKAGE_ID
    candidates = []
    if package.is_dir():
        for version in package.iterdir():
            binary = version / "binaries" / rid_for_platform() / "ffmpeg"
            if not binary.exists():
                binary = binary.with_suffix(".exe")
            if binary.exists():
                candidates.append((_version_key(version), binary))
    if not candidates:
        raise FfmpegNotFoundError(
            f"MonoGame.Tool.FFmpeg not found under {package}. "
            "Restore the solution first; the ffmpeg on PATH must not be used."
        )
    return max(candidates)[1]


def find_system_ffmpeg() -> Path:
    """Returns the ffmpeg on PATH.

    Only the video step may use this. The pinned build is audio-only -- no VP9, no VP8,
    no Opus, no WebM muxer -- so for video there is nothing to pin against. Audio must
    keep using `find_pinned_ffmpeg`, because the silent-substitution hazard this module
    exists to prevent is real; for video it is headed off instead by calling
    `require_encoders` before any conversion runs.
    """
    found = shutil.which("ffmpeg")
    if found is None:
        raise FfmpegNotFoundError(
            "No ffmpeg on PATH. Converting the cutscenes needs one that can encode "
            "VP9 and Opus."
        )
    return Path(found)


def available_encoders(ffmpeg: Path) -> set[str]:
    """Returns the encoder names the given ffmpeg binary reports."""
    result = subprocess.run(
        [str(ffmpeg), "-hide_banner", "-encoders"],
        capture_output=True,
        text=True,
        check=False,
    )
    names = set()
    for line in result.stdout.splitlines():
        match = re.match(r"^\s*[A-Z.]{6}\s+(\S+)", line)
        if match:
            names.add(match.group(1))
    return names


def require_encoders(ffmpeg: Path, required: Iterable[str]) -> None:
    """Raises MissingEncoderError unless every required encoder is available."""
    present = available_encoders(ffmpeg)
    missing = sorted(set(required) - present)
    if missing:
        raise MissingEncoderError(
            f"{ffmpeg} cannot encode: {', '.join(missing)}. "
            "This build of ffmpeg is unusable for the web content pipeline."
        )
