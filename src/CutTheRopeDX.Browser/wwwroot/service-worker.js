// Migration bridge for clients whose existing registration still checks this
// URL. New documents register coi-sw.js directly, but removing this script would
// strand an old worker behind its cached index forever.
importScripts("./coi-sw.js");

self.addEventListener("install", () => {
    self.skipWaiting();
});
