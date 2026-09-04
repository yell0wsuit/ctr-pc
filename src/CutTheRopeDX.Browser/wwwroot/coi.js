// Registers the cross-origin isolation service worker and reloads once so its
// headers apply to the document.
//
// Adapted from https://github.com/yell0wsuit/coi-sw. Licensed under the MIT
// License; see coi-sw.LICENSE.txt.

(() => {
    const RELOAD_FLAG = "ctrdx-coi-reloaded";
    const CONTROLLER_TIMEOUT_MS = 15 * 1000;

    let finishReady;
    globalThis.ctrdxIsolationReady = new Promise((resolve) => {
        finishReady = resolve;
    });

    const isolated = globalThis.crossOriginIsolated === true;
    if (isolated) {
        sessionStorage.removeItem(RELOAD_FLAG);
        globalThis.ctrdxServiceWorkerRegistration =
            navigator.serviceWorker?.getRegistration("./") ??
            Promise.resolve(null);
        finishReady(true);
        return;
    }

    if (!("serviceWorker" in navigator)) {
        globalThis.ctrdxServiceWorkerRegistration = Promise.resolve(null);
        finishReady(false);
        return;
    }

    if (sessionStorage.getItem(RELOAD_FLAG) === "1") {
        globalThis.ctrdxServiceWorkerRegistration =
            navigator.serviceWorker.getRegistration("./");
        finishReady(false);
        return;
    }

    let reloadRequested = false;
    const reloadForIsolation = () => {
        if (reloadRequested) {
            return;
        }

        reloadRequested = true;
        sessionStorage.setItem(RELOAD_FLAG, "1");
        globalThis.location.replace(globalThis.location.href);
    };

    const registrationPromise = navigator.serviceWorker.register("./coi-sw.js", {
        scope: "./",
        updateViaCache: "none",
    });
    globalThis.ctrdxServiceWorkerRegistration = registrationPromise;

    const controllerTimeout = globalThis.setTimeout(() => {
        finishReady(false);
    }, CONTROLLER_TIMEOUT_MS);
    navigator.serviceWorker.addEventListener(
        "controllerchange",
        () => {
            globalThis.clearTimeout(controllerTimeout);
            reloadForIsolation();
        },
        { once: true },
    );

    registrationPromise
        .then((registration) => {
            const promote = (worker) => {
                if (worker?.state === "installed") {
                    worker.postMessage({ type: "skip-waiting" });
                }
            };

            promote(registration.waiting);
            const installing = registration.installing;
            installing?.addEventListener("statechange", () => {
                if (installing.state === "redundant") {
                    globalThis.clearTimeout(controllerTimeout);
                    finishReady(false);
                    return;
                }
                promote(installing);
            });

            // An already-active worker from a previous visit never fires
            // controllerchange for this document.
            if (registration.active && !navigator.serviceWorker.controller) {
                globalThis.clearTimeout(controllerTimeout);
                reloadForIsolation();
            }
        })
        .catch((error) => {
            globalThis.clearTimeout(controllerTimeout);
            console.warn("cross-origin isolation registration failed:", error);
            finishReady(false);
        });
})();
