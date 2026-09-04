import re
from pathlib import Path


REPOSITORY_ROOT = Path(__file__).resolve().parents[2]
RING = REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/HostEventRing.cs"
WRITER = REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/host-events.js"
ROUTER = REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/InputRouter.cs"


def test_resize_is_reported_rather_than_polled():
    loop = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/GameLoop.cs"
    ).read_text(encoding="utf-8")
    glcontext = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/glcontext.js"
    ).read_text(encoding="utf-8")

    assert "CanvasChangeCount" not in loop
    assert "canvasChangeCount" not in glcontext
    assert "ResizeObserver" in glcontext
    assert "HostEventKind.Resize" in loop
    assert "HostShim.ResizeCanvas(" in loop


def test_the_page_no_longer_reaches_managed_code_directly():
    main = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/main.js"
    ).read_text(encoding="utf-8")

    assert "getAssemblyExports" not in main
    assert "loop.Flush" not in main


def _csharp_const(name):
    source = RING.read_text(encoding="utf-8")
    match = re.search(rf"const int {name} = (\d+);", source)
    assert match, f"{name} not found in HostEventRing.cs"
    return int(match.group(1))


def _js_const(name):
    source = WRITER.read_text(encoding="utf-8")
    match = re.search(rf"const {name} = (\d+);", source)
    assert match, f"{name} not found in host-events.js"
    return int(match.group(1))


def test_ring_geometry_agrees_across_the_boundary():
    assert _js_const("CAPACITY") == _csharp_const("Capacity")
    assert _js_const("HEADER_BYTES") == _csharp_const("HeaderBytes")
    assert _js_const("RECORD_BYTES") == _csharp_const("RecordBytes")


def test_event_kinds_agree_across_the_boundary():
    ring = RING.read_text(encoding="utf-8")
    writer = WRITER.read_text(encoding="utf-8")
    for name, value in (
        ("POINTER", 1),
        ("KEY", 2),
        ("WHEEL", 3),
        ("ACTIVE", 4),
        ("RESIZE", 5),
    ):
        assert f"const KIND_{name} = {value};" in writer
        assert f"{name.capitalize()} = {value}," in ring


def test_key_ids_agree_across_the_boundary():
    writer = WRITER.read_text(encoding="utf-8")
    router = ROUTER.read_text(encoding="utf-8")
    for code, key_id, managed in (
        ("KeyQ", 1, "Escape"),
        ("KeyR", 2, "F5"),
        ("Space", 3, "Space"),
        ("Enter", 4, "Enter"),
        ("ArrowLeft", 5, "Left"),
        ("ArrowRight", 6, "Right"),
    ):
        assert f'"{code}": {key_id}' in writer
        assert f"HostKey.{managed} => KeyCode.{managed}" in router


def test_writer_refetches_its_view_after_memory_growth():
    writer = WRITER.read_text(encoding="utf-8")
    # A grown wasm memory replaces the buffer, so a cached view goes stale.
    assert "buffer !==" in writer or "buffer !=" in writer
    assert "Atomics.store" in writer
    assert "Atomics.load" in writer
