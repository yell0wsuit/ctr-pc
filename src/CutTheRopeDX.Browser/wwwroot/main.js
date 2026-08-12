import { dotnet } from "./_framework/dotnet.js";
import { setLoadingProgress } from "./loading-progress.js";

const reportDownloadProgress = (loaded, total) => {
    setLoadingProgress("runtime", loaded, total);
};

const builder = dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery();

// Probed rather than called outright: losing the counter is cosmetic, but throwing
// here would cost the whole app its boot.
if (typeof builder.withModuleConfig === "function") {
    builder.withModuleConfig({
        onDownloadResourceProgress: reportDownloadProgress,
    });
}

const runtime = await builder.create();
const config = runtime.getConfig();
globalThis.ctrdxWasmModule = runtime.Module;
await runtime.runMain(config.mainAssemblyName, []);

const exports = await runtime.getAssemblyExports(config.mainAssemblyName);
const canvas = document.getElementById("game");
const input = exports.CutTheRopeDX.Browser.InputRouter;
const loop = exports.CutTheRopeDX.Browser.GameLoop;

const toBacking = (event) => {
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    return [
        (event.clientX - rect.left) * scaleX,
        (event.clientY - rect.top) * scaleY,
    ];
};

const sendPointer = (event, phase) => {
    event.preventDefault();
    const [x, y] = toBacking(event);
    input.OnPointer(x, y, phase);
};

canvas.addEventListener("pointerdown", (event) => {
    canvas.setPointerCapture(event.pointerId);
    sendPointer(event, 0);
});
canvas.addEventListener("pointermove", (event) => sendPointer(event, 1));
canvas.addEventListener("pointerup", (event) => sendPointer(event, 2));
canvas.addEventListener("pointercancel", (event) => sendPointer(event, 2));

// Core scrolls in the desktop's wheel units: one notch is 120 and positive scrolls up. A
// WheelEvent reports the opposite sign and, depending on deltaMode, counts lines or pages
// rather than pixels — so both are normalized here and Core sees what it does on desktop.
const PIXELS_PER_NOTCH = 100;
const UNITS_PER_NOTCH = 120;
// Firefox reports one notch as three lines where other browsers report ~100px, so a line is
// worth a third of a notch here rather than a text line's height. Sizing it any other way
// makes the same wheel scroll a different distance per browser.
const PIXELS_PER_LINE = PIXELS_PER_NOTCH / 3;

canvas.addEventListener(
    "wheel",
    (event) => {
        event.preventDefault();
        const scale =
            event.deltaMode === 1
                ? PIXELS_PER_LINE
                : event.deltaMode === 2
                  ? canvas.clientHeight
                  : 1;
        const units =
            (-event.deltaY * scale * UNITS_PER_NOTCH) / PIXELS_PER_NOTCH;
        const rounded = Math.round(units);
        if (rounded !== 0) {
            input.OnWheel(rounded);
        }
    },
    // preventDefault needs a non-passive listener, which wheel handlers default to.
    { passive: false },
);

const sendKey = (event, down) => {
    if (["Space", "ArrowLeft", "ArrowRight"].includes(event.code)) {
        event.preventDefault();
    }
    input.OnKey(event.code, down);
};
globalThis.addEventListener("keydown", (event) => sendKey(event, true));
globalThis.addEventListener("keyup", (event) => sendKey(event, false));

// Focus and visibility are separate losses and either one must freeze the game: a hidden
// tab stops getting animation frames but keeps its audio, while a window merely pushed
// behind another stays visible and keeps ticking at full speed.
const syncActive = () =>
    loop.SetActive(
        document.visibilityState === "visible" && document.hasFocus(),
    );
globalThis.addEventListener("focus", syncActive);
globalThis.addEventListener("blur", syncActive);
document.addEventListener("visibilitychange", syncActive);
syncActive();

// Pausing already flushes the save, but a page can be discarded without ever going
// inactive first.
globalThis.addEventListener("pagehide", () => loop.Flush());

let started = false;
const frame = (timestamp) => {
    loop.Tick(timestamp);
    requestAnimationFrame(frame);
};
globalThis.ctrdxStart = () => {
    if (!started) {
        started = true;
        requestAnimationFrame(frame);
    }
};
globalThis.ctrdxReady?.();
