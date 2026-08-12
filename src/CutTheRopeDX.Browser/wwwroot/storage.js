// localStorage accessors. Synchronous by nature, which is exactly what Preferences needs:
// it reads and writes from ordinary game code with no await points available.

export function read(key) {
    return globalThis.localStorage.getItem(key);
}

export function write(key, value) {
    globalThis.localStorage.setItem(key, value);
}

export function keysWithPrefix(prefix) {
    const found = [];
    for (let i = 0; i < globalThis.localStorage.length; i++) {
        const key = globalThis.localStorage.key(i);
        if (key !== null && key.startsWith(prefix)) {
            found.push(key);
        }
    }
    return found;
}
