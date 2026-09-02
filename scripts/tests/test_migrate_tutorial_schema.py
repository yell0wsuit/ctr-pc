import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from migrate_tutorial_schema import (  # noqa: E402
    MigrationError,
    main,
    migrate_document,
)


def wrap(game_design: str, *objects: str) -> str:
    body = "\n".join(f"        {node}" for node in objects)
    return (
        "<map>\n"
        '    <layer name="settings">\n'
        '        <map gridSize="32" width="320" height="480" />\n'
        f"        {game_design}\n"
        "    </layer>\n"
        '    <layer name="Objects">\n'
        f"{body}\n"
        "    </layer>\n"
        "</map>\n"
    )


def test_dead_game_design_metadata_is_stripped():
    source = wrap(
        '<gameDesign ropePhysicsSpeed="1" special="1" twoParts="false" />',
        '<tutorialText x="10" y="20" locale="en" text="T" width="120" />',
    )

    migrated = migrate_document(source, "3_7")

    assert '<gameDesign ropePhysicsSpeed="1" twoParts="false" />' in migrated
    assert "special" not in migrated


def test_1_5_trigger_becomes_a_bubbled_region():
    source = wrap(
        '<gameDesign ropePhysicsSpeed="1" special="1" twoParts="false" />',
        '<tutorialText x="43" y="45" locale="en" text="T" width="120" special="1" />',
        '<tutorial05 x="218" y="78" locale="en" moveSpeed="100" rotateSpeed="10" special="1" />',
    )

    migrated = migrate_document(source, "1_5")

    assert (
        '<tutorialText x="43" y="45" locale="en" text="T" width="120"'
        ' showOn="bubbled" inArea="133,0,186,133" />' in migrated
    )
    assert (
        '<tutorial05 x="218" y="78" locale="en" moveSpeed="100" rotateSpeed="10"'
        ' showOn="bubbled" inArea="133,0,186,133" />' in migrated
    )


def test_1_1_swipe_becomes_a_preset_and_every_element_holds_for_ten_seconds():
    source = wrap(
        '<gameDesign ropePhysicsSpeed="1.0" special="1" twoParts="false" />',
        '<tutorialText x="174" y="46" locale="en" text="T" width="160" />',
        '<tutorial01 x="57" y="119" locale="en" moveSpeed="100" rotateSpeed="100" />',
        '<tutorial10 x="93" y="149" locale="en" moveSpeed="100" rotateSpeed="100" special="2" />',
    )

    migrated = migrate_document(source, "1_1")

    assert (
        '<tutorialText x="174" y="46" locale="en" text="T" width="160" duration="10" />'
        in migrated
    )
    assert (
        '<tutorial01 x="57" y="119" locale="en" moveSpeed="100" rotateSpeed="100"'
        ' duration="10" />' in migrated
    )
    assert (
        '<tutorial10 x="93" y="149" locale="en" moveSpeed="100" rotateSpeed="100"'
        ' anim="swipe" duration="10" />' in migrated
    )


def test_14_1_trigger_becomes_lantern_catch():
    source = wrap(
        '<gameDesign special="3" twoParts="false" ropePhysicsSpeed="1" />',
        '<tutorialText x="60" y="50" locale="en" text="T" width="200" height="60" special="3" />',
    )

    migrated = migrate_document(source, "14_1")

    assert (
        '<tutorialText x="60" y="50" locale="en" text="T" width="200" height="60"'
        ' showOn="lanternCatch" />' in migrated
    )
    assert '<gameDesign twoParts="false" ropePhysicsSpeed="1" />' in migrated


def test_15_1_trigger_becomes_mouse_grab():
    source = wrap(
        '<gameDesign special="4" twoParts="false" ropePhysicsSpeed="1" />',
        '<tutorialText x="65" y="271" locale="en" text="T" width="130" special="4" />',
        '<tutorial09 x="47" y="318" locale="en" moveSpeed="100" rotateSpeed="100" special="4" />',
    )

    migrated = migrate_document(source, "15_1")

    assert (
        '<tutorialText x="65" y="271" locale="en" text="T" width="130" showOn="mouseGrab" />'
        in migrated
    )
    assert (
        '<tutorial09 x="47" y="318" locale="en" moveSpeed="100" rotateSpeed="100"'
        ' showOn="mouseGrab" />' in migrated
    )


