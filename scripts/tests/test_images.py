import io
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import images


def test_lossy_wins_when_comfortably_smaller():
    chosen, kind = images.pick_encoding(1000, b"x" * 100, b"y" * 900)
    assert kind == "lossy"
    assert chosen == b"x" * 100


def test_lossless_wins_when_lossy_exceeds_ratio_and_lossless_is_smaller():
    chosen, kind = images.pick_encoding(1000, b"x" * 900, b"y" * 500)
    assert kind == "lossless"
    assert chosen == b"y" * 500


def test_lossy_kept_when_over_ratio_but_still_smaller_than_lossless():
    chosen, kind = images.pick_encoding(1000, b"x" * 700, b"y" * 950)
    assert kind == "lossy"


def test_boundary_at_exactly_the_ratio_keeps_lossy():
    chosen, kind = images.pick_encoding(1000, b"x" * 600, b"y" * 100)
    assert kind == "lossy"


def _write_png(path: Path, size, color) -> None:
    Image.new("RGBA", size, color).save(path, "PNG")


def test_encode_webp_round_trips_dimensions(tmp_path):
    src = tmp_path / "a.png"
    _write_png(src, (64, 48), (10, 200, 30, 255))
    data, _ = images.encode_webp(src)
    decoded = Image.open(io.BytesIO(data))
    assert decoded.size == (64, 48)
    assert decoded.format == "WEBP"


def test_encode_webp_preserves_alpha_exactly(tmp_path):
    src = tmp_path / "a.png"
    Image.new("RGBA", (32, 32), (255, 0, 0, 0)).save(src, "PNG")
    data, _ = images.encode_webp(src)
    decoded = Image.open(io.BytesIO(data)).convert("RGBA")
    assert decoded.getchannel("A").getextrema() == (0, 0)


def test_convert_images_writes_webp_and_skips_second_run(tmp_path):
    content = tmp_path / "content"
    (content / "images" / "sub").mkdir(parents=True)
    _write_png(content / "images" / "a.png", (16, 16), (1, 2, 3, 255))
    _write_png(content / "images" / "sub" / "b.png", (16, 16), (4, 5, 6, 255))
    out = tmp_path / "out"
    entries: dict[str, str] = {}

    converted, skipped = images.convert_images(content, out, entries)
    assert (converted, skipped) == (2, 0)
    assert (out / "images" / "a.webp").exists()
    assert (out / "images" / "sub" / "b.webp").exists()

    converted, skipped = images.convert_images(content, out, entries)
    assert (converted, skipped) == (0, 2)
