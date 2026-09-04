import * as hostEvents from "./host-events.js";

// Hands the canvas to the managed owner thread's worker. Ownership is permanent:
// the browser thread can never draw to this canvas or resize its backing store
// again, so nothing may fall back to browser-thread rendering after this returns.
export function transferCanvasToThread(canvasId, threadId) {
    const canvas = document.getElementById(canvasId);
    const worker = globalThis.ctrdxWasmModule?.PThread?.pthreads?.[threadId];
    if (canvas === null || !worker) {
        return [];
    }

    const ratio = Math.min(globalThis.devicePixelRatio || 1, 2);
    const cssWidth = Math.max(1, Math.round(canvas.clientWidth));
    const cssHeight = Math.max(1, Math.round(canvas.clientHeight));

    let offscreen;
    try {
        offscreen = canvas.transferControlToOffscreen();
    } catch (error) {
        console.info(
            JSON.stringify({
                marker: "ctrdx-host",
                boundary: "canvas-transfer",
                threadId,
                message: String(error),
            }),
        );
        return [];
    }

    worker.postMessage({ cmd: "ctrdx-transfer-canvas", canvas: offscreen }, [
        offscreen,
    ]);
    return [
        cssWidth,
        cssHeight,
        Math.max(1, Math.round(cssWidth * ratio)),
        Math.max(1, Math.round(cssHeight * ratio)),
    ];
}

let watchedCanvas = null;
let devicePixelRatioQuery = null;

function report(canvas) {
    hostEvents.resize(
        Math.max(1, canvas.clientWidth),
        Math.max(1, canvas.clientHeight),
        Math.min(globalThis.devicePixelRatio || 1, 2),
    );
}

// A ResizeObserver reports the canvas box changing, but not the page moving to a display of a
// different pixel density. A media query pinned to the current ratio covers that: it stops
// matching the moment the ratio changes, and is re-armed against the new one.
function watchDevicePixelRatio() {
    devicePixelRatioQuery?.removeEventListener(
        "change",
        onDevicePixelRatioChange,
    );
    devicePixelRatioQuery = globalThis.matchMedia(
        `(resolution: ${globalThis.devicePixelRatio || 1}dppx)`,
    );
    devicePixelRatioQuery.addEventListener("change", onDevicePixelRatioChange);
}

function onDevicePixelRatioChange() {
    if (watchedCanvas !== null) {
        report(watchedCanvas);
        watchDevicePixelRatio();
    }
}

// The canvas fills the viewport through the stylesheet, so its CSS box needs no help from
// here. Measuring it rather than sizing it is what lets the game adopt whatever shape the
// window or the device is, instead of the page choosing a shape and the game obeying it.
export function watchCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return;
    }
    watchedCanvas = canvas;
    new ResizeObserver(() => report(canvas)).observe(canvas);
    watchDevicePixelRatio();
    report(canvas);
}

export function documentBaseUrl() {
    return document.baseURI;
}
