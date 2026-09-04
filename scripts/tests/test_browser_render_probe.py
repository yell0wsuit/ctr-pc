import json
import shutil
import subprocess
from pathlib import Path

import pytest


def test_render_probe_uses_current_emscripten_context():
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

const calls = [];
const gl = {{
    RGBA: 6408,
    UNSIGNED_BYTE: 5121,
    NO_ERROR: 0,
    finish() {{ calls.push(["finish"]); }},
    readPixels(x, y, width, height, format, type, pixel) {{
        calls.push(["readPixels", x, y, width, height, format, type]);
        pixel.set([17, 34, 51, 255]);
    }},
    getError() {{ calls.push(["getError"]); return 0; }},
}};
globalThis.document = {{
    getElementById(id) {{
        assert.equal(id, "game");
        return {{ width: 10, height: 8 }};
    }},
}};
globalThis.ctrdxWasmModule = {{
    GL: {{
        currentContext: {{ GLctx: gl }},
        createContext() {{ throw new Error("must not create a second context"); }},
    }},
}};

assert.deepEqual(probe.currentContextStatus(), [1, 1]);
assert.deepEqual(probe.readCenterPixel("game"), [17, 34, 51, 255, 0]);
assert.deepEqual(calls, [
    ["finish"],
    ["readPixels", 5, 4, 1, 1, 6408, 5121],
    ["getError"],
]);
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
