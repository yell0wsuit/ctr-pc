const EXPECTED_PIXEL = [17, 34, 51, 255, 0];

export function isRequested() {
    return globalThis.ctrdxRenderProbe === true;
}

export function executionContext() {
    return typeof Window !== "undefined" && globalThis instanceof Window
        ? "window"
        : "worker";
}

export function isExpectedPixel(values) {
    return (
        values != null &&
        values.length === EXPECTED_PIXEL.length &&
        EXPECTED_PIXEL.every((expected, index) => values[index] === expected)
    );
}
