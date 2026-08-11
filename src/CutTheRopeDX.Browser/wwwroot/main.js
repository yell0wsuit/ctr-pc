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

const sendKey = (event, down) => {
    if (["Space", "ArrowLeft", "ArrowRight"].includes(event.code)) {
        event.preventDefault();
    }
    input.OnKey(event.code, down);
};
globalThis.addEventListener("keydown", (event) => sendKey(event, true));
globalThis.addEventListener("keyup", (event) => sendKey(event, false));

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
