import json
import subprocess
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import build_web_content


def _seed(content: Path) -> None:
    (content / "images").mkdir(parents=True)
    (content / "maps").mkdir(parents=True)
    (content / "locales").mkdir(parents=True)
    Image.new("RGBA", (8, 8), (1, 2, 3, 255)).save(content / "images" / "a.png")
    (content / "images" / "a.json").write_text('{\n  "frames": []\n}')
    (content / "maps" / "1_1.xml").write_text("<map />")
    (content / "locales" / "en.json").write_text('{\n  "hi": "there"\n}')
    (content / "packlist.json").write_text("{}")
    (content / "ctroriginal_packs.json").write_text("{}")


def test_run_without_audio_produces_images_fonts_and_bundle(tmp_path):
    content = tmp_path / "content"
    _seed(content)
    out = tmp_path / "out"

    code = build_web_content.main(
        ["--source", str(content), "--out", str(out), "--skip-audio"]
    )

    assert code == 0
    assert (out / "images" / "a.webp").exists()
    assert (out / "tier0.json").exists()
    bundle = json.loads((out / "tier0.json").read_text(encoding="utf-8"))
    assert bundle["locales/en.json"] == '{"hi":"there"}'
    assert (out / ".build-manifest.json").exists()
    assert json.loads((out / "assets.json").read_text(encoding="utf-8")) == {
        "images": ["images/a.webp"]
    }


def test_second_run_skips_everything(tmp_path, capsys):
    content = tmp_path / "content"
    _seed(content)
    out = tmp_path / "out"
    args = ["--source", str(content), "--out", str(out), "--skip-audio"]

    build_web_content.main(args)
    capsys.readouterr()
    build_web_content.main(args)

    assert "images: 0 converted, 1 skipped" in capsys.readouterr().out


def test_module_runs_as_a_script(tmp_path):
    content = tmp_path / "content"
    _seed(content)
    script = Path(build_web_content.__file__)
    result = subprocess.run(
        [
            sys.executable,
            str(script),
            "--source",
            str(content),
            "--out",
            str(tmp_path / "out"),
            "--skip-audio",
        ],
        capture_output=True,
        text=True,
        check=False,
    )
    assert result.returncode == 0, result.stderr


def test_skip_video_reports_and_succeeds(tmp_path, capsys):
    content = tmp_path / "content"
    _seed(content)

    code = build_web_content.main(
        [
            "--source",
            str(content),
            "--out",
            str(tmp_path / "out"),
            "--skip-audio",
            "--skip-video",
        ]
    )

    assert code == 0
    assert "video: skipped" in capsys.readouterr().out


def test_missing_video_encoder_warns_but_still_succeeds(tmp_path, monkeypatch, capsys):
    """A web build without cutscenes is the status quo, so this must not fail the build."""
    content = tmp_path / "content"
    _seed(content)
    monkeypatch.setattr(
        build_web_content.ffmpeg_tool,
        "find_system_ffmpeg",
        lambda: Path("/ff"),
    )

    def missing(_ffmpeg, _required):
        raise build_web_content.ffmpeg_tool.MissingEncoderError("no libvpx-vp9")

    monkeypatch.setattr(build_web_content.ffmpeg_tool, "require_encoders", missing)

    code = build_web_content.main(
        ["--source", str(content), "--out", str(tmp_path / "out"), "--skip-audio"]
    )

    assert code == 0
    assert "no libvpx-vp9" in capsys.readouterr().err


def test_require_video_fails_when_no_usable_ffmpeg(tmp_path, monkeypatch, capsys):
    """An automated build must not publish a payload that quietly lost its cutscenes."""
    content = tmp_path / "content"
    _seed(content)

    def missing() -> Path:
        raise build_web_content.ffmpeg_tool.FfmpegNotFoundError("no ffmpeg on PATH")

    monkeypatch.setattr(build_web_content.ffmpeg_tool, "find_system_ffmpeg", missing)

    code = build_web_content.main(
        [
            "--source",
            str(content),
            "--out",
            str(tmp_path / "out"),
            "--skip-audio",
            "--require-video",
        ]
    )

    assert code == 3
    assert "no ffmpeg on PATH" in capsys.readouterr().err


def test_progress_reports_each_file_and_no_progress_suppresses_it(tmp_path, capsys):
    content = tmp_path / "content"
    _seed(content)

    build_web_content.main(
        ["--source", str(content), "--out", str(tmp_path / "out"), "--skip-audio"]
    )
    with_progress = capsys.readouterr().err

    build_web_content.main(
        [
            "--source",
            str(content),
            "--out",
            str(tmp_path / "out2"),
            "--skip-audio",
            "--no-progress",
        ]
    )
    without_progress = capsys.readouterr().err

    assert "images 1/1 100%" in with_progress
    assert "images/a.webp" in with_progress
    assert "images/a.webp" not in without_progress
