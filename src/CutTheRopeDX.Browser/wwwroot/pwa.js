// Service worker registration, the update prompt, and the request for persistent storage.
//
// A worker that finds new assets installs alongside the running one and then waits, so a
// version never swaps in underneath a session in progress. The dialog is how the player asks
// for it: the waiting worker takes over and the page reloads onto the new build.

const UPDATE_CHECK_INTERVAL_MS = 15 * 60 * 1000;
const CONTROLLER_TIMEOUT_MS = 15 * 1000;

requestPersistentStorage();

let registrationPromise = Promise.resolve(null);
if ("serviceWorker" in navigator) {
    // updateViaCache: "none" keeps the HTTP cache away from the worker script itself. It is
    // the one file whose freshness decides whether an update is ever noticed.
    registrationPromise = navigator.serviceWorker
        .register("./service-worker.js", { updateViaCache: "none" })
        .then((registration) => {
            watch(registration);
            return registration;
        })
        .catch((error) => {
            console.warn("service worker registration failed:", error);
            return null;
        });
}

// GitHub Pages cannot set COOP/COEP response headers. The published service worker
// adds them, but a document only becomes isolated after a controlled navigation.
// main.js awaits this promise, so the threaded runtime is never imported during the
// install/reload transition.
globalThis.ctrdxIsolationReady = prepareCrossOriginIsolation(registrationPromise);

async function prepareCrossOriginIsolation(registrationResult) {
    if (globalThis.crossOriginIsolated === true) {
        sessionStorage.removeItem("ctrdx-isolation-reload");
        return true;
    }

    const registration = await registrationResult;
    if (registration === null) {
        return false;
    }

    const previousController = navigator.serviceWorker.controller;
    const controllerChanged = waitForControllerChange();

    // An older caching worker may control the page while the isolation-capable
    // worker installs. This boot cannot run anyway, so promote the new worker
    // immediately instead of showing the normal in-game update prompt.
    await registration.update().catch(() => {});
    const candidate = registration.waiting ?? registration.installing;
    let controllerChangeExpected = previousController === null;
    if (candidate !== null) {
        await waitUntilInstalled(candidate);
        if (candidate.state === "redundant") {
            return false;
        }
        if (candidate.state === "installed" && previousController !== null) {
            controllerChangeExpected = true;
            candidate.postMessage({ type: "skip-waiting" });
        }
    }

    if (
        controllerChangeExpected &&
        navigator.serviceWorker.controller === previousController
    ) {
        if (!(await controllerChanged)) {
            return false;
        }
    }

    // Avoid an infinite reload if a browser accepts the worker but still declines
    // isolation. The next load falls through to main.js's explicit error instead.
    if (sessionStorage.getItem("ctrdx-isolation-reload") === "attempted") {
        return false;
    }
    sessionStorage.setItem("ctrdx-isolation-reload", "attempted");
    globalThis.location.reload();
    return new Promise(() => {});
}

function waitForControllerChange() {
    return new Promise((resolve) => {
        let settled = false;
        const finish = (changed) => {
            if (!settled) {
                settled = true;
                globalThis.clearTimeout(timeout);
                navigator.serviceWorker.removeEventListener(
                    "controllerchange",
                    onChange,
                );
                resolve(changed);
            }
        };
        const onChange = () => finish(true);
        const timeout = globalThis.setTimeout(
            () => finish(false),
            CONTROLLER_TIMEOUT_MS,
        );
        navigator.serviceWorker.addEventListener("controllerchange", onChange);
    });
}

function waitUntilInstalled(worker) {
    if (worker.state === "installed" || worker.state === "activated") {
        return Promise.resolve();
    }
    return new Promise((resolve) => {
        worker.addEventListener("statechange", () => {
            if (
                worker.state === "installed" ||
                worker.state === "activated" ||
                worker.state === "redundant"
            ) {
                resolve();
            }
        });
    });
}

/**
 * Watches a registration for a worker that has installed and is waiting to take over.
 *
 * @param {ServiceWorkerRegistration} registration
 */
function watch(registration) {
    if (registration.waiting) {
        promptForUpdate(registration.waiting);
    }

    registration.addEventListener("updatefound", () => {
        const installing = registration.installing;
        if (installing === null) {
            return;
        }
        installing.addEventListener("statechange", () => {
            // Without a controller this is the first install rather than an update, and it
            // activates on its own — there is nothing for the player to decide.
            if (
                installing.state === "installed" &&
                navigator.serviceWorker.controller
            ) {
                promptForUpdate(installing);
            }
        });
    });

    // The browser only checks for a new worker on navigation, and this page is meant to be
    // left open. Checking when the tab comes back into view covers the long sessions.
    let lastCheck = Date.now();
    document.addEventListener("visibilitychange", () => {
        if (
            document.visibilityState === "visible" &&
            Date.now() - lastCheck > UPDATE_CHECK_INTERVAL_MS
        ) {
            lastCheck = Date.now();
            registration.update().catch(() => {});
        }
    });
}

/**
 * Offers the waiting worker to the player.
 *
 * @param {ServiceWorker} waiting
 */
function promptForUpdate(waiting) {
    const dialog = document.getElementById("update");
    if (dialog === null || dialog.open) {
        return;
    }

    document.getElementById("update-later").onclick = () => dialog.close();
    document.getElementById("update-now").onclick = () => {
        dialog.close();
        // Reload once the new worker is actually in control, so the fresh assets are the ones
        // served. Guarded because controllerchange also fires on the very first activation.
        let reloading = false;
        navigator.serviceWorker.addEventListener("controllerchange", () => {
            if (!reloading) {
                reloading = true;
                location.reload();
            }
        });
        waiting.postMessage({ type: "skip-waiting" });
    };

    dialog.showModal();
}

/**
 * Asks the browser not to evict this origin's storage.
 *
 * The offline caches and the player's save share one evictable pool, which a browser is free
 * to clear when the device runs short of room. Losing it costs the player their progress and
 * costs the next visit a fresh download of the content. Chromium grants persistence silently
 * to an app that has been installed or used enough; the browsers that ask instead raise a
 * permission prompt on the call, so this waits for the first gesture rather than firing at
 * page load, where such a prompt would arrive out of nowhere.
 */
function requestPersistentStorage() {
    if (typeof navigator.storage?.persist !== "function") {
        return;
    }

    const ask = async () => {
        globalThis.removeEventListener("pointerdown", ask);
        globalThis.removeEventListener("keydown", ask);
        try {
            if (!(await navigator.storage.persisted())) {
                await navigator.storage.persist();
            }
        } catch (error) {
            // A refusal is the expected outcome on a browser that declines, and eviction is a
            // risk rather than a failure there is anything to recover from here.
            console.warn("persistent storage request failed:", error);
        }
    };

    globalThis.addEventListener("pointerdown", ask, { passive: true });
    globalThis.addEventListener("keydown", ask);
}