def test_17_1_loses_only_its_dead_metadata():
    tutorial = '<tutorial10 moveSpeed="100" path="-95,49," rotateSpeed="0" x="192" y="369" locale="en" />'
    source = wrap(
        '<gameDesign nightLevel="false" ropePhysicsSpeed="1" special="5" twoParts="false" />',
        tutorial,
    )

    migrated = migrate_document(source, "17_1")

    assert (
        '<gameDesign nightLevel="false" ropePhysicsSpeed="1" twoParts="false" />' in migrated
    )
    assert tutorial in migrated


def test_explicit_zero_specials_are_dropped_from_samples():
    source = wrap(
        '<gameDesign special="1" twoParts="false" useTimeTravelRocketPhysics="true" />',
        '<tutorialText x="199" y="315" locale="en" text="Tap it" width="120" special="0" />',
    )

    migrated = migrate_document(source, "time_freeze_tutorial")

    assert '<gameDesign twoParts="false" useTimeTravelRocketPhysics="true" />' in migrated
    assert '<tutorialText x="199" y="315" locale="en" text="Tap it" width="120" />' in migrated


@pytest.mark.parametrize("stem", ["1_1", "1_5", "14_1", "15_1", "17_1", "3_7"])
def test_migration_is_idempotent(stem):
    source = wrap(
        '<gameDesign ropePhysicsSpeed="1" special="1" twoParts="false" />',
        '<tutorialText x="43" y="45" locale="en" text="T" width="120" special="1" />'
        if stem == "1_5"
        else '<tutorialText x="43" y="45" locale="en" text="T" width="120" />',
    )

    once = migrate_document(source, stem)

    assert migrate_document(once, stem) == once


def test_unrelated_formatting_and_attributes_survive():
    source = (
        "<map>\r\n"
        '  <layer   name="settings">\r\n'
        '    <gameDesign special="1"  twoParts="false" />\r\n'
        "  </layer>\r\n"
        '  <layer name="Objects">\r\n'
        '    <candy x="10" y="20" />\r\n'
        '    <grab x="1" y="2" radius="-1" spider="false" />\r\n'
        "  </layer>\r\n"
        "</map>\r\n"
    )

    migrated = migrate_document(source, "3_7")

    assert "\r\n" in migrated
    assert '<layer   name="settings">' in migrated
    assert '<candy x="10" y="20" />' in migrated
    assert '<grab x="1" y="2" radius="-1" spider="false" />' in migrated
    assert '<gameDesign  twoParts="false" />' in migrated


def test_unexpected_nonzero_tutorial_special_is_rejected():
    source = wrap(
        '<gameDesign special="1" twoParts="false" />',
        '<tutorialText x="1" y="2" locale="en" text="T" width="10" special="7" />',
    )

    with pytest.raises(MigrationError) as failure:
        migrate_document(source, "9_9")

    assert "9_9" in str(failure.value)
    assert 'special="7"' in str(failure.value)


def seed(root: Path) -> Path:
    maps = root / "content" / "maps"
    maps.mkdir(parents=True)
    target = maps / "14_1.xml"
    target.write_text(
        wrap(
            '<gameDesign special="3" twoParts="false" ropePhysicsSpeed="1" />',
            '<tutorialText x="60" y="50" locale="en" text="T" width="200" special="3" />',
        ),
        encoding="utf-8",
    )
    (root / "samples" / "levels").mkdir(parents=True)
    return target


def test_check_mode_reports_the_changed_paths_without_writing(tmp_path, capsys):
    target = seed(tmp_path)
    before = target.read_text(encoding="utf-8")

    code = main(["--check", "--root", str(tmp_path)])

    assert code == 1
    assert "14_1.xml" in capsys.readouterr().out
    assert target.read_text(encoding="utf-8") == before


def test_write_mode_rewrites_and_then_checks_clean(tmp_path):
    target = seed(tmp_path)

    assert main(["--write", "--root", str(tmp_path)]) == 0

    migrated = target.read_text(encoding="utf-8")
    assert "special" not in migrated
    assert 'showOn="lanternCatch"' in migrated
    assert main(["--check", "--root", str(tmp_path)]) == 0
