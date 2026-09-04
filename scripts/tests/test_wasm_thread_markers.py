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


def test_context_loss_pauses_rather_than_drawing_on():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Browser/GameLoop.cs"
    ).read_text(encoding="utf-8")

    assert "HostShim.ContextLost()" in source
    assert "ctrdx-context-lost" in source


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
        "ctrdx_supports_animation_frame",
        "ctrdx_set_frame_callback",
        "ctrdx_request_frame",
        "ctrdx_frame_entry",
        "ctrdx_frame_callback_hits",
    ):
        # Every entry point has to survive linking to be callable from managed code.
        assert re.search(rf"EMSCRIPTEN_KEEPALIVE\s+\S[^\n]*\b{export}\b", source)


def test_probe_reports_the_frame_driver_assumptions():
    source = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/Browser/WorkerRenderProbe.cs"
    ).read_text(encoding="utf-8")

    assert "HostShim.SupportsAnimationFrame()" in source
    assert "HostShim.SetFrameCallback" in source
    assert "HostShim.RequestFrame()" in source
    assert '"frame-driver"' in source


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


def test_managed_worker_proof_stays_out_of_the_boot_path():
    """The worker proof belongs to the probe, not to a normal launch."""
    boot = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Program.cs"
    ).read_text(encoding="utf-8")

    assert "ctrdx-thread-smoke:" not in boot
    assert "Task.Run" not in boot


def test_managed_worker_probe_runs_before_the_render_boundary():
    source = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/Browser/WorkerRenderProbe.cs"
    ).read_text(encoding="utf-8")

    marker_offset = source.find("ctrdx-thread-smoke:")
    first_context_offset = source.find(
        'GLContextInterop.TransferCanvasToThread("game", threadId)'
    )

    assert marker_offset >= 0
    assert first_context_offset >= 0
    assert marker_offset < first_context_offset
    assert "Environment.CurrentManagedThreadId" in source
    assert "Task.Run" in source
    assert "different=" in source
    assert "result={workerResult}" in source
    assert "42" in source


def test_render_probe_interop_is_typed_and_dom_free():
    path = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/Browser/RenderProbeInterop.cs"
    )
    source = path.read_text(encoding="utf-8")

    assert 'JSHost.ImportAsync("renderprobe", "../render-probe.js")' in source
    for export in (
        "isRequested",
        "executionContext",
        "isExpectedPixel",
    ):
        assert f'[JSImport("{export}", "renderprobe")]' in source

    assert "public static partial bool IsExpectedPixel(int[] values);" in source
    assert "JSObject" not in source


def test_render_probe_branches_before_normal_browser_bootstrap():
    source = (
        REPOSITORY_ROOT / "src/CutTheRopeDX.Browser/Program.cs"
    ).read_text(encoding="utf-8")

    gl_import = source.find("await GLContextInterop.ImportAsync();")
    probe_import = source.find("await RenderProbeInterop.ImportAsync();")
    probe_branch = source.find("if (RenderProbeInterop.IsRequested())")
    probe_run = source.find("await WorkerRenderProbe.RunAsync();")
    content_import = source.find("await FetchInterop.ImportAsync();")
    audio_import = source.find("await AudioInterop.ImportAsync();")
    storage_import = source.find("await StorageInterop.ImportAsync();")

    assert -1 not in (
        gl_import,
        probe_import,
        probe_branch,
        probe_run,
        content_import,
        audio_import,
        storage_import,
    )
    assert (
        gl_import
        < probe_import
        < probe_branch
        < probe_run
        < content_import
        < audio_import
        < storage_import
    )
    branch = source[probe_branch:content_import]
    assert "return;" in branch


def test_worker_render_probe_uses_production_skia_path_and_one_result_marker():
    source = (
        REPOSITORY_ROOT
        / "src/CutTheRopeDX.Browser/Browser/WorkerRenderProbe.cs"
    ).read_text(encoding="utf-8")

    for required in (
        "Environment.CurrentManagedThreadId",
        "RenderProbeInterop.ExecutionContext()",
        'GLContextInterop.TransferCanvasToThread("game", threadId)',
        "HostShim.CreateWorkerContext(",
        "HostShim.ContextUsable()",
        "using SkiaSurface",
        "new SKColor(17, 34, 51, 255)",
        ".Canvas.Clear(",
        ".Flush()",
        "HostShim.ReadCenterPixel(",
        "RenderProbeInterop.IsExpectedPixel(",
    ):
        assert required in source

    for result in (
        "CONTEXT_CREATE_FAILED",
        "CONTEXT_NOT_CURRENT",
        "SKIA_INTERFACE_FAILED",
        "SKIA_CONTEXT_FAILED",
        "SKIA_SURFACE_FAILED",
        "SKIA_FLUSH_FAILED",
        "PIXEL_READBACK_FAILED",
        "PIXEL_MISMATCH",
        "GATE2_PASS",
    ):
        assert result in source

    assert "ctrdx-render-probe: milestone=" in source
    assert source.count("ctrdx-render-probe: result=") == 1
