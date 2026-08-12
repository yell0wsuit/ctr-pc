import io
import json
import sys
from pathlib import Path

import pytest
from fontTools.ttLib import TTFont

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import fonts

REPO_ROOT = Path(__file__).resolve().parents[2]
CONTENT = REPO_ROOT / "content"


def test_mapping_matches_resources_cs():
    """The font-per-language mapping must track Core's FontConfig constants."""
    source = (
        REPO_ROOT
        / "src"
        / "CutTheRopeDX.Core"
        / "GameMain"
        / "Resources.cs"
    ).read_text(encoding="utf-8")
    for font_file in fonts.FONT_LANGUAGES:
        assert f'"{font_file}"' in source, f"{font_file} no longer named in Resources.cs"


def test_charset_always_includes_printable_ascii(tmp_path):
    (tmp_path / "en.json").write_text(json.dumps({"a": "hi"}), encoding="utf-8")
    charset = fonts.collect_charset(tmp_path, ["en"])
    assert " " in charset and "~" in charset and "0" in charset


def test_charset_collects_nested_values_and_keys(tmp_path):
    (tmp_path / "zh.json").write_text(
        json.dumps({"outer": {"inner": ["你好"]}}, ensure_ascii=False),
        encoding="utf-8",
    )
    charset = fonts.collect_charset(tmp_path, ["zh"])
    assert "你" in charset and "好" in charset


def test_charset_ignores_missing_locale_files(tmp_path):
    charset = fonts.collect_charset(tmp_path, ["nope"])
    assert "A" in charset


@pytest.mark.skipif(
    not (CONTENT / "fonts" / "gooddog_new-webfont.ttf").exists(),
    reason="binary font assets not fetched",
)
def test_subset_produces_smaller_font_that_keeps_requested_glyphs():
    source = CONTENT / "fonts" / "gooddog_new-webfont.ttf"
    charset = set("AB0 ")
    data = fonts.subset_font(source, charset)

    assert len(data) < source.stat().st_size
    font = TTFont(io.BytesIO(data))
    assert font.flavor is None
    cmap = font.getBestCmap()
    assert ord("A") in cmap and ord("B") in cmap


def test_equal_length_charset_change_reconverts_font(monkeypatch, tmp_path):
    content = tmp_path / "content"
    fonts_dir = content / "fonts"
    locales_dir = content / "locales"
    fonts_dir.mkdir(parents=True)
    locales_dir.mkdir()
    (fonts_dir / "test.ttf").write_bytes(b"font")
    locale = locales_dir / "zh.json"
    locale.write_text(json.dumps({"text": "你"}, ensure_ascii=False), encoding="utf-8")

    monkeypatch.setattr(fonts, "FONT_LANGUAGES", {"test.ttf": ("zh",)})
    monkeypatch.setattr(
        fonts,
        "subset_font",
        lambda _source, charset: "".join(sorted(charset)).encode("utf-8"),
    )

    out = tmp_path / "out"
    entries: dict[str, str] = {}
    assert fonts.convert_fonts(content, out, entries) == (1, 0)
    assert fonts.convert_fonts(content, out, entries) == (0, 1)

    locale.write_text(json.dumps({"text": "好"}, ensure_ascii=False), encoding="utf-8")
    assert fonts.convert_fonts(content, out, entries) == (1, 0)
