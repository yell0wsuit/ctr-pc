#!/usr/bin/env python3
"""Bundle content assets into ctrdx-assets.zip and write file_manifest.json.

Maintainer-side release tooling. Packs the content folders the build fetches —
fonts/, images/, sounds/, video_hd/ plus the two desktop cursors — into a single
max-compressed zip (a full snapshot of those folders, minus .DS_Store), and emits
a SHA-256 manifest the build uses to detect missing assets and verify a download.

    python scripts/bundle_content.py content/

The zip mirrors the folders as-is, including the git-tracked json/xml metadata, so
it doubles as a complete snapshot. The manifest, however, lists ONLY binary assets
(the 9 fetched extensions): the build copies just those out of the bundle, leaving
git-tracked text to come from Git — so a stale bundle can never clobber it.
"""

import argparse
import hashlib
import json
import sys
import zipfile
from pathlib import Path

BINARY_EXTS = {".png", ".wav", ".ogg", ".mp4", ".ttf", ".otf", ".cur", ".xnb", ".wmv"}
INCLUDE_DIRS = ("fonts", "images", "sounds", "video_hd")
INCLUDE_FILES = ("cursor_windows.cur", "cursor_active_windows.cur")
EXCLUDE_NAMES = {".DS_Store"}
MANIFEST_NAME = "file_manifest.json"
CHUNK = 1 << 20  # 1 MiB


def _sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(CHUNK), b""):
            h.update(chunk)
    return h.hexdigest()


def collect(content: Path) -> list[Path]:
    """All in-scope files to bundle (excluding EXCLUDE_NAMES), as absolute paths."""
    found: list[Path] = []
    for name in INCLUDE_DIRS:
        root = content / name
        if not root.is_dir():
            print(f"warning: missing folder {root}", file=sys.stderr)
            continue
        found += [p for p in root.rglob("*") if p.is_file() and p.name not in EXCLUDE_NAMES]
    for name in INCLUDE_FILES:
        f = content / name
        if f.is_file():
            found.append(f)
        else:
            print(f"warning: missing file {f}", file=sys.stderr)
    return found


def main() -> int:
    parser = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("content_dir", type=Path, help="path to the content/ tree")
    parser.add_argument(
        "-o", "--output", type=Path, default=Path("ctrdx-assets.zip"),
        help="output zip path (default: ./ctrdx-assets.zip)")
    parser.add_argument(
        "-m", "--manifest", type=Path, default=None,
        help=f"manifest path (default: <content_dir>/{MANIFEST_NAME})")
    args = parser.parse_args()

    content = args.content_dir.resolve()
    if not content.is_dir():
        print(f"error: not a directory: {content}", file=sys.stderr)
        return 1

    files = sorted(collect(content), key=lambda p: p.relative_to(content).as_posix())
    if not files:
        print("error: no files found to bundle", file=sys.stderr)
        return 1

    # Manifest covers only the binary assets the build fetches.
    manifest = {
        p.relative_to(content).as_posix(): _sha256(p)
        for p in files if p.suffix.lower() in BINARY_EXTS
    }
    manifest_path = args.manifest or (content / MANIFEST_NAME)
    manifest_path.write_text(json.dumps({"files": manifest}, indent=4) + "\n", encoding="utf-8")
    print(f"Wrote {len(manifest)} binary entries to {manifest_path}")

    # Zip the full snapshot (max deflate), with the manifest at the root.
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(args.output, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        for p in files:
            zf.write(p, p.relative_to(content).as_posix())
        zf.write(manifest_path, MANIFEST_NAME)

    size_mb = args.output.stat().st_size / (1 << 20)
    print(f"Zipped {len(files)} files + {MANIFEST_NAME} to {args.output} ({size_mb:.1f} MiB)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
