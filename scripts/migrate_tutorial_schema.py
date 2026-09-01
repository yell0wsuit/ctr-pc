#!/usr/bin/env python3
"""Migrate tutorial metadata from the legacy `special` int to the authored XML schema.

The old model hung four behaviors off one scene-wide `<gameDesign special="N">` and a
matching `special` on the listening tutorial element. Every other map set the attribute
without anything listening, so almost all of it is dead data. This tool performs the
mechanical half of the move:

    - drops `special` from every `<gameDesign>`;
    - drops an explicit `special="0"` from a tutorial element, which was already the
      "play immediately" default;
    - rewrites the four trigger-bearing maps' tutorial elements to their named event;
    - gives 1_1's tutorials the ten-second hold the loader used to hardcode for it.

Anything else carrying a nonzero tutorial `special` is a migration the table below does
not describe, and is reported as an error rather than silently dropped.

    python3 scripts/migrate_tutorial_schema.py            # report what would change
    python3 scripts/migrate_tutorial_schema.py --write    # rewrite in place

The rewrite is textual so unrelated attributes, attribute order, and file formatting
survive untouched, and the result reviews as a small diff.
"""

import argparse
import re
import sys
from collections.abc import Iterator
from pathlib import Path

# (map stem, authored special) -> the attributes that replace it, in its place.
TRIGGER_REWRITES: dict[tuple[str, str], str] = {
    # The world-space `x > 1200 && y < 400` region, in map coordinates, extended to the
    # map's right and top edges.
    ("1_5", "1"): 'showOn="bubbled" inArea="133,0,186,133"',
    ("1_1", "2"): 'anim="swipe"',
    ("14_1", "3"): 'showOn="lanternCatch"',
    ("15_1", "4"): 'showOn="mouseGrab"',
}

# Maps whose hold the loader used to override by pack/level index instead of by author.
DURATION_OVERRIDES: dict[str, str] = {"1_1": "10"}

TUTORIAL_ELEMENT = re.compile(r"<tutorial(?:Text|\d+)\b[^>]*/>")
GAME_DESIGN_ELEMENT = re.compile(r"<gameDesign\b[^>]*/>")
SPECIAL_ATTRIBUTE = re.compile(r'\s*special="([^"]*)"')

MAP_GLOB = "content/maps/*.xml"
SAMPLE_GLOB = "samples/levels/*.xml"


class MigrationError(Exception):
    """A tutorial element carries a `special` this migration has no rule for."""


def _strip_special(element: str) -> tuple[str, str | None]:
    """Removes a `special` attribute from one element, reporting the value it held."""
    match = SPECIAL_ATTRIBUTE.search(element)
    if match is None:
        return element, None
    return element[: match.start()] + element[match.end() :], match.group(1)


def _with_attributes(element: str, attributes: str) -> str:
    """Appends attributes to a self-closing element, keeping its ` />` ending."""
    return f"{element[: element.rindex('/>')].rstrip()} {attributes} />"


def _migrate_game_design(element: str) -> str:
    return _strip_special(element)[0]


def _migrate_tutorial(element: str, stem: str) -> str:
    element, special = _strip_special(element)
    if special not in (None, "0"):
        replacement = TRIGGER_REWRITES.get((stem, special))
        if replacement is None:
            raise MigrationError(
                f'{stem}: no migration rule for tutorial special="{special}"'
            )
        element = _with_attributes(element, replacement)

    duration = DURATION_OVERRIDES.get(stem)
    if duration is not None and 'duration="' not in element:
        element = _with_attributes(element, f'duration="{duration}"')
    return element


def migrate_document(text: str, stem: str) -> str:
    """Migrates one map or sample level.

    Args:
        text: The file's full text.
        stem: The level's file name without its extension, which selects the rewrite.

    Returns:
        The migrated text, unchanged when the file is already on the new schema.

    Raises:
        MigrationError: A tutorial element carries a `special` with no rule.
    """
    text = GAME_DESIGN_ELEMENT.sub(lambda m: _migrate_game_design(m.group(0)), text)
    return TUTORIAL_ELEMENT.sub(lambda m: _migrate_tutorial(m.group(0), stem), text)


def iter_targets(root: Path) -> Iterator[Path]:
    """Yields every shipped map and sample level, in a stable order."""
    for pattern in (MAP_GLOB, SAMPLE_GLOB):
        yield from sorted(root.glob(pattern))


def migrate_tree(root: Path, write: bool) -> list[Path]:
    """Migrates every target under a repository root, returning the changed paths."""
    changed: list[Path] = []
    for path in iter_targets(root):
        before = path.read_text(encoding="utf-8")
        after = migrate_document(before, path.stem)
        if after == before:
            continue
        changed.append(path)
        if write:
            path.write_text(after, encoding="utf-8")
    return changed


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument(
        "--check",
        action="store_true",
        help="report the files that would change and exit nonzero (default)",
    )
    mode.add_argument("--write", action="store_true", help="rewrite the files in place")
    parser.add_argument(
        "--root",
        type=Path,
        default=Path(__file__).resolve().parents[1],
        help="repository root holding content/maps and samples/levels",
    )
    args = parser.parse_args(argv)

    try:
        changed = migrate_tree(args.root, write=args.write)
    except MigrationError as failure:
        print(f"error: {failure}", file=sys.stderr)
        return 2

    if not changed:
        print("tutorial schema: no changes")
        return 0

    verb = "migrated" if args.write else "would change"
    for path in changed:
        print(f"{verb}: {path.relative_to(args.root)}")
    print(f"{verb}: {len(changed)} file(s)")
    return 0 if args.write else 1


if __name__ == "__main__":
    raise SystemExit(main())
