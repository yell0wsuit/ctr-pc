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

    // A browser set to block all site data throws on any sessionStorage access rather than
    // answering null, and main.js waits on ctrdxIsolationReady before it does anything at
    // all - so a throw that escapes this file leaves the page on its spinner for good, with
    // nothing logged to say why. The flag only guards against a reload loop, so losing it
    // degrades to "no flag" instead of taking the boot down.
    const readFlag = () => {
        try {
            return sessionStorage.getItem(RELOAD_FLAG);
        } catch {
            return null;
        }
    };
    const writeFlag = () => {
        try {
            sessionStorage.setItem(RELOAD_FLAG, "1");
            return true;
        } catch {
            return false;
        }
    };
    const clearFlag = () => {
        try {
            sessionStorage.removeItem(RELOAD_FLAG);
        } catch {
            // Nothing recorded it, so there is nothing to clear.
        }
    };

    try {
        bootstrapIsolation();
    } catch (error) {
        // Isolation either happened or it did not, and crossOriginIsolated is the authority
        // on which. Reporting it here keeps an already-isolated page - the one whose only
        // failure was touching storage - booting normally.
        console.warn("cross-origin isolation bootstrap failed:", error);
        globalThis.ctrdxServiceWorkerRegistration ??= Promise.resolve(null);
        finishReady(globalThis.crossOriginIsolated === true);
    }

    function registerWorker() {
        return navigator.serviceWorker.register("./coi-sw.js", {
            scope: "./",
            updateViaCache: "none",
        });
    }

    function bootstrapIsolation() {
        const isolated = globalThis.crossOriginIsolated === true;
        if (isolated) {
            clearFlag();
            // Isolation came from the server here rather than from the worker, but the worker
            // is also what serves the game offline and what drives the update prompt, so it
            // still has to be installed. Looking up an existing registration instead meant a
            // first visit to a host that sends the headers itself never got one. No reload is
            // needed, unlike the path below: the headers are already right.
            const registration = "serviceWorker" in navigator
                ? registerWorker()
                : Promise.resolve(null);
            // Handled here as well as by pwa.js. The page is already isolated, so a failed
            // registration costs offline caching and nothing else - but an unhandled rejection
            // would reach the boot error screen and take the whole session with it.
            registration.catch((error) =>
                console.warn("service worker registration failed:", error),
            );
            globalThis.ctrdxServiceWorkerRegistration = registration;
            finishReady(true);
            return;
        }

        if (!("serviceWorker" in navigator)) {
            globalThis.ctrdxServiceWorkerRegistration = Promise.resolve(null);
            finishReady(false);
            return;
        }

        if (readFlag() === "1") {
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

            // Without a recorded flag the next load registers, reloads, and arrives back
            // here to do it again. Continuing unisolated shows the player an error they can
            // act on; a reload loop shows them nothing at all, so an unrecordable reload is
            // one this stands down from.
            if (!writeFlag()) {
                finishReady(false);
                return;
            }

            reloadRequested = true;
            globalThis.location.replace(globalThis.location.href);
        };

        const registrationPromise = registerWorker();
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
                if (
                    registration.active &&
                    !navigator.serviceWorker.controller
                ) {
                    globalThis.clearTimeout(controllerTimeout);
                    reloadForIsolation();
                }
            })
            .catch((error) => {
                globalThis.clearTimeout(controllerTimeout);
                console.warn(
                    "cross-origin isolation registration failed:",
                    error,
                );
                finishReady(false);
            });
    }
})();
