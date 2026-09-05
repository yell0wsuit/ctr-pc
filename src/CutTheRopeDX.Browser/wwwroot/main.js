import * as hostEvents from "./host-events.js";
import { setLoadingProgress } from "./loading-progress.js";

// The failure seam is installed by the inline module in index.html rather than here, because
// this module's static imports are fetched and evaluated before its first statement runs: an
// import that fails would take this file down before anything it installed could exist. This
// is only the local alias; index.html owns the handlers and the report-once guard.
const fail = (id, detail) => globalThis.ctrdxFail?.(id, detail);

/**
 * Reports why this browser cannot run the game, or null when it can.
 *
 * The build renders from a worker through a transferred OffscreenCanvas and has no
 * browser-thread path to degrade to. Safari grew OffscreenCanvas at 16.4 but only added a
 * WebGL2 context on one at 17, so a 16.x iPhone gets all the way through the canvas
 * transfer and only then meets a null context - several seconds into a 56MB download, from
 * managed code, where nothing surfaces it. Probing first turns that into a sentence the
 * player can act on.
 *
 * Capability probes only, no user agent matching: what a runtime reports about itself is a
 * poorer answer to "can this run" than asking the runtime to do the thing.
 *
 * Support data: https://github.com/mdn/browser-compat-data
 */
function unsupportedReason() {
    if (typeof SharedArrayBuffer !== "function") {
        return "This browser does not support the shared memory the game needs.";
    }
    if (
        typeof OffscreenCanvas !== "function" ||
        typeof HTMLCanvasElement.prototype.transferControlToOffscreen !==
            "function"
    ) {
        return (
            "This browser cannot hand a canvas to a background thread. " +
            "iOS and iPadOS need version 17 or newer."
        );
    }

    try {
        const probe = new OffscreenCanvas(1, 1);
        const context = probe.getContext("webgl2");
        if (context === null) {
            return (
                "This browser cannot draw 3D graphics from a background thread. " +
                "iOS and iPadOS need version 17 or newer."
            );
        }
        // The probe holds a real GL context and a browser caps how many may exist at once,
        // so it is handed back rather than left for the collector to get to eventually.
        context.getExtension("WEBGL_lose_context")?.loseContext();
    } catch (error) {
        return `This browser cannot start the graphics the game needs: ${error}`;
    }

    return null;
}

await globalThis.ctrdxIsolationReady;
const isolated = globalThis.crossOriginIsolated === true;
console.info(`ctrdx-wasm-env: crossOriginIsolated=${isolated}`);
if (!isolated) {
    // Threaded-only: there is no browser-thread rendering path to degrade to, and
    // the canvas transfer this build depends on cannot be undone once it happens.
    console.error(
        "ctrdx-isolation-error: refusing to start without shared memory",
    );
    fail("isolation-error");
    throw new Error("Cross-origin isolation is required.");
}

const unsupported = unsupportedReason();
if (unsupported !== null) {
    // The probe knows more about which capability was missing than the markup's fallback
    // wording does, so it replaces it.
    const element = document.getElementById("unsupported-error");
    if (element !== null) {
        element.textContent = unsupported;
    }
    fail("unsupported-error", unsupported);
    throw new Error(unsupported);
}

try {
    // Importing the threaded runtime itself requires SharedArrayBuffer, so the
    // isolation guard must run before this module is evaluated.
    const { dotnet } = await import("./_framework/dotnet.js");
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
} catch (error) {
    // Content preload failures land here too, not just runtime ones: a phone that loses its
    // connection partway through the asset catalog throws from managed code, and without
    // this the player watches the spinner for as long as they are willing to.
    fail("boot-error", error);
    throw error;
}

const canvas = document.getElementById("game");

// getBoundingClientRect forces the browser to settle layout before it answers, and a drag
// asks once per pointermove. The rectangle only moves when the canvas box does, so it is
// measured then and reused for every event in between.
let canvasRect = null;
const invalidateCanvasRect = () => {
    canvasRect = null;
};
new ResizeObserver(invalidateCanvasRect).observe(canvas);
globalThis.addEventListener("resize", invalidateCanvasRect);
globalThis.addEventListener("scroll", invalidateCanvasRect, {
    capture: true,
    passive: true,
});

const sendPointer = (event, phase) => {
    event.preventDefault();
    canvasRect ??= canvas.getBoundingClientRect();
    const rect = canvasRect;
    hostEvents.pointer(
        phase,
        event.clientX - rect.left,
        event.clientY - rect.top,
        rect.width,
        rect.height,
    );
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
            hostEvents.wheel(rounded);
        }
    },
    // preventDefault needs a non-passive listener, which wheel handlers default to.
    { passive: false },
);

const sendKey = (event, down) => {
    if (hostEvents.reservedKey(event.code)) {
        event.preventDefault();
    }
    hostEvents.key(event.code, down);
};
globalThis.addEventListener("keydown", (event) => sendKey(event, true));
globalThis.addEventListener("keyup", (event) => sendKey(event, false));

// Focus and visibility are separate losses and either one must freeze the game: a hidden
// tab stops getting animation frames but keeps its audio, while a window merely pushed
// behind another stays visible and keeps ticking at full speed.
const syncActive = () => {
    const visible = document.visibilityState === "visible";
    hostEvents.active(visible && document.hasFocus(), !visible);
};
globalThis.addEventListener("focus", syncActive);
globalThis.addEventListener("blur", syncActive);
document.addEventListener("visibilitychange", syncActive);

// A mobile browser can discard a backgrounded page without giving it another
// visibilitychange, and pagehide is the last callback such a page receives. Treating it as a
// deactivation is a best-effort request for the pending save, not an assurance of one:
// nothing here can hold the page open until the owner thread has drained the ring. pageshow
// covers the other direction, since a back/forward-cache restore fires no visibilitychange.
globalThis.addEventListener("pagehide", () => hostEvents.active(false, true));
globalThis.addEventListener("pageshow", syncActive);

syncActive();

// Booting only gets as far as a game standing ready; this is what lets it move. Until the
// player presses Play the loop runs but the game does not, so nothing simulates and no sound
// plays behind the splash.
globalThis.ctrdxStart = () => hostEvents.start();
globalThis.ctrdxReady?.();

// Last, so the error boundary still covers ctrdxReady itself. Boot is not over until the
// thing that announces boot is over has run.
globalThis.ctrdxBootComplete?.();
