import json
import shutil
import subprocess
from pathlib import Path

import pytest


def test_video_overlay_uses_the_canvas_viewport_box():
    node = shutil.which("node")
    if node is None:
        pytest.skip("Node.js is not installed")

    module_path = (
        Path(__file__).resolve().parents[2]
        / "src"
        / "CutTheRopeDX.Browser"
        / "wwwroot"
        / "glcontext.js"
    )
    script = f"""
import assert from "node:assert/strict";
import fs from "node:fs/promises";

const source = await fs.readFile({json.dumps(str(module_path))}, "utf8");
const glcontext = await import(
    `data:text/javascript;base64,${{Buffer.from(source).toString("base64")}}`
);
const canvas = {{ style: {{}} }};
const movie = {{ style: {{}} }};
globalThis.window = {{ innerWidth: 1200, innerHeight: 1000 }};
globalThis.document = {{
    getElementById(id) {{
        return id === "game" ? canvas : movie;
    }},
}};
globalThis.ctrdxWasmModule = {{
    GL: {{
        createContext() {{ return 1; }},
        makeContextCurrent() {{}},
    }},
}};

glcontext.createContext("game");

const expected = {{
    width: "1200px",
    height: "675px",
    left: "0px",
    top: "163px",
}};
assert.deepEqual(canvas.style, expected);
assert.deepEqual(movie.style, expected);

window.innerWidth = 1000;
window.innerHeight = 1200;
canvas.clientWidth = 1000;
canvas.clientHeight = 563;
glcontext.canvasSize("game");

const resized = {{
    width: "1000px",
    height: "563px",
    left: "0px",
    top: "319px",
}};
assert.deepEqual(canvas.style, resized);
assert.deepEqual(movie.style, resized);
"""
    result = subprocess.run(
        [node, "--input-type=module"],
        input=script,
        capture_output=True,
        text=True,
        check=False,
    )

    assert result.returncode == 0, result.stderr
