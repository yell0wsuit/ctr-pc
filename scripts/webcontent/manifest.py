"""Incremental-build state shared by every converter.

An entry maps an output path (relative to the output root) to a stamp combining the
source file's SHA-256 and the encoder settings used. Changing either invalidates the
output, so a quality tweak rebuilds without needing a clean.
"""

from __future__ import annotations

import hashlib
import json
from pathlib import Path


def stamp_for(source: Path, settings: str) -> str:
    """Returns the stamp identifying one conversion of `source` under `settings`."""
    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    return f"{digest}|{settings}"


def load_manifest(path: Path) -> dict[str, str]:
    """Loads the manifest, treating a missing or unreadable file as empty.

    A corrupt manifest must not abort the build: the correct recovery is a full
    rebuild, which an empty dict produces.
    """
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, ValueError):
        return {}
    return data if isinstance(data, dict) else {}


def save_manifest(path: Path, entries: dict[str, str]) -> None:
    """Writes the manifest, creating parent directories as needed."""
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        json.dumps(entries, indent=2, sort_keys=True, ensure_ascii=False),
        encoding="utf-8",
    )


# Top-level groups the browser fetches by URL rather than reading through the content
# store. Listing one here would make the store preload it into a byte cache nothing
# ever reads, and stall the loading screen on the download.
URL_FETCHED_GROUPS = frozenset({"video_hd"})


def write_asset_catalog(path: Path, entries: dict[str, str]) -> None:
    """Writes public runtime asset paths grouped by their top-level directory."""
    groups: dict[str, list[str]] = {}
    for relative_path in sorted(entries):
        asset_type, separator, _ = relative_path.partition("/")
        if separator and asset_type not in URL_FETCHED_GROUPS:
            groups.setdefault(asset_type, []).append(relative_path)

    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        json.dumps(groups, indent=2, sort_keys=True, ensure_ascii=False),
        encoding="utf-8",
    )


def is_current(
    entries: dict[str, str], out_rel: str, stamp: str, out_path: Path
) -> bool:
    """Whether a previously converted output is still valid and present on disk."""
    return entries.get(out_rel) == stamp and out_path.exists()
