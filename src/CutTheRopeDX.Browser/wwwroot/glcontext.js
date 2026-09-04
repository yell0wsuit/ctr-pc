// Hands the canvas to the managed owner thread's worker. Ownership is permanent:
// the browser thread can never draw to this canvas or resize its backing store
// again, so nothing may fall back to browser-thread rendering after this returns.
export function transferCanvasToThread(canvasId, threadId) {
    const canvas = document.getElementById(canvasId);
    const worker = globalThis.ctrdxWasmModule?.PThread?.pthreads?.[threadId];
    if (canvas === null || !worker) {
        return [];
    }

    measure(canvas);
    appliedDevicePixelRatio = measuredRatio;

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
        Math.max(1, Math.round(canvas.clientWidth)),
        Math.max(1, Math.round(canvas.clientHeight)),
        measuredWidth,
        measuredHeight,
    ];
}

// Retained until normal boot moves to the owner thread. The render probe transfers
// the canvas before this path can run, so the two ownership models never mix.
export function createContext(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return 0;
    }
    const attributes = {
        alpha: 1,
        depth: 1,
        stencil: 8,
        antialias: 0,
        premultipliedAlpha: 1,
        preserveDrawingBuffer: 0,
        majorVersion: 2,
        minorVersion: 0,
        enableExtensionsByDefault: 1,
    };
    const GL = globalThis.ctrdxWasmModule?.GL;
    if (!GL) {
        return 0;
    }
    const handle = GL.createContext(canvas, attributes);
    if (!handle) {
        return 0;
    }
    GL.makeContextCurrent(handle);
    return 0;
}

// The ratio canvasSize last applied to the backing store. Read by canvasDevicePixelRatio
// so the ratio and the size a caller acts on always describe the same measurement.
let appliedDevicePixelRatio = 1;

// Bumped whenever a measurement finds a shape the game has not adopted yet. The game loop
// polls this once a frame and only asks for the size itself when it moves, so the steady
// state costs one integer across the interop boundary rather than a DOM measurement, an
// array allocation and a marshalled copy sixty times a second.
let canvasGeneration = 0;

let measuredWidth = 0;
let measuredHeight = 0;
let measuredRatio = 0;
let watchedCanvas = null;
let devicePixelRatioQuery = null;

// Measures without touching the canvas: the backing store is only resized from canvasSize,
// where the caller goes on to rebuild the renderer's surface from the same numbers. Doing it
// here instead would clear the drawing buffer while the renderer still believed the old size,
// and the frames in between would be drawn against a buffer nothing agreed on.
function measure(canvas) {
    const ratio = Math.min(globalThis.devicePixelRatio || 1, 2);
    const width = Math.max(1, Math.round(canvas.clientWidth * ratio));
    const height = Math.max(1, Math.round(canvas.clientHeight * ratio));
    if (
        width !== measuredWidth ||
        height !== measuredHeight ||
        ratio !== measuredRatio
    ) {
        measuredWidth = width;
        measuredHeight = height;
        measuredRatio = ratio;
        canvasGeneration++;
    }
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
        measure(watchedCanvas);
        watchDevicePixelRatio();
    }
}

export function canvasSize(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return [0, 0];
    }
    measure(canvas);
    if (canvas.width !== measuredWidth || canvas.height !== measuredHeight) {
        canvas.width = measuredWidth;
        canvas.height = measuredHeight;
    }
    appliedDevicePixelRatio = measuredRatio;
    return [measuredWidth, measuredHeight];
}

// The canvas fills the viewport through the stylesheet, so its CSS box needs no help from
// here. Measuring it rather than sizing it is what lets the game adopt whatever shape the
// window or the device is, instead of the page choosing a shape and the game obeying it.
// Starts reporting canvas shape changes through canvasChangeCount, so callers stop having to
// measure the DOM to discover that nothing moved.
export function watchCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return;
    }
    watchedCanvas = canvas;
    new ResizeObserver(() => measure(canvas)).observe(canvas);
    watchDevicePixelRatio();
    measure(canvas);
}

export function canvasChangeCount() {
    return canvasGeneration;
}

export function canvasDevicePixelRatio() {
    return appliedDevicePixelRatio;
}

export function documentBaseUrl() {
    return document.baseURI;
}
