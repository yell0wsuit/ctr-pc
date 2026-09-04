// The browser thread's write side of the event ring. Wasm linear memory is a
// SharedArrayBuffer in a threaded build, so events reach the game thread without
// a message, a structured clone, or an allocation per pointer move.

const HEADER_BYTES = 16;
const RECORD_BYTES = 24;
const CAPACITY = 1024;
// Pointer moves are the only high-volume event. Stop admitting them before the
// ring is full so releases, lifecycle transitions, and resizes retain space.
const CONTROL_RESERVE = 32;

const WRITE_INDEX = 0;
const READ_INDEX = 1;
const DROPPED = 2;

const KIND_POINTER = 1;
const KIND_KEY = 2;
const KIND_WHEEL = 3;
const KIND_ACTIVE = 4;
const KIND_RESIZE = 5;

const KEY_IDS = {
    // Q and R stand in for Escape and F5: a browser keeps both of those for
    // itself, and neither can be reliably taken back from it.
    "KeyQ": 1,
    "KeyR": 2,
    "Space": 3,
    "Enter": 4,
    "ArrowLeft": 5,
    "ArrowRight": 6,
};

let baseWord = 0;
let view = null;
let viewBuffer = null;
let ownerWorker = null;

// A grown wasm memory replaces the buffer behind every typed-array view, so the
// view is rebuilt whenever the buffer identity changes. The ring's address does
// not move, only the window onto it.
function heap() {
    const memory = globalThis.ctrdxWasmModule.wasmMemory;
    if (memory.buffer !== viewBuffer) {
        viewBuffer = memory.buffer;
        view = new Int32Array(memory.buffer);
    }
    return view;
}

function write(kind, word0, word1, word2, word3, word4, droppable = false) {
    if (baseWord === 0) {
        return false;
    }

    const words = heap();
    const writeIndex = words[baseWord + WRITE_INDEX];
    const readIndex = Atomics.load(words, baseWord + READ_INDEX);
    const limit = droppable ? CAPACITY - CONTROL_RESERVE : CAPACITY;
    if (writeIndex - readIndex >= limit) {
        Atomics.add(words, baseWord + DROPPED, 1);
        return false;
    }

    const slot =
        baseWord +
        HEADER_BYTES / 4 +
        ((writeIndex >>> 0) & (CAPACITY - 1)) * (RECORD_BYTES / 4);
    words[slot] = kind;
    words[slot + 1] = word0;
    words[slot + 2] = word1;
    words[slot + 3] = word2;
    words[slot + 4] = word3;
    words[slot + 5] = word4;

    // Published last, so the reader never sees a slot before it is filled.
    Atomics.store(words, baseWord + WRITE_INDEX, writeIndex + 1);
    return true;
}

const floatBits = new DataView(new ArrayBuffer(4));
function bits(value) {
    floatBits.setFloat32(0, value, true);
    return floatBits.getInt32(0, true);
}

export function attach(address, threadId) {
    baseWord = address / 4;
    ownerWorker =
        globalThis.ctrdxWasmModule?.PThread?.pthreads?.[threadId] ?? null;
}

export function pointer(phase, offsetX, offsetY, rectWidth, rectHeight) {
    write(
        KIND_POINTER,
        phase,
        bits(offsetX),
        bits(offsetY),
        bits(rectWidth),
        bits(rectHeight),
        phase === 1,
    );
}

export function key(code, down) {
    const id = KEY_IDS[code];
    if (id !== undefined) {
        write(KIND_KEY, down ? 1 : 0, id, 0, 0, 0);
    }
}

export function wheel(delta) {
    write(KIND_WHEEL, delta, 0, 0, 0, 0);
}

export function active(isActive) {
    if (write(KIND_ACTIVE, isActive ? 1 : 0, 0, 0, 0, 0) && !isActive) {
        ownerWorker?.postMessage({ ctrdxWake: 1 });
    }
}

export function resize(cssWidth, cssHeight, devicePixelRatio) {
    write(
        KIND_RESIZE,
        bits(cssWidth),
        bits(cssHeight),
        bits(devicePixelRatio),
        0,
        0,
    );
}

export function reservedKey(code) {
    return code === "Space" || code === "ArrowLeft" || code === "ArrowRight";
}
