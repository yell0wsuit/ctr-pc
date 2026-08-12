"""PNG to WebP conversion.

Lossy q80 is the default because it takes the background art to ~3% of its PNG size.
It is a bad trade on the character and font atlases, where it saves almost nothing and
can even grow the file, so anything that fails to compress well falls back to lossless.
The rule is self-tuning: no hand-maintained exclusion list to fall out of date.
"""

from __future__ import annotations

import io
from pathlib import Path

from PIL import Image

from . import manifest, progress

QUALITY = 80
LOSSLESS_FALLBACK_RATIO = 0.60
METHOD = 4
SETTINGS = "webp:q80+ll60"


def pick_encoding(
    png_size: int,
    lossy: bytes,
    lossless: bytes,
    ratio: float = LOSSLESS_FALLBACK_RATIO,
) -> tuple[bytes, str]:
    """Chooses between the lossy and lossless encodings of one image.

    Lossy is kept whenever it compresses well. Once it exceeds `ratio` of the source
    size it has stopped paying for its quality loss, so whichever encoding is actually
    smaller wins.
    """
    if len(lossy) <= ratio * png_size:
        return lossy, "lossy"
    if len(lossless) < len(lossy):
        return lossless, "lossless"
    return lossy, "lossy"


def _load_for_encode(source: Path) -> Image.Image:
    """Opens a PNG as RGBA, dropping to RGB when it is fully opaque."""
    image = Image.open(source).convert("RGBA")
    if image.getchannel("A").getextrema()[0] == 255:
        return image.convert("RGB")
    return image


def encode_webp(source: Path) -> tuple[bytes, str]:
    """Encodes one PNG, returning the chosen bytes and which encoding won."""
    image = _load_for_encode(source)

    lossy_buffer = io.BytesIO()
    image.save(lossy_buffer, "WEBP", quality=QUALITY, method=METHOD)

    lossless_buffer = io.BytesIO()
    image.save(lossless_buffer, "WEBP", lossless=True, method=METHOD)

    return pick_encoding(
        source.stat().st_size, lossy_buffer.getvalue(), lossless_buffer.getvalue()
    )


def convert_images(
    content_root: Path,
    out_root: Path,
    entries: dict[str, str],
    report: progress.Reporter = progress.SILENT,
) -> tuple[int, int]:
    """Converts every PNG under content_root/images, skipping unchanged outputs."""
    converted = 0
    skipped = 0
    sources = sorted((content_root / "images").rglob("*.png"))
    report.start("images", len(sources))
    try:
        for source in sources:
            relative = source.relative_to(content_root).with_suffix(".webp")
            out_rel = relative.as_posix()
            out_path = out_root / relative
            report.advance(out_rel)
            stamp = manifest.stamp_for(source, SETTINGS)

            if manifest.is_current(entries, out_rel, stamp, out_path):
                skipped += 1
                continue

            data, _kind = encode_webp(source)
            out_path.parent.mkdir(parents=True, exist_ok=True)
            out_path.write_bytes(data)
            entries[out_rel] = stamp
            converted += 1
    finally:
        report.finish()

    return converted, skipped
