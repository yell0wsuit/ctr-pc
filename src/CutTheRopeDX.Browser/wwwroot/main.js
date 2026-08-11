import { dotnet } from "./_framework/dotnet.js";

const progress = document.getElementById("splash-progress");

const reportDownloadProgress = (loaded, total) => {
    if (progress !== null) {
        progress.textContent = `Loading ${loaded} of ${total}…`;
    }
};

const builder = dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery();

// Probed rather than called outright: losing the counter is cosmetic, but throwing
// here would cost the whole app its boot.
if (typeof builder.withModuleConfig === "function") {
    builder.withModuleConfig({
        onDownloadResourceProgress: reportDownloadProgress,
    });
}

const runtime = await builder.create();
const config = runtime.getConfig();
globalThis.ctrdxWasmModule = runtime.Module;
await runtime.runMain(config.mainAssemblyName, []);
