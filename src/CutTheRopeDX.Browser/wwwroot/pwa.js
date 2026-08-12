// Service worker registration and the update prompt.
//
// A worker that finds new assets installs alongside the running one and then waits, so a
// version never swaps in underneath a session in progress. The dialog is how the player asks
// for it: the waiting worker takes over and the page reloads onto the new build.

const UPDATE_CHECK_INTERVAL_MS = 15 * 60 * 1000;

if ("serviceWorker" in navigator) {
    // updateViaCache: "none" keeps the HTTP cache away from the worker script itself. It is
    // the one file whose freshness decides whether an update is ever noticed.
    navigator.serviceWorker
        .register("./service-worker.js", { updateViaCache: "none" })
        .then(watch)
        .catch((error) =>
            console.warn("service worker registration failed:", error),
        );
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
