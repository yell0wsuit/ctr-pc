const EXPECTED_PIXEL = [17, 34, 51, 255, 0];

const currentContext = () => globalThis.ctrdxWasmModule?.GL?.currentContext;
const currentGL = () => currentContext()?.GLctx;

export function isRequested() {
    return globalThis.ctrdxRenderProbe === true;
}

export function executionContext() {
    return typeof Window !== "undefined" && globalThis instanceof Window
        ? "window"
        : "worker";
}

export function currentContextStatus() {
    const context = currentContext();
    const usable = typeof context?.GLctx?.readPixels === "function";
    return [context ? 1 : 0, usable ? 1 : 0];
}

export function clearErrors() {
    const gl = currentGL();
    if (!gl) {
        return false;
    }

    while (gl.getError() !== gl.NO_ERROR) {
        // Drain stale GL errors so readback owns the next reported error.
    }
    return true;
}

export function readCenterPixel(canvasId) {
    const canvas = document.getElementById(canvasId);
    const gl = currentGL();
    if (!canvas || !gl) {
        return [];
    }

    const pixel = new Uint8Array(4);
    gl.finish();
    gl.readPixels(
        Math.floor(canvas.width / 2),
        Math.floor(canvas.height / 2),
        1,
        1,
        gl.RGBA,
        gl.UNSIGNED_BYTE,
        pixel,
    );
    return [pixel[0], pixel[1], pixel[2], pixel[3], gl.getError()];
}

export function isExpectedPixel(values) {
    return (
        values != null &&
        values.length === EXPECTED_PIXEL.length &&
        EXPECTED_PIXEL.every((expected, index) => values[index] === expected)
    );
}
