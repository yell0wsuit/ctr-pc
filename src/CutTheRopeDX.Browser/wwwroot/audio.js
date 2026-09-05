// WebAudio graph for the game. Decoded buffers are keyed by content path; each playing
// voice gets an integer handle the managed side uses to stop it or change its volume.

let context = null;
const buffers = new Map();
const decodes = new Map();
const voices = new Map();
let nextVoice = 1;

function ensureContext() {
    if (context === null) {
        context = new (
            globalThis.AudioContext || globalThis.webkitAudioContext
        )();
    }
    return context;
}

export async function resume() {
    await ensureContext().resume();
}

async function loadBuffer(key, url) {
    if (buffers.has(key)) {
        return buffers.get(key);
    }
    if (decodes.has(key)) {
        return await decodes.get(key);
    }

    const pending = (async () => {
        const response = await fetch(url);
        if (!response.ok) {
            return null;
        }
        const bytes = await response.arrayBuffer();
        const buffer = await ensureContext().decodeAudioData(bytes);
        buffers.set(key, buffer);
        return buffer;
    })();

    decodes.set(key, pending);
    try {
        return await pending;
    } finally {
        decodes.delete(key);
    }
}

export async function decode(key, url) {
    return (await loadBuffer(key, url)) === null ? 0 : 1;
}

function startVoice(handle, buffer) {
    const voice = voices.get(handle);
    if (voice === undefined) {
        return;
    }
    voice.buffer = buffer;
    if (voice.paused) {
        return;
    }

    const ctx = ensureContext();
    const source = ctx.createBufferSource();
    const gain = ctx.createGain();
    source.buffer = buffer;
    source.loop = voice.loop;
    gain.gain.value = voice.volume;
    source.connect(gain).connect(ctx.destination);

    source.onended = () => {
        if (voices.get(handle)?.source === source) {
            voices.delete(handle);
        }
    };
    voice.source = source;
    voice.gain = gain;
    voice.startedAt = ctx.currentTime;
    source.start(0, voice.offset);
}

export function play(key, loop, volume) {
    const buffer = buffers.get(key);
    const pending = buffer === undefined ? decodes.get(key) : undefined;
    if (buffer === undefined && pending === undefined) {
        return 0;
    }

    const handle = nextVoice++;
    voices.set(handle, {
        source: null,
        gain: null,
        buffer: null,
        loop,
        volume,
        paused: false,
        offset: 0,
        startedAt: 0,
    });
    resumeIfSuspended();

    // Everything the game plays during a level is already decoded, and going through a promise
    // for those would put a microtask between the cut and its sound for no reason. Only a
    // buffer still in flight has to wait.
    if (buffer !== undefined) {
        startVoice(handle, buffer);
        return handle;
    }

    void pending.then((decoded) => {
        if (decoded === null) {
            voices.delete(handle);
            return;
        }
        startVoice(handle, decoded);
    });
    return handle;
}

export function stop(handle) {
    const voice = voices.get(handle);
    if (voice !== undefined) {
        if (voice.source !== null) {
            try {
                voice.source.stop();
            } catch {
                // Already ended; onended has cleaned up.
            }
        }
        voices.delete(handle);
    }
}

export function setVolume(handle, volume) {
    const voice = voices.get(handle);
    if (voice !== undefined) {
        voice.volume = volume;
        if (voice.gain !== null) {
            voice.gain.gain.value = volume;
        }
    }
}

export function pauseVoice(handle) {
    const voice = voices.get(handle);
    if (voice === undefined || voice.paused) {
        return;
    }

    voice.paused = true;
    if (voice.source === null) {
        return;
    }

    const elapsed = Math.max(0, ensureContext().currentTime - voice.startedAt);
    const duration = voice.buffer?.duration ?? 0;
    voice.offset += elapsed;
    if (duration > 0) {
        voice.offset = voice.loop
            ? voice.offset % duration
            : Math.min(voice.offset, duration);
    }

    const source = voice.source;
    voice.source = null;
    voice.gain = null;
    source.onended = null;
    try {
        source.stop();
    } catch {
        // The source ended between the state check and stop; the retained offset still resumes.
    }
}

export function resumeVoice(handle) {
    const voice = voices.get(handle);
    if (voice === undefined || !voice.paused) {
        return;
    }

    voice.paused = false;
    if (voice.buffer !== null) {
        startVoice(handle, voice.buffer);
    }
}

export function isPlaying(handle) {
    return voices.has(handle);
}

export function durationOf(key) {
    const buffer = buffers.get(key);
    return buffer === undefined ? 0 : buffer.duration;
}

// A running context needs no resuming, so neither the play path nor a gesture pays for a
// promise once the browser has let the graph start.
function resumeIfSuspended() {
    if (ensureContext().state === "running") {
        return;
    }
    void resume().catch(() => {
        // Browsers reject resume outside a user gesture. The gesture handlers below retry
        // while any queued voice remains ready to start.
    });
}

function resumeFromGesture() {
    if (context !== null && context.state === "running") {
        globalThis.removeEventListener("pointerdown", resumeFromGesture);
        globalThis.removeEventListener("keydown", resumeFromGesture);
        return;
    }
    void resume().catch(() => {
        // The gesture was not one the browser accepts as user activation; the next one will be.
    });
}

function armGestureResume() {
    globalThis.addEventListener("pointerdown", resumeFromGesture, {
        passive: true,
    });
    globalThis.addEventListener("keydown", resumeFromGesture, { passive: true });
}
armGestureResume();

// Safari suspends the graph while the page is in the background, and has an "interrupted"
// state of its own that a phone call or another app taking the audio device puts it in.
// Either way the context stops being "running" long after resumeFromGesture removed itself,
// so coming back to a visible page re-arms the listeners it took off. Registering a listener
// that is already attached is a no-op, so this cannot stack duplicates.
document.addEventListener("visibilitychange", () => {
    if (document.visibilityState !== "visible") {
        return;
    }
    if (context !== null && context.state !== "running") {
        resumeIfSuspended();
        armGestureResume();
    }
});
