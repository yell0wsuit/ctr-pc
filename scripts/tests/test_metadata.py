import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import metadata


def test_json_minification_preserves_non_ascii_literally():
    """ensure_ascii=True would expand CJK to \\uXXXX and grow the locale files."""
    raw = json.dumps({"k": "你好"}, ensure_ascii=False, indent=4).encode("utf-8")
    minified = metadata.minify_json_bytes(raw)
    assert "你好" in minified
    assert "\\u" not in minified
    assert len(minified.encode("utf-8")) < len(raw)


def test_json_minification_drops_whitespace():
    raw = b'{\n    "a" : 1,\n    "b" : 2\n}'
    assert metadata.minify_json_bytes(raw) == '{"a":1,"b":2}'


def test_xml_minification_returns_parseable_xml():
    raw = b"<root>\n  <child a='1' />\n</root>"
    minified = metadata.minify_xml_bytes(raw)
    assert "<child" in minified
    assert "\n" not in minified
    assert len(minified) <= len(raw.decode("utf-8"))


def _seed(content: Path) -> None:
    (content / "images").mkdir(parents=True)
    (content / "images" / "animations").mkdir()
    (content / "maps").mkdir(parents=True)
    (content / "locales").mkdir(parents=True)
    (content / "images" / "atlas.json").write_text('{\n  "frames": []\n}')
    (content / "images" / "animations" / "splash.xml").write_text(
        "<FlashAnimation width='10' height='20' />"
    )
    (content / "images" / "image_dimensions.json").write_text('{"a": [1, 2]}')
    (content / "maps" / "1_1.xml").write_text("<map>\n  <o />\n</map>")
    (content / "locales" / "en.json").write_text('{\n  "hi": "there"\n}')
    (content / "packlist.json").write_text('{\n  "packs": []\n}')
    (content / "ctroriginal_packs.json").write_text("{}")


def test_bundle_includes_expected_files(tmp_path):
    content = tmp_path / "content"
    _seed(content)
    bundle = metadata.build_tier0(content)
    assert "images/atlas.json" in bundle
    assert "images/animations/splash.xml" in bundle
    assert "maps/1_1.xml" in bundle
    assert "locales/en.json" in bundle
    assert "packlist.json" in bundle
    assert "ctroriginal_packs.json" in bundle


def test_bundle_excludes_image_dimensions(tmp_path):
    """image_dimensions.json serves HeadlessAssetPlatform only; the browser reads SKImage."""
    content = tmp_path / "content"
    _seed(content)
    assert "images/image_dimensions.json" not in metadata.build_tier0(content)


def test_bundle_values_are_minified(tmp_path):
    content = tmp_path / "content"
    _seed(content)
    assert metadata.build_tier0(content)["locales/en.json"] == '{"hi":"there"}'


def test_write_tier0_is_loadable_json(tmp_path):
    content = tmp_path / "content"
    _seed(content)
    out = tmp_path / "out" / "tier0.json"
    written = metadata.write_tier0(metadata.build_tier0(content), out)
    assert written == out.stat().st_size
    reloaded = json.loads(out.read_text(encoding="utf-8"))
    assert reloaded["locales/en.json"] == '{"hi":"there"}'
