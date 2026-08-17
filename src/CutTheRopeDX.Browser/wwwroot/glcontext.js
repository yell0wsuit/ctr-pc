// Creates the WebGL2 context Skia renders into. Emscripten's GL registry is used
// rather than canvas.getContext directly, because Skia's GPU backend resolves its GL
// entry points through that registry - a context created outside it is invisible to Skia.

function getGL() {
    const gl = globalThis.ctrdxWasmModule?.GL;
    if (!gl) {
        throw new Error("Emscripten GL registry unavailable");
    }
    return gl;
}

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
    const GL = getGL();
    const handle = GL.createContext(canvas, attributes);
    if (!handle) {
        return 0;
    }
    GL.makeContextCurrent(handle);
    // Skia renders to the default framebuffer of the context it is handed.
    return 0;
}

// The ratio canvasSize last applied to the backing store. Read by canvasDevicePixelRatio
// so the ratio and the size a caller acts on always describe the same measurement.
let appliedDevicePixelRatio = 1;

// The canvas fills the viewport through the stylesheet, so its CSS box needs no help from
// here. Measuring it rather than sizing it is what lets the game adopt whatever shape the
// window or the device is, instead of the page choosing a shape and the game obeying it.
export function canvasSize(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return [0, 0];
    }
    const ratio = Math.min(globalThis.devicePixelRatio || 1, 2);
    appliedDevicePixelRatio = ratio;
    const width = Math.max(1, Math.round(canvas.clientWidth * ratio));
    const height = Math.max(1, Math.round(canvas.clientHeight * ratio));
    if (canvas.width !== width || canvas.height !== height) {
        canvas.width = width;
        canvas.height = height;
    }
    return [width, height];
}

export function canvasDevicePixelRatio() {
    return appliedDevicePixelRatio;
}

export function documentBaseUrl() {
    return document.baseURI;
}
