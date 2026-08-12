// Offline cache for the published game.
//
// service-worker-assets.js is generated at publish by the static web assets SDK: every asset
// with its integrity hash, plus a version derived from those hashes. The version names the
// cache, so changing any asset produces a new cache and retires the old one on activation.
//
// The worker and the web manifest are deliberately absent from the cache. The browser
// revalidates this file on every navigation, which is the only thing that notices a new
// deployment; serving it from a cache it controls would leave it unable to replace itself.

self.importScripts("./service-worker-assets.js");

const cachePrefix = "ctrdx-offline-";
const cacheName = `${cachePrefix}${self.assetsManifest.version}`;

// Relative to the worker, so the app works from a domain root or a project subpath alike.
const scopeUrl = new URL("./", self.location.href);

// The runtime and the page shell are fetched once at startup and needed before anything can
// run, so they are pulled in at install. The 38MB of content under content/ is not: the game
// requests all of it during its own loading screen, and precaching would download it a second
// time in parallel with that.
const precacheExclude = [
    /^content\//,
    /^service-worker(-assets)?\.js$/,
    /^manifest\.webmanifest$/,
    /\.pdb$/,
    /\.map$/,
];

const cacheableUrls = new Set(
    self.assetsManifest.assets.map(
        (asset) => new URL(asset.url, scopeUrl).href,
    ),
);

self.addEventListener("install", (event) => event.waitUntil(onInstall()));
self.addEventListener("activate", (event) => event.waitUntil(onActivate()));
self.addEventListener("fetch", (event) => event.respondWith(onFetch(event)));

// The page asks for this once the player accepts the update prompt. Until then a new worker
// waits, so a version never changes underneath a session in progress.
self.addEventListener("message", (event) => {
    if (event.data?.type === "skip-waiting") {
        self.skipWaiting();
    }
});

async function onInstall() {
    const requests = self.assetsManifest.assets
        .filter(
            (asset) =>
                !precacheExclude.some((pattern) => pattern.test(asset.url)),
        )
        .map(
            (asset) =>
                new Request(new URL(asset.url, scopeUrl), {
                    integrity: asset.hash,
                    cache: "no-cache",
                }),
        );
    const cache = await caches.open(cacheName);
    await cache.addAll(requests);
}

async function onActivate() {
    const keys = await caches.keys();
    await Promise.all(
        keys
            .filter((key) => key.startsWith(cachePrefix) && key !== cacheName)
            .map((key) => caches.delete(key)),
    );

    // Claim the page that registered this worker so its content requests are cached during the
    // very first visit rather than only from the second one onwards.
    await self.clients.claim();
}

async function onFetch(event) {
    const request = event.request;
    if (request.method !== "GET") {
        return fetch(request);
    }

    // A navigation to any in-scope URL is this single page.
    if (request.mode === "navigate") {
        const cached = await caches.match(new URL("index.html", scopeUrl));
        return cached ?? fetch(request);
    }

    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    if (cached) {
        return cached;
    }

    const response = await fetch(request);
    // Only assets this build published are stored, and only when the server gave a usable
    // answer — caching an error or a range response would poison the next visit.
    if (
        response.ok &&
        response.status === 200 &&
        cacheableUrls.has(request.url)
    ) {
        await cache.put(request, response.clone());
    }
    return response;
}
