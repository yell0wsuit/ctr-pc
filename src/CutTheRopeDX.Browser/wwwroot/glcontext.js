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

export function fitCanvasToViewport(viewportWidth, viewportHeight) {
    const nativeWidth = 2560;
    const nativeHeight = 1440;
    const scale = Math.min(viewportWidth / nativeWidth, viewportHeight / nativeHeight);
    const width = Math.max(1, Math.round(nativeWidth * scale));
    const height = Math.max(1, Math.round(nativeHeight * scale));
    return {
        width,
        height,
        left: Math.round((viewportWidth - width) / 2),
        top: Math.round((viewportHeight - height) / 2),
    };
}

function resizeCanvasElement(canvas) {
    const layout = fitCanvasToViewport(window.innerWidth, window.innerHeight);
    const width = `${layout.width}px`;
    const height = `${layout.height}px`;
    const left = `${layout.left}px`;
    const top = `${layout.top}px`;
    if (
        canvas.style.width !== width ||
        canvas.style.height !== height ||
        canvas.style.left !== left ||
        canvas.style.top !== top
    ) {
        canvas.style.width = width;
        canvas.style.height = height;
        canvas.style.left = left;
        canvas.style.top = top;
    }
}

export function createContext(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return 0;
    }
    resizeCanvasElement(canvas);
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

export function canvasSize(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (canvas === null) {
        return [0, 0];
    }
    resizeCanvasElement(canvas);
    const ratio = Math.min(globalThis.devicePixelRatio || 1, 2);
    const width = Math.max(1, Math.round(canvas.clientWidth * ratio));
    const height = Math.max(1, Math.round(canvas.clientHeight * ratio));
    if (canvas.width !== width || canvas.height !== height) {
        canvas.width = width;
        canvas.height = height;
    }
    return [width, height];
}

export function documentBaseUrl() {
    return document.baseURI;
}
