import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import manifest


def test_stamp_changes_with_content(tmp_path):
    src = tmp_path / "a.png"
    src.write_bytes(b"one")
    first = manifest.stamp_for(src, "webp:q80")
    src.write_bytes(b"two")
    assert manifest.stamp_for(src, "webp:q80") != first


def test_stamp_changes_with_settings(tmp_path):
    src = tmp_path / "a.png"
    src.write_bytes(b"one")
    assert manifest.stamp_for(src, "webp:q80") != manifest.stamp_for(src, "webp:q90")


def test_load_missing_manifest_returns_empty(tmp_path):
    assert manifest.load_manifest(tmp_path / "nope.json") == {}


def test_load_corrupt_manifest_returns_empty(tmp_path):
    path = tmp_path / "m.json"
    path.write_text("{not json")
    assert manifest.load_manifest(path) == {}


def test_round_trip(tmp_path):
    path = tmp_path / "m.json"
    manifest.save_manifest(path, {"images/a.webp": "abc|webp:q80"})
    assert manifest.load_manifest(path) == {"images/a.webp": "abc|webp:q80"}


def test_write_asset_catalog_groups_every_runtime_asset(tmp_path):
    path = tmp_path / "assets.json"

    manifest.write_asset_catalog(
        path,
        {
            "sounds/tap.ogg": "sound-stamp",
            "images/logo.webp": "image-stamp",
            "fonts/game.ttf": "font-stamp",
        },
    )

    assert json.loads(path.read_text(encoding="utf-8")) == {
        "fonts": ["fonts/game.ttf"],
        "images": ["images/logo.webp"],
        "sounds": ["sounds/tap.ogg"],
    }


def test_write_asset_catalog_omits_url_fetched_groups(tmp_path):
    """Video is fetched by <video> src, not read through the content store."""
    path = tmp_path / "assets.json"

    manifest.write_asset_catalog(
        path,
        {
            "images/logo.webp": "image-stamp",
            "video_hd/ctr_intro.webm": "video-stamp",
        },
    )

    assert json.loads(path.read_text(encoding="utf-8")) == {
        "images": ["images/logo.webp"]
    }


def test_is_current_requires_matching_stamp_and_existing_output(tmp_path):
    out = tmp_path / "a.webp"
    entries = {"a.webp": "abc|webp:q80"}
    assert not manifest.is_current(entries, "a.webp", "abc|webp:q80", out)
    out.write_bytes(b"x")
    assert manifest.is_current(entries, "a.webp", "abc|webp:q80", out)
    assert not manifest.is_current(entries, "a.webp", "different", out)
    assert not manifest.is_current(entries, "missing.webp", "abc|webp:q80", out)
