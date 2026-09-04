import re
from pathlib import Path


REPOSITORY_ROOT = Path(__file__).resolve().parents[2]


def test_missing_isolation_is_terminal_before_the_runtime_starts():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/main.js"
    ).read_text(encoding="utf-8")

    isolation = source.find("crossOriginIsolated")
    runtime_import = source.find('await import("./_framework/dotnet.js")')
    creation = source.find("builder.create()")
    assert isolation >= 0
    assert runtime_import >= 0
    assert isolation < runtime_import
    assert isolation < creation
    # Threaded-only: there is no degraded mode to fall back to, and the canvas
    # transfer cannot be undone once it has happened.
    assert "ctrdx-isolation-error" in source


def test_pages_service_worker_bootstraps_isolation_before_main():
    coi = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/coi.js"
    ).read_text(encoding="utf-8")
    pwa = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/pwa.js"
    ).read_text(encoding="utf-8")
    main = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/main.js"
    ).read_text(encoding="utf-8")
    worker = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/wwwroot/service-worker.published.js"
    ).read_text(encoding="utf-8")
    project = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/CutTheRopeDX.Browser.csproj"
    ).read_text(encoding="utf-8")
    migration_worker = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/wwwroot/service-worker.js"
    ).read_text(encoding="utf-8")
    index = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/index.html"
    ).read_text(encoding="utf-8")

    assert "yell0wsuit/coi-sw" in coi
    assert "ctrdxIsolationReady" in coi
    assert 'register("./coi-sw.js"' in coi
    assert "controllerchange" in coi
    assert "location.replace(globalThis.location.href)" in coi
    assert "ctrdxServiceWorkerRegistration" in pwa
    assert 'ServiceWorker Include="wwwroot/coi-sw.js"' in project
    assert 'importScripts("./coi-sw.js")' in migration_worker
    assert index.find('src="./coi.js"') < index.find('src="./main.js"')
    assert "await globalThis.ctrdxIsolationReady" in main
    assert 'request.cache === "only-if-cached"' in worker
    assert "response.status === 0" in worker
    for header in (
        "Cross-Origin-Opener-Policy",
        "Cross-Origin-Embedder-Policy",
        "Cross-Origin-Resource-Policy",
    ):
        assert header in worker
    assert 'headers.set("Cross-Origin-Resource-Policy", "same-origin")' in worker


def test_context_loss_pauses_rather_than_drawing_on():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/GameLoop.cs"
    ).read_text(encoding="utf-8")

    assert "HostShim.ContextLost()" in source
    assert "ctrdx-context-lost" in source
    assert "reload required" in source
    assert source.find("try") < source.find("HostShim.ContextLost()")

    context_loss_start = source.find("if (HostShim.ContextLost()")
    context_loss = source[
        context_loss_start : source.find("try", context_loss_start + 1)
    ]
    assert "HostShim.RequestFrame()" not in context_loss

    browser = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/glcontext.js"
    ).read_text(encoding="utf-8")
    assert 'classList.remove("hidden")' in browser


def test_hidden_page_wakes_the_owner_to_process_lifecycle():
    events = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/host-events.js"
    ).read_text(encoding="utf-8")
    shim = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Native/ctrdxhost.c"
    ).read_text(encoding="utf-8")

    assert "ctrdxWake" in events
    assert "ctrdxWake" in shim


def test_host_messages_carry_no_command_field():
    """The runtime's worker dispatcher reports any `cmd` it does not know."""
    for relative in (
        "src/CutTheRopeDX.Browser/wwwroot/host-events.js",
        "src/CutTheRopeDX.Browser/wwwroot/glcontext.js",
        "src/CutTheRopeDX.Browser/Native/ctrdxhost.c",
    ):
        source = (REPOSITORY_ROOT / relative).read_text(encoding="utf-8")
        assert "cmd:" not in source
        assert "cmd ===" not in source


def test_event_ring_reserves_control_capacity_and_reports_drops():
    events = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/host-events.js"
    ).read_text(encoding="utf-8")
    loop = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/GameLoop.cs"
    ).read_text(encoding="utf-8")

    assert "CONTROL_RESERVE" in events
    assert "ctrdx-host-events-dropped" in loop


def test_no_jsexport_survives_on_the_browser_thread_boundary():
    """Synchronous JSExport from the browser thread throws under threading."""
    for relative in (
        "src/CutTheRopeDX.Browser/Browser/GameLoop.cs",
        "src/CutTheRopeDX.Browser/Browser/InputRouter.cs",
    ):
        source = (REPOSITORY_ROOT / relative).read_text(encoding="utf-8")
        assert "[JSExport]" not in source


def test_the_frame_comes_from_the_owner_threads_animation_frame():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/GameLoop.cs"
    ).read_text(encoding="utf-8")

    assert "UnmanagedCallersOnly" in source
    assert "HostShim.RequestFrame()" in source

    main = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/wwwroot/main.js"
    ).read_text(encoding="utf-8")
    assert "requestAnimationFrame" not in main


def test_native_shim_runs_in_the_calling_threads_scope():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Native/ctrdxhost.c"
    ).read_text(encoding="utf-8")

    # EM_ASM runs in the JS scope of the calling thread; JS interop does not.
    assert "EM_ASM" in source
    for export in (
        "ctrdx_thread_id",
        "ctrdx_is_main_runtime_thread",
        "ctrdx_set_frame_callback",
        "ctrdx_request_frame",
        "ctrdx_frame_entry",
    ):
        # Every entry point has to survive linking to be callable from managed code.
        assert re.search(rf"EMSCRIPTEN_KEEPALIVE\s+\S[^\n]*\b{export}\b", source)


def test_javascript_reports_isolation_before_runtime_creation():
    source = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/wwwroot/main.js"
    ).read_text(encoding="utf-8")

    marker_offset = source.find("ctrdx-wasm-env: crossOriginIsolated=")
    runtime_creation_offset = source.find("builder.create()")

    assert marker_offset >= 0
    assert runtime_creation_offset >= 0
    assert marker_offset < runtime_creation_offset
