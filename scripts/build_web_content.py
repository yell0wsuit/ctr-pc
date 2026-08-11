#!/usr/bin/env python3
"""Converts the desktop content tree into the browser payload.

Four jobs: PNG to WebP, WAV to Ogg Vorbis, font subsetting, and the tier-0
metadata bundle. Every job is incremental, so a rerun after changing one asset
reconverts only that asset.

Usage:
    python3 scripts/build_web_content.py
    python3 scripts/build_web_content.py --skip-audio
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from webcontent import audio, ffmpeg_tool, fonts, images, manifest, metadata

sys.path.insert(0, str(Path(__file__).resolve().parent))

REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SOURCE = REPO_ROOT / "content"
DEFAULT_OUT = REPO_ROOT / "src" / "CutTheRopeDX.Browser" / "wwwroot" / "content"
MANIFEST_NAME = ".build-manifest.json"


def _parse_args(argv: list[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--out", type=Path, default=DEFAULT_OUT)
    parser.add_argument(
        "--skip-audio",
        action="store_true",
        help="Skip WAV conversion; useful when the pinned ffmpeg is unavailable.",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    """Runs the pipeline. Returns a process exit code."""
    args = _parse_args(argv)
    source: Path = args.source
    out: Path = args.out

    if not source.is_dir():
        print(f"error: content source not found: {source}", file=sys.stderr)
        return 1

    manifest_path = out / MANIFEST_NAME
    entries = manifest.load_manifest(manifest_path)

    converted, skipped = images.convert_images(source, out, entries)
    print(f"images: {converted} converted, {skipped} skipped")

    if args.skip_audio:
        print("audio: skipped")
    else:
        try:
            ffmpeg = ffmpeg_tool.find_pinned_ffmpeg()
            ffmpeg_tool.require_encoders(ffmpeg, audio.REQUIRED_ENCODERS)
        except (
            ffmpeg_tool.FfmpegNotFoundError,
            ffmpeg_tool.MissingEncoderError,
        ) as error:
            print(f"error: {error}", file=sys.stderr)
            return 2
        converted, skipped = audio.convert_audio(source, out, entries, ffmpeg)
        print(f"audio: {converted} converted, {skipped} skipped")

    converted, skipped = fonts.convert_fonts(source, out, entries)
    print(f"fonts: {converted} converted, {skipped} skipped")

    size = metadata.write_tier0(metadata.build_tier0(source), out / "tier0.json")
    print(f"tier0: {size / 1024:.0f} KB")

    manifest.save_manifest(manifest_path, entries)
    runtime_entries = {
        relative_path: stamp
        for relative_path, stamp in entries.items()
        if (out / relative_path).is_file()
    }
    manifest.write_asset_catalog(out / "assets.json", runtime_entries)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
