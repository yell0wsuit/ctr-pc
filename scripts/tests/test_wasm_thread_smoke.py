import stat
import textwrap
import urllib.request

import pytest

from scripts.wasm_thread_smoke import (
    build_browser_command,
    classify_output,
    main,
    run_smoke,
    serve_publish_directory,
)


def test_identifies_deputy_startup_failure():
    text = "mono_wasm_start_deputy_thread_async() failed RuntimeError: unreachable"

    assert classify_output(text) == "DEPUTY_STARTUP_FAILED"


def test_requires_cross_origin_isolation():
    text = "ctrdx-wasm-env: crossOriginIsolated=false"

    assert classify_output(text) == "NOT_CROSS_ORIGIN_ISOLATED"


@pytest.mark.parametrize(
    "result",
    [
        "CONTEXT_CREATE_FAILED",
        "CONTEXT_NOT_CURRENT",
        "SKIA_INTERFACE_FAILED",
        "SKIA_CONTEXT_FAILED",
        "SKIA_SURFACE_FAILED",
        "SKIA_FLUSH_FAILED",
        "PIXEL_READBACK_FAILED",
        "PIXEL_MISMATCH",
        "GATE2_PASS",
    ],
)
def test_classifies_gate2_terminal_results(result):
    text = f"ctrdx-render-probe: result={result}"

    assert classify_output(text) == result


def test_runtime_failure_overrides_gate2_success():
    text = """
    ctrdx-render-probe: result=GATE2_PASS
    [MONO] Assertion failed after probe result
    """

    assert classify_output(text) == "RUNTIME_FAILED"


def test_mono_wasm_javascript_failure_is_a_runtime_failure():
    text = "MONO_WASM: Cannot read properties of undefined (reading 'getParameter')"

    assert classify_output(text) == "RUNTIME_FAILED"


def test_server_adds_cross_origin_isolation_headers(tmp_path):
    (tmp_path / "index.html").write_text("ok", encoding="utf-8")

    with serve_publish_directory(tmp_path) as url:
        with urllib.request.urlopen(url, timeout=2) as response:
            assert response.headers["Cross-Origin-Opener-Policy"] == "same-origin"
            assert (
                response.headers["Cross-Origin-Embedder-Policy"]
                == "require-corp"
            )
            assert (
                response.headers["Cross-Origin-Resource-Policy"]
                == "cross-origin"
            )


def test_browser_command_uses_headless_temporary_profile_and_url(tmp_path):
    command = build_browser_command(
        "/browser",
        tmp_path,
        "http://127.0.0.1:1234/",
    )

    assert command[0] == "/browser"
    assert "--headless=new" in command
    assert f"--user-data-dir={tmp_path}" in command
    assert command[-1] == "http://127.0.0.1:1234/"


def test_missing_browser_is_reported_as_nonzero(tmp_path, capsys):
    (tmp_path / "index.html").write_text("ok", encoding="utf-8")

    result = main(
        [
            "--publish-dir",
            str(tmp_path),
            "--browser",
            str(tmp_path / "missing-browser"),
            "--timeout",
            "1",
        ]
    )

    assert result != 0
    assert "browser executable does not exist" in capsys.readouterr().err


def test_timeout_stops_browser_process(tmp_path):
    (tmp_path / "index.html").write_text("ok", encoding="utf-8")
    browser = tmp_path / "fake-browser"
    browser.write_text(
        textwrap.dedent(
            """\
            #!/bin/sh
            trap 'exit 0' TERM INT
            while true; do sleep 1; done
            """
        ),
        encoding="utf-8",
    )
    browser.chmod(browser.stat().st_mode | stat.S_IXUSR)

    result = run_smoke(tmp_path, browser, timeout=0.05)

    assert result.classification == "INCOMPLETE"
    assert result.timed_out is True
    assert result.process_returncode is not None


def _write_fake_browser(path, body):
    path.write_text(f"#!/bin/sh\n{body}\n", encoding="utf-8")
    path.chmod(path.stat().st_mode | stat.S_IXUSR)


def test_gate2_waits_through_success_grace_period(tmp_path):
    (tmp_path / "index.html").write_text("ok", encoding="utf-8")
    browser = tmp_path / "fake-browser"
    _write_fake_browser(
        browser,
        "echo 'ctrdx-render-probe: result=GATE2_PASS'; sleep 1",
    )

    result = run_smoke(
        tmp_path,
        browser,
        timeout=2,
        gate="gate2",
        success_grace=0.05,
    )

    assert result.classification == "GATE2_PASS"
    assert result.timed_out is False
    assert result.process_returncode is not None


def test_runtime_failure_during_gate2_grace_overrides_success(tmp_path):
    (tmp_path / "index.html").write_text("ok", encoding="utf-8")
    browser = tmp_path / "fake-browser"
    _write_fake_browser(
        browser,
        "echo 'ctrdx-render-probe: result=GATE2_PASS'; "
        "sleep 0.02; echo '[MONO] Assertion failed'; sleep 1",
    )

    result = run_smoke(
        tmp_path,
        browser,
        timeout=2,
        gate="gate2",
        success_grace=0.1,
    )

    assert result.classification == "RUNTIME_FAILED"
