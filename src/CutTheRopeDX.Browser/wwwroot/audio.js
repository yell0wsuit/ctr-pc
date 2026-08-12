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
    const pending =
        buffer === undefined ? decodes.get(key) : Promise.resolve(buffer);
    if (pending === undefined) {
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
    void ensureContext()
        .resume()
        .catch(() => {
            // Browsers reject resume outside a user gesture. The global gesture handlers
            // below will retry while this queued voice remains ready to start.
        });
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

function resumeFromGesture() {
    void resume();
}

globalThis.addEventListener("pointerdown", resumeFromGesture, {
    passive: true,
});
globalThis.addEventListener("keydown", resumeFromGesture, { passive: true });
