import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from webcontent import audio


def test_sfx_detected_by_directory():
    assert audio.is_sfx(Path("sounds/sfx/tap.wav"))
    assert not audio.is_sfx(Path("sounds/menu_music.wav"))


def test_music_command_is_stereo_192k_libvorbis():
    command = audio.ogg_command(
        Path("/ff"), Path("in.wav"), Path("out.ogg"), 192, mono=False
    )
    assert command[0] == "/ff"
    assert "-c:a" in command and command[command.index("-c:a") + 1] == "libvorbis"
    assert "-b:a" in command and command[command.index("-b:a") + 1] == "192k"
    assert "-ac" not in command
    assert "-q:a" not in command


def test_sfx_command_is_mono_96k():
    command = audio.ogg_command(
        Path("/ff"), Path("in.wav"), Path("out.ogg"), 96, mono=True
    )
    assert command[command.index("-ac") + 1] == "1"
    assert command[command.index("-b:a") + 1] == "96k"


def test_settings_differ_between_music_and_sfx():
    assert audio.settings_for(Path("sounds/menu_music.wav")) != audio.settings_for(
        Path("sounds/sfx/tap.wav")
    )
