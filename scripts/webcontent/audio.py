"""WAV to Ogg Vorbis conversion through the pinned ffmpeg.

Music keeps stereo at 192 kbps. Sound effects are short and percussive, so 96 kbps
mono is transparent for them and cuts 21 MB of WAV to under 2 MB.
"""

from __future__ import annotations

import subprocess
from pathlib import Path

from . import manifest, progress

MUSIC_BITRATE_K = 192
SFX_BITRATE_K = 96
SFX_SAMPLE_RATE = 44100
REQUIRED_ENCODERS = ("libvorbis",)


def is_sfx(relative: Path) -> bool:
    """Whether a content-relative audio path is a sound effect rather than music."""
    return "sfx" in relative.parts


def settings_for(relative: Path) -> str:
    """Returns the manifest settings key describing how this file is encoded."""
    if is_sfx(relative):
        return f"ogg:vorbis:{SFX_BITRATE_K}k:mono:{SFX_SAMPLE_RATE}hz"
    return f"ogg:vorbis:{MUSIC_BITRATE_K}k:stereo"


def ogg_command(
    ffmpeg: Path, source: Path, dest: Path, bitrate_k: int, mono: bool
) -> list[str]:
    """Builds the ffmpeg argv for one conversion.

    `-b:a` is used rather than `-q:a`: libvorbis reads `-q:a` as a 0-10 quality scale,
    so passing a kbps figure there is silently wrong.
    """
    command = [str(ffmpeg), "-y", "-v", "error", "-i", str(source)]
    if mono:
        command += ["-ac", "1", "-ar", str(SFX_SAMPLE_RATE)]
    command += ["-c:a", "libvorbis", "-b:a", f"{bitrate_k}k", str(dest)]
    return command


def convert_audio(
    content_root: Path,
    out_root: Path,
    entries: dict[str, str],
    ffmpeg: Path,
    report: progress.Reporter = progress.SILENT,
) -> tuple[int, int]:
    """Converts every WAV under content_root/sounds, skipping unchanged outputs."""
    converted = 0
    skipped = 0
    sources = sorted((content_root / "sounds").rglob("*.wav"))
    report.start("audio", len(sources))
    try:
        for source in sources:
            relative = source.relative_to(content_root)
            out_relative = relative.with_suffix(".ogg")
            out_rel = out_relative.as_posix()
            out_path = out_root / out_relative
            report.advance(out_rel)
            stamp = manifest.stamp_for(source, settings_for(relative))

            if manifest.is_current(entries, out_rel, stamp, out_path):
                skipped += 1
                continue

            out_path.parent.mkdir(parents=True, exist_ok=True)
            sfx = is_sfx(relative)
            subprocess.run(
                ogg_command(
                    ffmpeg,
                    source,
                    out_path,
                    SFX_BITRATE_K if sfx else MUSIC_BITRATE_K,
                    mono=sfx,
                ),
                check=True,
            )
            entries[out_rel] = stamp
            converted += 1
    finally:
        report.finish()

    return converted, skipped
