// Custom cursor, as a browser can do it.
//
// The desktop host swaps a native cursor between two bitmaps rather than drawing one into the
// scene, so the CSS equivalent is the faithful port: same two images, same top-left hotspot,
// and the pointer keeps the responsiveness of a real cursor instead of trailing a frame behind.

const GAME_WIDTH = 2560;

// Below this the resize is not worth redrawing for.
const SCALE_EPSILON = 0.01;

const canvas = document.getElementById("game");
// The cutscene overlay covers the canvas, so a cursor set only there would be invisible
// for the whole of a cutscene - including the paused state, where Core asks for it back.
const surfaces = [canvas, document.getElementById("movie")];
const sources = {
    idle: "./content/images/cursor.webp",
    pressed: "./content/images/cursor_active.webp",
};

const bitmaps = {};
const scaled = {};
let enabled = true;
let pressed = false;
let appliedScale = 0;

/** Applies the cursor the current state calls for. */
function apply() {
    if (!enabled) {
        for (const surface of surfaces) {
            surface.style.cursor = "none";
        }
        return;
    }
    const url = scaled[pressed ? "pressed" : "idle"];
    // Until the bitmaps load there is nothing to show, so the pointer stays the system one.
    const value = url ? `url("${url}") 0 0, auto` : "auto";
    for (const surface of surfaces) {
        surface.style.cursor = value;
    }
}

/** Redraws both bitmaps for the canvas's current size, if that size changed enough. */
function rescale() {
    const width = canvas.clientWidth;
    if (width === 0) {
        return;
    }

    const scale = width / GAME_WIDTH;
    if (Math.abs(scale - appliedScale) < SCALE_EPSILON) {
        return;
    }
    appliedScale = scale;

    for (const [name, image] of Object.entries(bitmaps)) {
        const target = document.createElement("canvas");
        target.width = Math.max(1, Math.round(image.naturalWidth * scale));
        target.height = Math.max(1, Math.round(image.naturalHeight * scale));
        target
            .getContext("2d")
            .drawImage(image, 0, 0, target.width, target.height);
        scaled[name] = target.toDataURL("image/png");
    }

    apply();
}

for (const [name, src] of Object.entries(sources)) {
    const image = new Image();
    image.decoding = "async";
    image.addEventListener("load", () => {
        bitmaps[name] = image;
        // A load invalidates whatever scale was applied without this bitmap.
        appliedScale = 0;
        rescale();
    });
    image.src = src;
}

new ResizeObserver(rescale).observe(canvas);

/**
 * Shows or hides the cursor over the canvas. Core hides it while a cutscene plays.
 *
 * @param {boolean} value
 */
export function setEnabled(value) {
    enabled = value;
    apply();
}

/**
 * Switches between the idle and pressed bitmaps.
 *
 * @param {boolean} value
 */
export function setPressed(value) {
    pressed = value;
    apply();
}
