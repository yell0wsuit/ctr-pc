// Offline cache for the published game.
//
// service-worker-assets.js is generated at publish by the static web assets SDK: every asset
// with its integrity hash, plus a version derived from those hashes.
//
// Storage is split in two, because the runtime and the game content age differently.
//
//   Shell cache   — the .NET runtime and the page shell, named after the manifest version and
//                   replaced wholesale when it changes. Those files carry a fingerprint in
//                   their own names, so a new build requests new URLs regardless and keeping
//                   the old ones would only waste space.
//   Content cache — the ~36MB under content/, kept across versions. These URLs are stable, so
//                   a cached entry stays valid until its bytes actually change. Each entry
//                   records the manifest hash it was stored from, and activation drops only
//                   the entries whose hash no longer matches. A publish that touches nothing
//                   but code therefore re-downloads nothing here.
//
// The worker and the web manifest are deliberately absent from both. The browser revalidates
// this file on every navigation, which is the only thing that notices a new deployment;
// serving it from a cache it controls would leave it unable to replace itself.

self.importScripts("./service-worker-assets.js");

const cachePrefix = "ctrdx-";
const shellCacheName = `${cachePrefix}shell-${self.assetsManifest.version}`;
const contentCacheName = `${cachePrefix}content`;

// Records on each cached content entry the manifest hash it was stored from, so a later
// activation can tell a still-current file from a changed one without rehashing 36MB.
const hashHeader = "x-ctrdx-asset-hash";

// Relative to the worker, so the app works from a domain root or a project subpath alike.
const scopeUrl = new URL("./", self.location.href);

// The runtime and page shell are fetched once at startup and needed before anything can run,
// so they are pulled in at install. Content is not: the game requests all of it during its own
// loading screen, and precaching would download it a second time in parallel with that.
const shellExclude = [
    /^content\//,
    /^service-worker(-assets)?\.js$/,
    /^manifest\.webmanifest$/,
    /\.pdb$/,
    /\.map$/,
];

const shellAssets = self.assetsManifest.assets.filter(
    (asset) => !shellExclude.some((pattern) => pattern.test(asset.url)),
);
const shellUrls = new Set(
    shellAssets.map((asset) => new URL(asset.url, scopeUrl).href),
);
const contentHashes = new Map(
    self.assetsManifest.assets
        .filter((asset) => asset.url.startsWith("content/"))
        .map((asset) => [new URL(asset.url, scopeUrl).href, asset.hash]),
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
    const requests = shellAssets.map(
        (asset) =>
            new Request(new URL(asset.url, scopeUrl), {
                integrity: asset.hash,
                cache: "no-cache",
            }),
    );
    const cache = await caches.open(shellCacheName);
    await cache.addAll(requests);
}

async function onActivate() {
    const keys = await caches.keys();
    await Promise.all(
        keys
            .filter(
                (key) =>
                    key.startsWith(cachePrefix) &&
                    key !== shellCacheName &&
                    key !== contentCacheName,
            )
            .map((key) => caches.delete(key)),
    );

    await pruneChangedContent();

    // Claim the page that registered this worker so its content requests are cached during the
    // very first visit rather than only from the second one onwards.
    await self.clients.claim();
}

/**
 * Drops content entries this publish changed or no longer ships, keeping the rest.
 */
async function pruneChangedContent() {
    const cache = await caches.open(contentCacheName);
    const requests = await cache.keys();
    await Promise.all(
        requests.map(async (request) => {
            const expected = contentHashes.get(request.url);
            const cached = await cache.match(request);
            // An entry with no recorded hash predates this scheme, so its bytes cannot be
            // vouched for and it goes too.
            if (
                expected === undefined ||
                cached?.headers.get(hashHeader) !== expected
            ) {
                await cache.delete(request);
            }
        }),
    );
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

    const contentHash = contentHashes.get(request.url);
    if (contentHash !== undefined) {
        return serveContent(request, contentHash);
    }

    if (shellUrls.has(request.url)) {
        return serveShell(request);
    }

    return fetch(request);
}

/**
 * Serves a content asset, filling the long-lived content cache on the way through.
 *
 * @param {Request} request
 * @param {string} hash Manifest hash to record against the stored entry.
 */
async function serveContent(request, hash) {
    // Media elements ask for byte ranges. A cache match is by URL alone, so a stored full
    // response would answer a range request with the whole file, which Safari refuses to
    // play. Ranges go straight to the network; a whole-file request still fills the cache.
    if (request.headers.has("range")) {
        return fetch(request);
    }

    const cache = await caches.open(contentCacheName);
    const cached = await cache.match(request);
    if (cached) {
        return cached;
    }

    const response = await fetch(request);
    if (response.ok && response.status === 200) {
        await cache.put(request, withHash(response.clone(), hash));
    }
    return response;
}

/**
 * Serves a shell asset. Install normally has these already; the fetch path covers a failed
 * install, which is all-or-nothing and would otherwise leave the cache empty for good.
 *
 * @param {Request} request
 */
async function serveShell(request) {
    const cache = await caches.open(shellCacheName);
    const cached = await cache.match(request);
    if (cached) {
        return cached;
    }

    const response = await fetch(request);
    if (response.ok && response.status === 200) {
        await cache.put(request, response.clone());
    }
    return response;
}

/**
 * Copies a response, tagging it with the manifest hash its bytes came from.
 *
 * @param {Response} response
 * @param {string} hash
 */
function withHash(response, hash) {
    const headers = new Headers(response.headers);
    headers.set(hashHeader, hash);
    return new Response(response.body, {
        status: response.status,
        statusText: response.statusText,
        headers,
    });
}
