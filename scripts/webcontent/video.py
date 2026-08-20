"""MP4 to WebM conversion through the system ffmpeg.

VP9 at CRF 32 takes the 720p cutscenes to roughly a fifth of their MP4 size, which is
worth more here than anywhere else in the payload: the two clips are the largest files
the browser build ships.

This is the one converter that cannot use the pinned ffmpeg. That build is audio-only,
so `find_system_ffmpeg` is used instead and the encoders are verified up front.
"""

from __future__ import annotations

import functools
import subprocess
from collections.abc import Iterator
from pathlib import Path

from . import pipeline, progress

CRF = 32
AUDIO_BITRATE_K = 96
REQUIRED_ENCODERS = ("libvpx-vp9", "libopus")
SETTINGS = f"webm:vp9:crf{CRF}:opus:{AUDIO_BITRATE_K}k"

#: Concurrent encodes. Deliberately low: `-row-mt 1` already spreads one clip across
#: the machine, so running many at once trades cores between them rather than adding any.
MAX_CONCURRENT = 2


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


def write_webm(job: pipeline.Job, ffmpeg: Path) -> None:
    """Converts one job. Runs in a pool worker."""
    subprocess.run(webm_command(ffmpeg, job.source, job.out_path), check=True)


def _jobs(content_root: Path, out_root: Path) -> Iterator[pipeline.Job]:
    for source in sorted((content_root / "video_hd").rglob("*.mp4")):
        relative = source.relative_to(content_root).with_suffix(".webm")
        yield pipeline.Job(source, relative.as_posix(), out_root / relative, SETTINGS)


def convert_videos(
    content_root: Path,
    out_root: Path,
    entries: dict[str, str],
    ffmpeg: Path,
    report: progress.Reporter = progress.SILENT,
) -> tuple[int, int]:
    """Converts every MP4 under content_root/video_hd, skipping unchanged outputs."""
    return pipeline.run_stage(
        "video",
        _jobs(content_root, out_root),
        functools.partial(write_webm, ffmpeg=ffmpeg),
        entries,
        report,
        cpu_bound=False,
        max_workers=MAX_CONCURRENT,
    )
