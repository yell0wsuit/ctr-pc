import json
import shutil
import subprocess
from pathlib import Path

import pytest


REPOSITORY_ROOT = Path(__file__).resolve().parents[2]


def test_canvas_transfer_hands_ownership_to_the_owner_thread():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/glcontext.js"
    ).read_text(encoding="utf-8")

    assert "transferControlToOffscreen()" in source
    assert "PThread" in source
    assert "postMessage" in source
    # The transfer path never sizes the backing store on the browser thread.
    transfer = source[
        source.index("export function transferCanvasToThread") :
        source.index("// Retained until normal boot")
    ]
    assert "canvas.width = " not in transfer


def test_probe_reads_pixels_through_the_owner_threads_context():
    source = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/Browser/WorkerRenderProbe.cs"
    ).read_text(encoding="utf-8")

    assert "HostShim.CreateWorkerContext(" in source
    assert "HostShim.ReadCenterPixel(" in source
    # Readback must not go through the browser thread, which no longer owns the
    # canvas or the context.
    assert "RenderProbeInterop.ReadCenterPixel(" not in source


def test_render_probe_keeps_only_browser_thread_facts_and_pixel_comparison():
    node = shutil.which("node")
    if node is None:
        pytest.skip("Node.js is not installed")

    module_path = (
        Path(__file__).resolve().parents[2]
        / "src"
        / "CutTheRopeDX.Browser"
        / "wwwroot"
        / "render-probe.js"
    )
    script = f"""
import assert from "node:assert/strict";
import fs from "node:fs/promises";

const source = await fs.readFile({json.dumps(str(module_path))}, "utf8");
const probe = await import(
    `data:text/javascript;base64,${{Buffer.from(source).toString("base64")}}`
);

delete globalThis.ctrdxRenderProbe;
assert.equal(probe.isRequested(), false);
globalThis.ctrdxRenderProbe = 1;
assert.equal(probe.isRequested(), false);
globalThis.ctrdxRenderProbe = true;
assert.equal(probe.isRequested(), true);

delete globalThis.Window;
assert.equal(probe.executionContext(), "worker");
globalThis.Window = Object;
assert.equal(probe.executionContext(), "window");

assert.equal(probe.isExpectedPixel([17, 34, 51, 255, 0]), true);
assert.equal(probe.isExpectedPixel(new Int32Array([17, 34, 51, 255, 0])), true);
assert.equal(probe.isExpectedPixel([18, 34, 51, 255, 0]), false);
assert.equal(probe.isExpectedPixel([17, 34, 51, 255]), false);
assert.equal(probe.isExpectedPixel([17, 34, 51, 255, 1280]), false);
"""
    result = subprocess.run(
        [node, "--input-type=module"],
        input=script,
        capture_output=True,
        text=True,
        check=False,
    )

    assert result.returncode == 0, result.stderr


def test_probe_mode_is_selected_before_runtime_creation():
    main_path = (
        Path(__file__).resolve().parents[2]
        / "src"
        / "CutTheRopeDX.Browser"
        / "wwwroot"
        / "main.js"
    )
    source = main_path.read_text(encoding="utf-8")

    selection_offset = source.find("globalThis.ctrdxRenderProbe")
    runtime_creation_offset = source.find("builder.create()")

    assert selection_offset >= 0
    assert runtime_creation_offset >= 0
    assert selection_offset < runtime_creation_offset
    assert 'get("renderProbe") === "1"' in source
