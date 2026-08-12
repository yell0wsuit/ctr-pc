// Development build: no offline support, so a stale cache can never shadow a rebuild.
// The published build swaps in service-worker.published.js.
self.addEventListener("fetch", () => {});
