"""The tier-0 metadata bundle.

604 small text files are fetched before the game constructs anything. Served
individually that is 604 requests -- seconds of latency however small each file is.
Concatenated and minified they are one ~143 KB gzipped request, after which every
non-binary content read is served synchronously from memory.
"""

from __future__ import annotations

import json
from pathlib import Path
from xml.etree import ElementTree

#: Paths that must never enter the bundle, relative to the content root.
EXCLUDED_RELATIVE = ("images/image_dimensions.json",)


def minify_json_bytes(raw: bytes) -> str:
    """Minifies JSON while keeping non-ASCII characters literal.

    ensure_ascii=False is required: the default escapes every CJK character to
    \\uXXXX, which makes the localisation files 16% larger rather than smaller.
    """
    return json.dumps(
        json.loads(raw.decode("utf-8")), separators=(",", ":"), ensure_ascii=False
    )


def _strip_formatting_whitespace(element: ElementTree.Element) -> None:
    if element.text is not None and not element.text.strip():
        element.text = None
    if element.tail is not None and not element.tail.strip():
        element.tail = None
    for child in element:
        _strip_formatting_whitespace(child)


def minify_xml_bytes(raw: bytes) -> str:
    """Minifies XML by removing formatting whitespace before reserialising it."""
    root = ElementTree.fromstring(raw.decode("utf-8"))
    _strip_formatting_whitespace(root)
    return ElementTree.tostring(root, encoding="unicode")


def _sources(content_root: Path) -> list[Path]:
    found = []
    found += sorted((content_root / "images").rglob("*.json"))
    found += sorted((content_root / "images" / "animations").rglob("*.xml"))
    found += sorted((content_root / "maps").rglob("*.xml"))
    found += sorted((content_root / "locales").glob("*.json"))
    for name in ("packlist.json", "ctroriginal_packs.json"):
        candidate = content_root / name
        if candidate.exists():
            found.append(candidate)
    return found


def build_tier0(content_root: Path) -> dict[str, str]:
    """Builds the bundle mapping content-relative paths to minified text."""
    bundle: dict[str, str] = {}
    for source in _sources(content_root):
        relative = source.relative_to(content_root).as_posix()
        if relative in EXCLUDED_RELATIVE:
            continue
        raw = source.read_bytes()
        bundle[relative] = (
            minify_xml_bytes(raw) if source.suffix == ".xml" else minify_json_bytes(raw)
        )
    return bundle


def write_tier0(bundle: dict[str, str], out_path: Path) -> int:
    """Writes the bundle as one JSON object and returns its size in bytes."""
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(
        json.dumps(bundle, separators=(",", ":"), ensure_ascii=False, sort_keys=True),
        encoding="utf-8",
    )
    return out_path.stat().st_size
