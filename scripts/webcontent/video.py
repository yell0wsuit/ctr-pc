"""MP4 to WebM conversion through the system ffmpeg.

VP9 at CRF 32 takes the 720p cutscenes to roughly a fifth of their MP4 size, which is
worth more here than anywhere else in the payload: the two clips are the largest files
the browser build ships.

This is the one converter that cannot use the pinned ffmpeg. That build is audio-only,
so `find_system_ffmpeg` is used instead and the encoders are verified up front.
"""

from __future__ import annotations

import subprocess
from pathlib import Path

from . import manifest, progress

CRF = 32
AUDIO_BITRATE_K = 96
REQUIRED_ENCODERS = ("libvpx-vp9", "libopus")
SETTINGS = f"webm:vp9:crf{CRF}:opus:{AUDIO_BITRATE_K}k"


def webm_command(ffmpeg: Path, source: Path, dest: Path) -> list[str]:
    """Builds the ffmpeg argv for one conversion.

    `-b:v 0` is not optional: without it libvpx-vp9 reads `-crf` as the ceiling of a
    constrained-quality encode rather than as constant quality, and quietly produces a
    much larger file than the one asked for. Same class of trap as `-q:a` in `audio.py`.
    """
    return [
        str(ffmpeg),
        "-y",
        "-v",
        "error",
        "-i",
        str(source),
        "-c:v",
        "libvpx-vp9",
        "-crf",
        str(CRF),
        "-b:v",
        "0",
        "-row-mt",
        "1",
        "-deadline",
        "good",
        "-cpu-used",
        "2",
        "-pix_fmt",
        "yuv420p",
        "-c:a",
        "libopus",
        "-b:a",
        f"{AUDIO_BITRATE_K}k",
        str(dest),
    ]


def convert_videos(
    content_root: Path,
    out_root: Path,
    entries: dict[str, str],
    ffmpeg: Path,
    report: progress.Reporter = progress.SILENT,
) -> tuple[int, int]:
    """Converts every MP4 under content_root/video_hd, skipping unchanged outputs."""
    converted = 0
    skipped = 0
    sources = sorted((content_root / "video_hd").rglob("*.mp4"))
    report.start("video", len(sources))
    try:
        for source in sources:
            relative = source.relative_to(content_root)
            out_relative = relative.with_suffix(".webm")
            out_rel = out_relative.as_posix()
            out_path = out_root / out_relative
            report.advance(out_rel)
            stamp = manifest.stamp_for(source, SETTINGS)

            if manifest.is_current(entries, out_rel, stamp, out_path):
                skipped += 1
                continue

            out_path.parent.mkdir(parents=True, exist_ok=True)
            subprocess.run(webm_command(ffmpeg, source, out_path), check=True)
            entries[out_rel] = stamp
            converted += 1
    finally:
        report.finish()

    return converted, skipped
