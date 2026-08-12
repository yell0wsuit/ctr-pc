"""Font subsetting to the characters the game can actually render.

The full typefaces total 13.4 MB, most of it CJK coverage the game never renders.
Subsetting to the characters actually present in the locale files takes that to ~678 KB.

Output stays TTF rather than WOFF2. WOFF2 would halve it again, but SkiaSharp's
WebAssembly build (4.151.1) compiles FreeType without FT_CONFIG_OPTION_USE_BROTLI -- its
archive has zero woff2 and zero Brotli symbols -- so SKTypeface.FromData cannot decode it.
Setting font.flavor = "woff2" here is the one-line change if that ever returns.
"""

from __future__ import annotations

import hashlib
import io
import json
from collections.abc import Sequence
from pathlib import Path

from fontTools.subset import Options, Subsetter
from fontTools.ttLib import TTFont

from . import manifest

SETTINGS = "ttf:subset:v1"

#: Font file -> the locale codes whose text it must cover. Mirrors
#: CutTheRopeDX.Core/GameMain/Resources.cs FontConfig.GetFontFile.
FONT_LANGUAGES: dict[str, tuple[str, ...]] = {
    "KNMaiyuan-Regular.ttf": ("zh", "zh_tw"),
    "MPLUSRounded1c-Medium.ttf": ("ja",),
    "Cafe24DongdongRegular.otf": ("ko",),
    "PlaypenSans-SemiBold.ttf": ("ru",),
    "gooddog_new-webfont.ttf": (
        "en",
        "ca",
        "de",
        "es",
        "fr",
        "it",
        "nl",
        "pt_br",
    ),
}


def _walk(value: object, into: set[str]) -> None:
    if isinstance(value, str):
        into.update(value)
    elif isinstance(value, dict):
        for key, item in value.items():
            _walk(key, into)
            _walk(item, into)
    elif isinstance(value, list):
        for item in value:
            _walk(item, into)


def collect_charset(locales_dir: Path, languages: Sequence[str]) -> set[str]:
    """Returns every character the given locales can render, plus printable ASCII.

    ASCII is unconditional because scores, level numbers and other generated text are
    never present in the locale files.
    """
    charset = {chr(code) for code in range(0x20, 0x7F)}
    for language in languages:
        path = locales_dir / f"{language}.json"
        if not path.exists():
            continue
        _walk(json.loads(path.read_text(encoding="utf-8")), charset)
    return charset


def subset_font(source: Path, charset: set[str]) -> bytes:
    """Subsets a typeface to `charset` and returns it as TTF."""
    font = TTFont(source)
    options = Options()
    options.layout_features = ["*"]
    options.notdef_outline = True
    subsetter = Subsetter(options=options)
    subsetter.populate(text="".join(sorted(charset)))
    subsetter.subset(font)

    buffer = io.BytesIO()
    font.save(buffer)
    return buffer.getvalue()


def _settings_for_charset(charset: set[str]) -> str:
    encoded = "".join(sorted(charset)).encode("utf-8")
    digest = hashlib.sha256(encoded).hexdigest()
    return f"{SETTINGS}:{len(charset)}:{digest}"


def convert_fonts(
    content_root: Path, out_root: Path, entries: dict[str, str]
) -> tuple[int, int]:
    """Subsets every mapped font, skipping unchanged outputs."""
    converted = 0
    skipped = 0
    locales_dir = content_root / "locales"

    for font_file, languages in sorted(FONT_LANGUAGES.items()):
        source = content_root / "fonts" / font_file
        if not source.exists():
            continue

        relative = Path("fonts") / f"{Path(font_file).stem}.ttf"
        out_rel = relative.as_posix()
        out_path = out_root / relative
        charset = collect_charset(locales_dir, languages)
        stamp = manifest.stamp_for(source, _settings_for_charset(charset))

        if manifest.is_current(entries, out_rel, stamp, out_path):
            skipped += 1
            continue

        out_path.parent.mkdir(parents=True, exist_ok=True)
        out_path.write_bytes(subset_font(source, charset))
        entries[out_rel] = stamp
        converted += 1

    return converted, skipped
