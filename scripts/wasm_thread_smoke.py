#!/usr/bin/env python3
"""Run and classify the browser WebAssembly threading smoke test."""

from __future__ import annotations

import argparse
from contextlib import contextmanager
from dataclasses import dataclass
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import tempfile
import threading
import time
from typing import Iterator, Sequence


RESULT_PREFIX = "RESULT: "
GATE2_RESULTS = {
    "CONTEXT_CREATE_FAILED",
    "CONTEXT_NOT_CURRENT",
    "SKIA_INTERFACE_FAILED",
    "SKIA_CONTEXT_FAILED",
    "SKIA_SURFACE_FAILED",
    "SKIA_FLUSH_FAILED",
    "PIXEL_READBACK_FAILED",
    "PIXEL_MISMATCH",
    "GATE2_PASS",
}


def classify_output(text: str, gate: str = "gate2") -> str:
    """Classify the stable markers and runtime failures in browser output."""
    if "ctrdx-wasm-env: crossOriginIsolated=false" in text:
        return "NOT_CROSS_ORIGIN_ISOLATED"
    if (
        "mono_wasm_start_deputy_thread_async() failed" in text
        or "mono-threads-wasm.c" in text
    ):
        return "DEPUTY_STARTUP_FAILED"
    if any(
        marker in text
        for marker in (
            "MONO_WASM:",
            "[MONO] Assertion",
            "StackOverflowException",
            "FATAL",
        )
    ):
        return "RUNTIME_FAILED"
    matches = re.findall(r"ctrdx-render-probe: result=([A-Z0-9_]+)", text)
    for result in reversed(matches):
        if result in GATE2_RESULTS:
            return result
    return "INCOMPLETE"


class CrossOriginIsolatedHandler(SimpleHTTPRequestHandler):
    """Serve static files with the headers required for shared WASM memory."""

    def end_headers(self) -> None:
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        self.send_header("Cross-Origin-Resource-Policy", "cross-origin")
        super().end_headers()

    def log_message(self, format: str, *args: object) -> None:
        pass


@contextmanager
def serve_publish_directory(directory: Path) -> Iterator[str]:
    handler = partial(CrossOriginIsolatedHandler, directory=str(directory))
    server = ThreadingHTTPServer(("127.0.0.1", 0), handler)
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    try:
        port = server.server_address[1]
        yield f"http://127.0.0.1:{port}/"
    finally:
        server.shutdown()
        server.server_close()
        thread.join(timeout=2)


def build_browser_command(
    browser: str | Path,
    profile_directory: str | Path,
    url: str,
) -> list[str]:
    return [
        str(browser),
        "--headless=new",
        "--no-first-run",
        "--no-default-browser-check",
        f"--user-data-dir={profile_directory}",
        "--enable-logging=stderr",
        "--v=0",
        url,
    ]


@dataclass(frozen=True)
class SmokeResult:
    classification: str
    output: str
    timed_out: bool
    process_returncode: int | None


def _stop_process(process: subprocess.Popen[str]) -> None:
    if process.poll() is not None:
        return
    try:
        os.killpg(process.pid, signal.SIGTERM)
        process.wait(timeout=3)
    except subprocess.TimeoutExpired:
        os.killpg(process.pid, signal.SIGKILL)
        process.wait(timeout=3)


def run_smoke(
    publish_directory: str | Path,
    browser: str | Path,
    timeout: float,
    gate: str = "gate2",
    success_grace: float = 0.0,
) -> SmokeResult:
    output_lines: list[str] = []
    timed_out = False

    with tempfile.TemporaryDirectory(prefix="ctrdx-wasm-smoke-") as profile:
        with serve_publish_directory(Path(publish_directory)) as url:
            if gate == "gate2":
                url = f"{url}?renderProbe=1"
            process = subprocess.Popen(
                build_browser_command(browser, profile, url),
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True,
                start_new_session=True,
            )

            def read_output() -> None:
                assert process.stdout is not None
                output_lines.extend(process.stdout)

            reader = threading.Thread(target=read_output, daemon=True)
            reader.start()
            deadline = time.monotonic() + timeout
            classification = "INCOMPLETE"
            terminal_at: float | None = None
            while process.poll() is None:
                classification = classify_output("".join(output_lines), gate=gate)
                if classification != "INCOMPLETE":
                    if classification == "RUNTIME_FAILED" or success_grace <= 0:
                        break
                    if terminal_at is None:
                        terminal_at = time.monotonic()
                    elif time.monotonic() - terminal_at >= success_grace:
                        break
                if time.monotonic() >= deadline:
                    timed_out = True
                    break
                time.sleep(0.01)

            _stop_process(process)
            reader.join(timeout=2)
            output = "".join(output_lines)
            classification = classify_output(output, gate=gate)

    return SmokeResult(classification, output, timed_out, process.returncode)


def _relevant_lines(output: str) -> Iterator[str]:
    needles = (
        "ctrdx-",
        "MONO_WASM",
        "Assertion",
        "assertion",
        "FATAL",
        "StackOverflowException",
        "mono-threads-wasm.c",
    )
    for line in output.splitlines():
        if any(needle in line for needle in needles):
            yield line


def main(arguments: Sequence[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--publish-dir", required=True, type=Path)
    parser.add_argument("--browser", required=True, type=Path)
    parser.add_argument("--timeout", type=float, default=120)
    # Only the render boundary is still worth classifying automatically; the
    # managed-threading proof it used to carry is implied by the game booting.
    parser.add_argument("--gate", choices=("gate2",), default="gate2")
    parser.add_argument("--success-grace", type=float, default=0.0)
    args = parser.parse_args(arguments)

    if not args.publish_dir.is_dir():
        print(f"publish directory does not exist: {args.publish_dir}", file=sys.stderr)
        return 2
    if not args.browser.is_file():
        print(f"browser executable does not exist: {args.browser}", file=sys.stderr)
        return 2

    result = run_smoke(
        args.publish_dir,
        args.browser,
        args.timeout,
        gate=args.gate,
        success_grace=args.success_grace,
    )
    for line in _relevant_lines(result.output):
        print(line)
    print(f"{RESULT_PREFIX}{result.classification}")
    expected_pass = "GATE2_PASS"
    return 0 if result.classification == expected_pass else 1


if __name__ == "__main__":
    raise SystemExit(main())
